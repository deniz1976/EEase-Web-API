using EEaseWebAPI.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Repositories
{
    public interface IWriteRepository<T> : IRepository<T> where T : BaseEntity
    {
        //return true for success
        Task<bool> AddAsync(T model);
        Task<bool> AddRangeAsync(List<T> datas);

        bool Remove(T model);
        Task<bool> RemoveAsync(string id);

        bool RemoveRange(List<T> datas);

        bool Update(T model);

        Task<int> SaveAsync();
    }
}
