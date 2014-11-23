using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;

namespace Dewadulator.Classes
{
    static class DataCache
    {
        readonly static SqlClientData MyData = new SqlClientData();
        private static readonly CacheItemPolicy cip  =  new CacheItemPolicy() { AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddDays(1)) };
        private static readonly object cacheLock = new object();
        private static readonly string nullstring = "%!NULL%!";

        public static int GetRowCount(string ServerName, string DataBaseName, string TableName)
        {
            string sqlConn = "Data Source=" + ServerName + ";Database=" + DataBaseName + ";Integrated Security=true;";
            string sql = "SELECT COUNT(*) FROM " + TableName + ";";
            SqlClientData ThisData = new SqlClientData(sqlConn);

            string CacheKey = String.Concat(sqlConn,"-GetRowCount-", TableName);

            var cachedData = MemoryCache.Default.Get(CacheKey, null) ;
            if (cachedData != null)
            {
                return (int)cachedData;
            }

            lock (cacheLock)
            {
                //Check to see if anyone wrote to the cache while we where waiting our turn to write the new value.
                cachedData = MemoryCache.Default.Get(CacheKey, null);

                if (cachedData != null)
                {
                    return (int)cachedData;
                }

                //The value still did not exist so we now write it in to the cache.
                var expensiveValue = (int)ThisData.TextScalar(sql);

                MemoryCache.Default.Set(CacheKey, expensiveValue, cip);
                return expensiveValue;
            }
        }
        public static string GetNewText(string OldText)
        {
            if (OldText == null)
                   return null;

            string CacheKey = String.Concat("GetNewText-", OldText);

            string cachedData = MemoryCache.Default.Get(CacheKey, null) as string;
            if (cachedData != null)
            {
                if (cachedData == nullstring)
                    return null;
                else
                    return cachedData;
            }

            lock (cacheLock)
            {
                //Check to see if anyone wrote to the cache while we where waiting our turn to write the new value.
                cachedData = cachedData = MemoryCache.Default.Get(CacheKey, null) as string;

                if (cachedData != null)
                {
                    if (cachedData == nullstring)
                        return null;
                    else
                        return cachedData;
                }

                //The value still did not exist so we now write it in to the cache.
                var expensiveString =  (string)MyData.TextScalar("SELECT TOP 1 NewText FROM dbo.BetterSQLNames WHERE OldText = @A", "@A", OldText);

                if (expensiveString  == null)
                {
                    expensiveString = nullstring;
                }

                MemoryCache.Default.Set(CacheKey, expensiveString, cip);
                if (expensiveString == nullstring)
                    return null;
                else
                    return expensiveString;
            }
        }
        public static void InvalidateCache(string OldText)
        {
            //
            CacheItemPolicy cip = new CacheItemPolicy() { AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddDays(-1)) };

            MemoryCache.Default.Set(OldText, OldText, cip);
             
        }
    }
}
