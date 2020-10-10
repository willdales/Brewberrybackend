using backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace backend.Logic
{
    public class JsonStoreProvider :IStoreProvider
    {
        private string _connectionString;
        private string dataStoreLocation;

        public JsonStoreProvider(IConfiguration configuration)
        {
            dataStoreLocation = configuration["dataStore"];
        }

        public void SetStoreName(string storeName)
        {
            if (!storeName.EndsWith(".json"))
            {
                storeName += ".json";
            }
            this._connectionString = Path.Combine(dataStoreLocation,  storeName);
        }


        public void SaveStore<T>(List<T> collection)
        {
            if (String.IsNullOrEmpty(_connectionString))
            {
                throw new NullReferenceException("Store Connection String must be set");
            }
            using (StreamWriter file = File.CreateText(_connectionString))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, collection);
            }
        }

        public List<T> LoadStore<T>() where T : IStored
        {
            if (String.IsNullOrEmpty(_connectionString))
            {
                throw new NullReferenceException("Store Connection String must be set");
            }
            if (File.Exists(_connectionString))
            {

                using (StreamReader file = File.OpenText(_connectionString))
                {
                    
                    JsonSerializer serializer = new JsonSerializer();
                    
                    var theList = serializer.Deserialize(file, typeof(List<T>));

                    if (theList != null)
                    {

                        return (List<T>)theList;
                    }
                }

            }

            return new List<T>();
        }
    }
}
