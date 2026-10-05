using Day21.DTO;
using Day21.Service;
using Microsoft.AspNetCore.Mvc;

namespace Day21.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _service;

        public EmployeeController(
            IEmployeeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result =  await _service.GetAllAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById( int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);

                if (result == null)
                    return NotFound(
                        "Employee not found.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode( 500, $"Internal server error: {ex.Message}");
            }

        }

        [HttpPost]
        public async Task<IActionResult> Add( EmployeeDto dto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _service.AddAsync(dto);

                    return Ok(result);
                }
                else
                {
                    return BadRequest("Employee is null");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{id}/upload-image")]
        public async Task<IActionResult> UploadImage( int id, IFormFile file)
        {
            var result =
                await _service.UploadImageAsync(
                    id,
                    file);

            if (!result)
                return BadRequest(
                    "Image upload failed.");

            return Ok(
                "Image uploaded successfully.");
        }

        [HttpGet("{id}/image")]
        public async Task<IActionResult> DownloadImage(int id)
        {
            var result =
                await _service
                    .DownloadImageAsync(id);

            if (result == null)
                return NotFound(
                    "Image not found.");

            return File(
                result.Value.Data,
                result.Value.ContentType,
                result.Value.FileName);
        }

        [HttpGet("pdf")]
        public async Task<IActionResult> Pdf()
        {
            var result =
                await _service.GeneratePdfAsync();

            return File(
                result,
                "application/pdf",
                "EmployeeReport.pdf");
        }

        [HttpGet("excel")]
        public async Task<IActionResult> Excel()
        {
            var result =
                await _service.GenerateExcelAsync();

            return File(
                result,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "EmployeeReport.xlsx");
        }

        [HttpGet("{id}/qr")]
        public async Task<IActionResult> Qr(
            int id)
        {
            var result =
                await _service.GenerateQrAsync(id);

            if (result.Length == 0)
                return NotFound(
                    "Employee not found.");

            return File(
                result,
                "image/png",
                $"Employee_{id}_QR.png");
        }
    }
}