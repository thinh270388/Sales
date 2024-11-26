using Sales.Models;

namespace Sales.Services.Constracts
{
    public interface IGenericService<T>
    {
        Task<DtoResult<T>> GetAllAsync(T dto);
        Task<DtoResult<T>> GetOneAsync(T dto);
        Task<DtoResult<T>> FindAsync(T dto);
        Task<DtoResult<T>> AddAsync(T dto);
        Task<DtoResult<T>> UpdateAsync(T dto);
        Task<DtoResult<T>> DeleteAsync(T dto);
    }
}