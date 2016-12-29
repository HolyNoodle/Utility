using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HolyNoodle.Utility.DAL
{
    public class BaseCacheProvider : ICacheProvider
    {
        private ConcurrentDictionary<string, CacheContainer> CacheContainerMap;

        public BaseCacheProvider()
        {
            CacheContainerMap = new ConcurrentDictionary<string, CacheContainer>();
        }

        public void Cache(IDbProcedure key, object value)
        {
            var attribute = key.GetType().GetTypeInfo().GetCustomAttribute<CacheObject>();
            if(attribute != null)
            {
                foreach(var procedure in CacheContainerMap)
                {
                    if(attribute.IsImpacted(procedure.Value.Procedure))
                    {
                        procedure.Value.LastAccess = DateTime.MinValue;
                    }
                }
            }

            if (value == null || (attribute != null && attribute.DoNotCache)) return;

            var hash = GetHash(key);
            if (CacheContainerMap.ContainsKey(hash))
            {
                var container = CacheContainerMap[hash];
                container.LastAccess = DateTime.Now;
                container.Value = value;
            }
            else
            {
                var newContainer = new CacheContainer { LastAccess = DateTime.Now, Value = value, Procedure = key };
                var container = CacheContainerMap.GetOrAdd(hash, newContainer);
                if (container != null && container.LastAccess < newContainer.LastAccess)
                {
                    Cache(key, value);
                }
            }
        }

        public void Dispose()
        {
            CacheContainerMap = new ConcurrentDictionary<string, CacheContainer>();
        }

        public T Get<T>(IDbProcedure key)
        {
            var hash = GetHash(key);
            if (CacheContainerMap.ContainsKey(hash))
            {
                var container = CacheContainerMap[hash];
                var elapsedTime = DateTime.Now - container.LastAccess;
                if(elapsedTime.TotalMinutes >= 5)
                {
                    container.LastAccess = DateTime.MinValue;
                    return (T)(object)null;
                }

                return (T)container.Value;
            }
            return (T)(object)null;
        }

        private string GetHash(IDbProcedure procedure)
        {
            var hash = new StringBuilder(procedure.GetType().FullName.ToLower());
            foreach(var property in procedure.GetType().GetProperties())
            {
                if(property.GetCustomAttribute<DalAttribute>() != null)
                {
                    hash.Append("_");
                    hash.Append(property.GetValue(procedure).ToString().ToLower());
                }
            }
            return hash.ToString();
        }

        private class CacheContainer
        {
            public object Value { get; set; }
            public DateTime LastAccess { get; set; }
            public IDbProcedure Procedure { get; set; }
        }
    }
}
