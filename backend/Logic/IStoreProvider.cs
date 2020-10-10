using backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Logic
{
    public interface IStoreProvider
    {
        void SaveStore<T>(List<T> collection);
        List<T> LoadStore<T>() where T : IStored;

        void SetStoreName(string storeName);
    }
}
