using HolyNoodle.Utility.Dal;
using Qbox.Common.DAL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HolyNoodle.Utility.DAL
{
    public class AzureDb
    {
        private static string ConnectionString;
        private static bool UseCache;
        private static ICacheProvider CacheProvider;

        static AzureDb()
        {
            ConnectionString = "";
            if (ConfigurationManager.AppSettings["holynoodle:DefaultConnectionString"] != null)
            {
                ConnectionString = ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["holynoodle:DefaultConnectionString"]].ConnectionString;
            }
            UseCache = false;

            if (ConfigurationManager.AppSettings["holynoodle:CacheProvider"] != null)
            {
                UseCache = true;
                var typeName = ConfigurationManager.AppSettings["holynoodle:CacheProvider"];
                var type = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.FullName == typeName);
                if (type != null)
                {
                    try
                    {
                        CacheProvider = (ICacheProvider)Activator.CreateInstance(type);
                    }
                    catch
                    {
                        throw;
                    }
                }
                else
                {
                    throw new Exception("Type '" + ConfigurationManager.AppSettings["holynoodle:CacheProvider"] + "' not found.");
                }
            }
        }

        private string _connectionString;

        public AzureDb()
        {
            _connectionString = ConnectionString;
        }

        public AzureDb(string cs)
        {
            _connectionString = cs;
        }

        private SqlConnection Connect(int deph = 0)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            var attempt = 0;
            Exception connectionError = null;
            while (connection.State != ConnectionState.Open && attempt < 3)
            {
                try
                {
                    attempt++;
                    connection.Open();
                }
                catch (Exception e)
                {
                    connectionError = e;
                }
            }
            if (connection.State != ConnectionState.Open)
            {
                if (deph < 3)
                {
                    return Connect(deph + 1);
                }
                else
                {
                    throw new Exception("No connection could open", connectionError);
                }
            }

            return connection;
        }

        public void ExecuteNonQuery(IDbProcedure procedure)
        {
            using (var connection = Connect())
            {
                var command = CreateCommand(procedure);
                command.Connection = connection;
                command.ExecuteNonQuery();
            }
        }


        public object ExecuteScalar(IDbProcedure procedure)
        {
            if (UseCache)
            {
                var result = CacheProvider.Get<object>(procedure);
                if (result != null)
                {
                    return result;
                }
            }
            using (var connection = Connect())
            {
                var command = CreateCommand(procedure);
                command.Connection = connection;
                var result = command.ExecuteScalar();
                if (UseCache)
                {
                    CacheProvider.Cache(procedure, result);
                }
                return result;
            }
        }

        public List<T> Execute<T>(IDbProcedure procedure) where T : IDalObject
        {
            if (UseCache)
            {
                var result = CacheProvider.Get<List<T>>(procedure);
                if (result != null)
                {
                    return result;
                }
            }
            using (var connection = Connect())
            {
                var command = CreateCommand(procedure);
                var result = new ConcurrentBag<T>();
                var values = new List<List<DalObjectValue>>();
                command.Connection = connection;

                using (var reader = command.ExecuteReader())
                {
                    if (reader == null) throw new Exception("DataReader is null and can't be browse");

                    if (reader.GetSchemaTable() == null) return result.ToList();

                    var columnStructure = new List<string>();

                    foreach (DataRow r in reader.GetSchemaTable().Rows)
                    {
                        columnStructure.Add(r["ColumnName"].ToString());
                    }

                    while (reader.Read())
                    {
                        var value = new List<DalObjectValue>();
                        foreach (var c in columnStructure)
                        {
                            value.Add(new DalObjectValue
                            {
                                Key = c,
                                Value = reader.GetValue(reader.GetOrdinal(c))
                            });
                        }
                        values.Add(value);
                    }
                }

                Parallel.ForEach(values, (v) =>
                {
                    result.Add(Load<T>(v));
                });

                if (UseCache)
                {
                    CacheProvider.Cache(procedure, result.ToList());
                }
                return result.ToList();
            }
        }

        private SqlCommand CreateCommand(IDbProcedure procedure)
        {
            if (procedure is IDbValidator)
            {
                var result = ((IDbValidator)procedure).Validate(this);
                if (!result.Item1)
                {
                    throw new AzureDbValidationException(result.Item2);
                }
            }

            var command = new SqlCommand()
            {
                CommandText = procedure.Procedure,
                CommandType = System.Data.CommandType.StoredProcedure,
            };

            var parameters = procedure.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (parameters != null && parameters.Count() > 0)
            {
                foreach (var property in parameters)
                {
                    var attributeTab = property.GetCustomAttributes(typeof(DalAttribute), false);
                    DalAttribute attribute = null;
                    foreach (var a in attributeTab)
                    {
                        if (a is DalAttribute)
                        {
                            attribute = (DalAttribute)a;
                            break;
                        }
                    }
                    if (attribute == null) continue;

                    object value = property.GetValue(procedure);
                    if (attribute.Crypter != null)
                    {
                        var instance = Activator.CreateInstance(attribute.Crypter) as ICrypter;
                        value = instance.Crypt(value);
                    }

                    command.Parameters.Add(new SqlParameter(attribute.DBName, value));
                }
            }

            return command;
        }
        private T Load<T>(List<DalObjectValue> values) where T : IDalObject
        {
            T result = (T)Activator.CreateInstance(typeof(T));
            var type = typeof(T);

            Parallel.ForEach(type.GetProperties(), property =>
            {
                var attributeTab = property.GetCustomAttributes(typeof(DalAttribute), false);
                DalAttribute attribute = null;
                foreach (var a in attributeTab)
                {
                    if (a is DalAttribute)
                    {
                        attribute = (DalAttribute)a;
                        break;
                    }
                }
                if (attribute != null)
                {
                    var value = values.FirstOrDefault(v => v.Key == attribute.DBName);
                    if (value != null && value.Value != DBNull.Value)
                    {
                        var objectValue = value.Value;
                        if (attribute.Crypter != null)
                        {
                            var instance = Activator.CreateInstance(attribute.Crypter) as ICrypter;
                            objectValue = instance.Decrypt(objectValue);
                        }
                        property.SetValue(result, objectValue);
                    }
                }
                var binder = property.GetCustomAttributes<ProcedureBinderAttribute>().FirstOrDefault();
                if (binder != null && binder.AutoBind)
                {
                    RefreshPropertyBinding(result, property, binder).Wait();
                }
            });

            return result;
        }

        public async Task<bool> RefreshAllBindings(List<IDalObject> objects)
        {
            try
            {
                var tasks = objects.Select(async o => await RefreshBindings(o));
                await Task.WhenAll(tasks);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task<bool> RefreshBindings(List<IDalObject> objects, string name)
        {
            try
            {
                var tasks = objects.Select(async o => await RefreshBinding(o, name));
                await Task.WhenAll(tasks);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task<bool> RefreshBindings(IDalObject o)
        {
            var tasks = o.GetType().GetProperties().Select(async property => await RefreshPropertyBinding(o, property));
            await Task.WhenAll(tasks);
            return true;
        }

        public async Task<bool> RefreshBinding(IDalObject o, string propertyName)
        {
            return await RefreshPropertyBinding(o, o.GetType().GetProperty(propertyName));
        }

        private async Task<bool> RefreshPropertyBinding(IDalObject o, PropertyInfo property, ProcedureBinderAttribute binder = null)
        {
            if(binder == null) binder = property.GetCustomAttributes<ProcedureBinderAttribute>().FirstOrDefault();

            if (binder.BindedSourcePropertyName != string.Empty && binder.BindedProcedure != null && binder.BindedProcedurePropertyName != string.Empty)
            {
                var linkProcedure = (IDbProcedure)Activator.CreateInstance(binder.BindedProcedure);
                var linkProperty = binder.BindedProcedure.GetProperty(binder.BindedProcedurePropertyName);
                var sourceProperty = o.GetType().GetProperty(binder.BindedSourcePropertyName);
                var propertyType = property.PropertyType;
                if (IsGenericList(propertyType))
                {
                    propertyType = propertyType.GenericTypeArguments.First();
                }
                linkProperty.SetValue(linkProcedure, sourceProperty.GetValue(o));

                var function = new Func<Type, IDbProcedure, Task<dynamic>>(async (type, proc) =>
                {
                    dynamic result = typeof(AzureDb)
                        .GetMethod("Execute")
                        .MakeGenericMethod(type)
                        .Invoke(this, new object[] { proc });
                    return result;
                });

                var linkedResult = await function(propertyType, linkProcedure);
                if (IsGenericList(property.PropertyType))
                {
                    property.SetValue(o, linkedResult);
                }
                else
                {
                    if (linkedResult.Count > 0)
                    {
                        property.SetValue(o, linkedResult[0]);
                    }
                }
            }
            return true;
        }

        private bool IsGenericList(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            foreach (Type @interface in type.GetInterfaces())
            {
                if (@interface.IsGenericType)
                {
                    if (@interface.GetGenericTypeDefinition() == typeof(ICollection<>))
                    {
                        // if needed, you can also return the type used as generic argument
                        return true;
                    }
                }
            }
            return false;
        }

        public void ResetCache(IDbProcedure procedure)
        {
            if(CacheProvider != null)
            {
                CacheProvider.Cache(procedure, null);
            }
        }
    }
}