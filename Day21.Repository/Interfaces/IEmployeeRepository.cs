using Day21.DAL.Models;

namespace Day21.Repository
{
    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllAsync();

        Task<Employee?> GetByIdAsync(int id);

        Task<Employee> AddAsync(Employee employee);

        Task UpdateAsync(Employee employee);
    }
}