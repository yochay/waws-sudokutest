using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace AzureWebSitesTraceListener
{
    public class StorageTraceListener : StorageTraceListenerBase
    {
        private static StorageTraceListener instance;
        static object instanceLock = new object();

        public static StorageTraceListener Instance
        {
            get
            {
                if (instance != null) return instance;

                lock (instanceLock)
                {
                    instance = instance ?? new StorageTraceListener();
                }

                return instance;
            }
        }

        private StorageTraceListener() : base(ConfiguredAccountName, ConfiguredAccountKey, ConfiguredTableName, ConfiguredPartitionKey, ConfiguredFlushTimeoutInMilliseconds) { }

        private static string ConfiguredAccountName
        {
            get
            {
                return ConfigurationManager.AppSettings["StorageTraceListenerAccountName"];
            }
        }

        private static string ConfiguredAccountKey
        {
            get
            {
                return ConfigurationManager.AppSettings["StorageTraceListenerAccountKey"];
            }
        }


        private static string ConfiguredTableName
        {
            get
            {
                return ConfigurationManager.AppSettings["StorageTraceListenerTableName"];
            }
        }

        private static string ConfiguredPartitionKey
        {
            get
            {
                return ConfigurationManager.AppSettings["StorageTraceListenerPartitionKey"];
            }
        }

        private static int ConfiguredFlushTimeoutInMilliseconds
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["StorageTraceListenerFlushTimeoutInMilliseconds"]);
            }
        }
    }
}
