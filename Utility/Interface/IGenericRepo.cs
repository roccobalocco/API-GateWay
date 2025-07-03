namespace Utility.Interface;

public interface IGenericRepo<T>
{
    public Task<List<T>> GetAllAsync();

    public Task<T?> GetByIdAsync(int id);

    public Task<T> AddAsync(T book);

    public Task<T?> UpdateAsync(int id, T book);

    public Task<bool> DeleteAsync(int id);
}