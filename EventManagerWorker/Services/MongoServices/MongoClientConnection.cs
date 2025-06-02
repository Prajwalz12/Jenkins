using Domain.Settings;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagerWorker.Services.MongoServices
{
    public class MongoClientConnection
    {
        public readonly MongoClient client;
        //protected readonly IDatabaseSettings settings;
        public MongoClientConnection(IDatabaseSettings settings)
        {
            client = new MongoClient(settings.ConnectionString);
        }
    }
}
