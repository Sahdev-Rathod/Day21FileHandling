using Day21.DAL.Models;
using Day21.DTO;
using Day21.Repository;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Hosting;

namespace Day21.Service
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;
        private readonly IWebHostEnvironment _environment;

        public EmployeeService( IEmployeeRepository repository, IWebHostEnvironment environment)
        {
            _repository = repository;
            _environment = environment;
        }

        public async Task<List<EmployeeDto>> GetAllAsync()
        {
            var employees =  await _repository.GetAllAsync();

            return employees.Select(x => new EmployeeDto
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                Department = x.Department,
                Salary = x.Salary,
                ImageName = x.ImageName
            }).ToList();
        }

        public async Task<EmployeeDto?> GetByIdAsync(int id)
        {
            var employee =  await _repository.GetByIdAsync(id);

            if (employee == null)
                return null;

            return new EmployeeDto
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                Salary = employee.Salary,
                ImageName = employee.ImageName
            };
        }

        public async Task<EmployeeDto> AddAsync(EmployeeDto dto)
        {
            var employee = new Employee
            {
                Name = dto.Name,
                Email = dto.Email,
                Department = dto.Department,
                Salary = dto.Salary
            };

            var result = await _repository.AddAsync(employee);

            dto.Id = result.Id;

            return dto;
        }

        public async Task<bool> UploadImageAsync( int employeeId, IFormFile file)
        {
            var employee =  await _repository.GetByIdAsync(employeeId);

            if (employee == null)
                return false;

            if (file == null || file.Length == 0)
                return false;

            var extension =
                Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

            if (!allowedExtensions.Contains(extension))
                return false;

            if (file.Length > 5 * 1024 * 1024)
                return false;

            var uploadFolder = Path.Combine( _environment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";

            var filePath =
                Path.Combine(
                    uploadFolder,
                    fileName);

            using var stream =
                new FileStream(
                    filePath,
                    FileMode.Create);

            await file.CopyToAsync(stream);

            employee.ImageName = fileName;

            employee.ImagePath =
                Path.Combine(
                    "uploads",
                    fileName);

            await _repository.UpdateAsync(employee);

            return true;
        }

        public async Task<( byte[] Data, string ContentType, string FileName)?> DownloadImageAsync(int employeeId)
        {
            var employee =
                await _repository.GetByIdAsync(employeeId);

            if (employee == null ||
                string.IsNullOrEmpty(employee.ImageName))
            {
                return null;
            }

            var filePath =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    employee.ImageName);

            if (!File.Exists(filePath))
                return null;

            var bytes =
                await File.ReadAllBytesAsync(filePath);

            var extension =
                Path.GetExtension(employee.ImageName)
                .ToLowerInvariant();

            var contentType = extension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            return (
                bytes,
                contentType,
                employee.ImageName);
        }

        public async Task<byte[]> GeneratePdfAsync()
        {
            var employees =
                await _repository.GetAllAsync();

            QuestPDF.Settings.License =
                LicenseType.Community;

            var document =
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(30);

                        page.Header()
                            .Text("Employee Report")
                            .FontSize(20)
                            .Bold();

                        page.Content()
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("ID");
                                    header.Cell().Text("Name");
                                    header.Cell().Text("Email");
                                    header.Cell().Text("Department");
                                    header.Cell().Text("Salary");
                                });

                                foreach (var employee in employees)
                                {
                                    table.Cell()
                                        .Text(employee.Id.ToString());

                                    table.Cell()
                                        .Text(employee.Name);

                                    table.Cell()
                                        .Text(employee.Email);

                                    table.Cell()
                                        .Text(employee.Department);

                                    table.Cell()
                                        .Text(employee.Salary.ToString("0.00"));
                                }
                            });
                    });
                });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateExcelAsync()
        {
            var employees =
                await _repository.GetAllAsync();

            ExcelPackage.License.SetNonCommercialPersonal("Sahdev Rathod");

            using var package =
                new ExcelPackage();

            var worksheet =
                package.Workbook.Worksheets
                    .Add("Employees");

            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "Name";
            worksheet.Cells[1, 3].Value = "Email";
            worksheet.Cells[1, 4].Value = "Department";
            worksheet.Cells[1, 5].Value = "Salary";

            int row = 2;

            foreach (var employee in employees)
            {
                worksheet.Cells[row, 1].Value =
                    employee.Id;

                worksheet.Cells[row, 2].Value =
                    employee.Name;

                worksheet.Cells[row, 3].Value =
                    employee.Email;

                worksheet.Cells[row, 4].Value =
                    employee.Department;

                worksheet.Cells[row, 5].Value =
                    employee.Salary;

                row++;
            }

            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        public async Task<byte[]> GenerateQrAsync( int employeeId)
        {
            var employee =
                await _repository.GetByIdAsync(employeeId);

            if (employee == null)
                return Array.Empty<byte>();

            var data =
                $"Employee ID: {employee.Id}\n" +
                $"Name: {employee.Name}\n" +
                $"Email: {employee.Email}\n" +
                $"Department: {employee.Department}";

            using var generator =
                new QRCodeGenerator();

            using var qrData =
                generator.CreateQrCode(
                    data,
                    QRCodeGenerator.ECCLevel.Q);

            var qrCode =
                new PngByteQRCode(qrData);

            return qrCode.GetGraphic(20);
        }
    }
}