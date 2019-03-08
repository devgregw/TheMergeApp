using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MergeApi.Framework.Interfaces.Receivers {
    public interface IDatabaseReceiver {
        Task<T> GetFromIdAsync<T>(string id) where T:  IIdentifiable;

        Task<IEnumerable<T>> GetAllAsync<T>() where T : IIdentifiable;

        Task<IEnumerable<T>> GetAllAsync<T>(Func<T, bool> selector) where T : IIdentifiable;

        Task DeleteAsync<T>(T item) where T : IIdentifiable;

        Task UpdateAsync<T>(T item) where T : IIdentifiable;
    }
}