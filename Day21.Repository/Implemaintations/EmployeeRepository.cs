using Day21.DAL;
using Day21.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace Day21.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly Day21DBContext _context;

        public EmployeeRepository(
            Day21DBContext context)
        {
            _context = context;
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _context.Employees
                .ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Employee> AddAsync(
            Employee employee)
        {
            _context.Employees.Add(employee);

            await _context.SaveChangesAsync();

            return employee;
        }

        public async Task UpdateAsync(
            Employee employee)
        {
            _context.Employees.Update(employee);

            await _context.SaveChangesAsync();
        }
    }
}