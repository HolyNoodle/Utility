using HolyNoodle.Utility.Dal;
using Qbox.Common.DAL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace HolyNoodle.Utility.DAL
{
    public class AzureDb : IDb
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
                var type = Assembly.GetEntryAssembly().GetTypes().FirstOrDefault(t => t.FullName == typeName);
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
        private SqlConnection _connection;

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
            if (_connection != null)
            {
                //Maybe send a query "SELECT GETDATE()" to check the connection still works, To be sure, but would slow down the connection
                if (_connection.State == ConnectionState.Open) return _connection;
            }
            _connection = new SqlConnection(_connectionString);
            var attempt = 0;
            Exception connectionError = null;
            while (_connection.State != ConnectionState.Open && attempt < 3)
            {
                try
                {
                    attempt++;
                    _connection.Open();
                }
                catch (Exception e)
                {
                    connectionError = e;
                }
            }
            if (_connection.State != ConnectionState.Open)
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

            return _connection;
        }

        public void ExecuteNonQuery(IDbProcedure procedure)
        {
            var connection = Connect();
            var command = CreateCommand(procedure);
            command.Connection = connection;
            command.ExecuteNonQuery();
        }


        public object ExecuteScalar(IDbProcedure procedure)
        {
            if (UseCache)
            {
                var cacheResult = CacheProvider.Get<object>(procedure);
                if (cacheResult != null)
                {
                    return cacheResult;
                }
            }
            var connection = Connect();

            var command = CreateCommand(procedure);
            command.Connection = connection;
            var result = command.ExecuteScalar();
            if (UseCache)
            {
                CacheProvider.Cache(procedure, result);
            }
            return result;
        }

        public List<T> Execute<T>(IDbProcedure procedure) where T : IDalObject
        {
            if (UseCache)
            {
                var cacheResult = CacheProvider.Get<List<T>>(procedure);
                if (cacheResult != null)
                {
                    return cacheResult;
                }
            }
            var connection = Connect();

            var command = CreateCommand(procedure);
            var result = new ConcurrentBag<T>();
            var values = new List<List<DalObjectValue>>();
            command.Connection = connection;

            using (var reader = command.ExecuteReader())
            {
                if (reader == null) throw new Exception("DataReader is null and can't be browse");

                while (reader.Read())
                {
                    var value = new List<DalObjectValue>();
                    for(var i = 0; i < reader.FieldCount; ++i)
                    {
                        value.Add(new DalObjectValue
                        {
                            Key = reader.GetName(i),
                            Value = reader.GetValue(i)
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
            //Create the object
            var type = typeof(T);
            var result = Activator.CreateInstance(type) as T;

            if (result == null) throw new Exception("Type " + type.FullName + " could not be instanciate");

            //Parallel because I can set different properties of the same object at the same time
            Parallel.ForEach(type.GetProperties(), property =>
            {
                //Used faster method to get attribute instead of looping on all the attributes.
                //Was not logic to do that...
                var attribute = property.GetCustomAttributes<DalAttribute>().FirstOrDefault();
                if (attribute != null)
                {
                    var value = values.FirstOrDefault(v => v.Key == attribute.DBName);
                    //Check : value exist and is not null
                    if (value != null && value.Value != DBNull.Value)
                    {
                        var objectValue = value.Value;
                        if (attribute.Crypter != null)
                        {
                            //Convert to decrypted data
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

        public async Task<bool> RefreshAllBindings(IList<IDalObject> objects)
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

        public async Task<bool> RefreshBindings(IList<IDalObject> objects, string name)
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
            if (binder == null) binder = property.GetCustomAttributes<ProcedureBinderAttribute>().FirstOrDefault();

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
                if (@interface.IsGenericParameter)
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
            if (CacheProvider != null)
            {
                CacheProvider.Cache(procedure, null);
            }
        }
    }
}