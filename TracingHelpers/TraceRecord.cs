using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;

namespace AzureWebSitesTraceListener
{
    public class TraceRecord : TableServiceEntity 
    {
        public string Message { get; set; }
        public string Severity { get; set; }
        public string AdditionalData { get; set; }
        public string TraceTimestamp { get; set; }

        public TraceRecord(string partitionKey, DateTime timestamp, string rowKey = null)
        {
            TraceTimestamp = timestamp.ToString("yyyy-MM-dd hh:mm:ss.fffffff tt");
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = partitionKey;
        }

        public override string ToString()
        {
            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}", PartitionKey, RowKey, Severity, Message, AdditionalData);
        }
    }

    public class TraceContext : TableServiceContext
    {
        string tableName;
        public TraceContext(string tableName, string baseAddress, StorageCredentials credentials) : base(baseAddress, credentials)
        {
            this.tableName = tableName;
        }

        public void WriteTrace(TraceRecord trace)
        {
            this.AddObject(tableName, trace);
        }
    }
}
