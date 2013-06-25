using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace AzureWebSitesTraceListener
{
    public class StorageTraceListenerBase : TraceListener
    {
        Queue<TraceRecord> messages;
        Thread flushThread;
        CloudStorageAccount storageAccount;
        CloudTableClient storageClient;
        TraceContext context;
        string partitionKey;
        int flushTimeInMilliseconds;

        internal StorageTraceListenerBase(string storageAccountName, string storageAccountKey, string storageTableName, string partitionKey, int flushTimeInMilliseconds)
        {
            this.partitionKey = partitionKey;
            this.flushTimeInMilliseconds = flushTimeInMilliseconds;
            messages = new Queue<TraceRecord>();
            storageAccount = new CloudStorageAccount(new StorageCredentialsAccountAndKey(storageAccountName, storageAccountKey), true);
            storageClient = storageAccount.CreateCloudTableClient();
            storageClient.CreateTableIfNotExist(storageTableName);
            context = new TraceContext(storageTableName, storageClient.BaseUri.ToString(), storageClient.Credentials);
        }

        public void TraceRecord (TraceEventType type, string message, string partitionKey, string additionalData = null)
        {
            var record = new TraceRecord(partitionKey, DateTime.UtcNow)
            {
                Message = message,
                Severity = type.ToString(),
                AdditionalData = additionalData
            };
            QueueEvent(record);
        }

        public override void Write(string message)
        {
            var record = new TraceRecord(partitionKey,DateTime.UtcNow)
            {
                Message = message,
                Severity = TraceEventType.Verbose.ToString()
            };
            QueueEvent(record);
        }

        public override void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }

        protected void QueueEvent(TraceRecord record)
        {
            lock (messages)
            {
#if DEBUG
                Console.WriteLine("Queueing event: "+record);
#endif
                messages.Enqueue(record);
                if (flushTimeInMilliseconds > 0 && flushThread == null)
                {
                    flushThread = new Thread(Flush) { IsBackground = false, Priority = ThreadPriority.BelowNormal };
                    flushThread.Start();
                }
                else if (flushTimeInMilliseconds == 0)
                {
                    Flush();
                }
            }
        }

        public void Flush()
        {
            if(flushTimeInMilliseconds > 0) Thread.Sleep(flushTimeInMilliseconds);
            Queue<TraceRecord> toFlush = new Queue<TraceRecord>();
            lock (messages)
            {
                flushThread = null;
                while (messages.Count > 0) toFlush.Enqueue(messages.Dequeue());
            }

            if(toFlush.Count == 0) return;
            while (toFlush.Count > 0)
            {
                var record = toFlush.Dequeue();
#if DEBUG
                Console.WriteLine("Flushing event: " + record);
#endif
                context.WriteTrace(record);
            }
            context.SaveChanges();
        }
    }
}
