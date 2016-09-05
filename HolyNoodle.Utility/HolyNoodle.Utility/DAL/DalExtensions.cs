using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HolyNoodle.Utility.DAL
{
    public static class IDalObjectExtensions
    {
        public static AzureDb BindingDb { get; set; }

        public static async Task<bool> RefreshBinding(this IDalObject o, string name)
        {
            return await BindingDb.RefreshBinding(o, name);
        }
        public static async Task<bool> RefreshBindings(this IDalObject o)
        {
            return await BindingDb.RefreshBindings(o);
        }
        public static async Task<bool> RefreshBindings(this List<IDalObject> list, string name)
        {
            return await BindingDb.RefreshBindings(list, name);
        }
        public static async Task<bool> RefreshBindings(this List<IDalObject> list)
        {
            return await BindingDb.RefreshAllBindings(list);
        }
    }
}
