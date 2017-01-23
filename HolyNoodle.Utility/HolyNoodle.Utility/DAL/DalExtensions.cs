using FastMember;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
        public static DataTable ConvertToDataTable(this IEnumerable<IDalObject> list)
        {
            
                using (var table = new DataTable())
                {
                
                    table.Columns.Add("FirstName", typeof(string));
                    table.Columns.Add("LastName", typeof(string));
                    table.Columns.Add("IGG", typeof(string));

                    for (int i = 0; i < 100000; i++)
                    {
                        table.Rows.Add("Aure" + i, "lien" + i, "igg" + i);
                    }
                return table;
                }
                
        }
    }
}
