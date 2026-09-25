using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Dashboard;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,HOD")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("departments")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<DepartmentReportDto>>>> GetDepartmentReport()
        {
            var result = await _reportService.GetDepartmentReportAsync();
            return Ok(result);
        }

        [HttpGet("sla")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<SlaReportDto>>>> GetSlaReport()
        {
            var result = await _reportService.GetSlaReportAsync();
            return Ok(result);
        }

        [HttpGet("trends")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<TrendsReportDto>>>> GetTrends()
        {
            var result = await _reportService.GetTrendsAsync();
            return Ok(result);
        }
    }
}
