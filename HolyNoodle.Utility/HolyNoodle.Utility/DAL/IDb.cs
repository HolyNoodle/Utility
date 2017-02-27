using System.Collections.Generic;
using System.Threading.Tasks;

namespace HolyNoodle.Utility.DAL
{
    public interface IDb
    {
        void ExecuteNonQuery(IDbProcedure procedure);
        object ExecuteScalar(IDbProcedure procedure);
        List<T> Execute<T>(IDbProcedure procedure) where T : IDalObject;

        Task ExecuteNonQueryAsync(IDbProcedure procedure);
        Task<object> ExecuteScalarAsync(IDbProcedure procedure);
        Task<List<T>> ExecuteAsync<T>(IDbProcedure procedure) where T : IDalObject;

        Task<bool> RefreshAllBindings(IList<IDalObject> objects);
        Task<bool> RefreshBindings(IList<IDalObject> objects, string name);
        Task<bool> RefreshBindings(IDalObject o);
        Task<bool> RefreshBinding(IDalObject o, string propertyName);
    }
}
