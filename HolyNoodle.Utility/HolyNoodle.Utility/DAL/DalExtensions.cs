using FastMember;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace HolyNoodle.Utility.DAL
{
    public static class IDalObjectExtensions
    {
        public static IDb BindingDb { get; set; }

        public static async Task<bool> RefreshBinding(this IDalObject o, string name)
        {
            return await BindingDb.RefreshBinding(o, name);
        }
        public static async Task<bool> RefreshBindings(this IDalObject o)
        {
            return await BindingDb.RefreshBindings(o);
        }
        public static async Task<bool> RefreshBindings(this IList<IDalObject> list, string name)
        {
            return await BindingDb.RefreshBindings(list, name);
        }
        public static async Task<bool> RefreshBindings(this IList<IDalObject> list)
        {
            return await BindingDb.RefreshAllBindings(list);
        }
        public static DataTable ConvertToDataTable(this IList<IDalObject> list)
        {
            DataTable table = new DataTable();
            using (var reader = ObjectReader.Create(list))
            {
                table.Load(reader);
            }
            return table;
        }
    }
}
