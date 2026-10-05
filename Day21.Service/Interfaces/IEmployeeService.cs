using Day21.DTO;
using Microsoft.AspNetCore.Http;

namespace Day21.Service
{
    public interface IEmployeeService
    {
        Task<List<EmployeeDto>> GetAllAsync();

        Task<EmployeeDto?> GetByIdAsync(int id);

        Task<EmployeeDto> AddAsync(EmployeeDto dto);

        Task<bool> UploadImageAsync(
            int employeeId,
            IFormFile file);

        Task<(byte[] Data,
            string ContentType,
            string FileName)?>
            DownloadImageAsync(int employeeId);

        Task<byte[]> GeneratePdfAsync();

        Task<byte[]> GenerateExcelAsync();

        Task<byte[]> GenerateQrAsync(int employeeId);
    }
}