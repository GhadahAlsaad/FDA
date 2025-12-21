using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetById(int id); 
        IQueryable<T> GetAll();  
        Task Insert(T entity);
        Task InsertRange(List<T> entity); 
        void Update(T entity);
        Task Delete(T entity); 
        Task DeleteRange(List<T> entity);
        Task<int> SaveChanges();
    }
}
