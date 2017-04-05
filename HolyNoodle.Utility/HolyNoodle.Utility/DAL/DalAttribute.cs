using System;
using System.Collections.Generic;
using System.Linq;

namespace HolyNoodle.Utility.DAL
{
    public class DalAttribute : Attribute
    {
        public DalAttribute()
        {
            DBName = null;
        }

        public DalAttribute(string dbName)
        {
            DBName = dbName;
        }

        public DalAttribute(string dbName, Type crypterType)
        {
            DBName = dbName;
            Crypter = crypterType;
        }

        public string DBName { get; set; }
        public Type Crypter { get; set; }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class ProcedureBinderAttribute : Attribute
    {
        public bool AutoBind { get; set; }
        public string BindedProcedurePropertyName { get; set; }
        public Type BindedProcedure { get; set; }
        public string BindedSourcePropertyName { get; set; }

        public ProcedureBinderAttribute()
        {
            AutoBind = false;
            BindedProcedurePropertyName = string.Empty;
            BindedProcedure = null;
            BindedSourcePropertyName = string.Empty;
        }
    }
}