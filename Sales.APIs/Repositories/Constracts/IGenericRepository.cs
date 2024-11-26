using Sales.Models;

namespace Sales.APIs.Repositories.Constracts
{
    public interface IGenericRepository<T>
    {
        DtoResult<T> GetAll();
        DtoResult<T> GetOne(T dto);
        DtoResult<T> Find(T dto);
        DtoResult<T> Add(T dto);
        DtoResult<T> Update(T dto);
        DtoResult<T> Delete(T dto);
    }
}