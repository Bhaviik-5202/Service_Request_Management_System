using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.DTOs.Masters;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MastersController : ControllerBase
    {
        private readonly IMasterService _masterService;

        public MastersController(IMasterService masterService)
        {
            _masterService = masterService;
        }

        // ─────────────── STATUSES ───────────────

        [HttpGet("statuses")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<StatusDto>>>> GetStatuses()
        {
            var result = await _masterService.GetStatusesAsync();
            return Ok(result);
        }

        [HttpGet("statuses/{id}")]
        public async Task<ActionResult<ApiResponseDto<StatusDto>>> GetStatusById(int id)
        {
            var result = await _masterService.GetStatusByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("statuses")]
        public async Task<ActionResult<ApiResponseDto<StatusDto>>> CreateStatus([FromBody] StatusDto dto)
        {
            var result = await _masterService.CreateStatusAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("statuses/{id}")]
        public async Task<ActionResult<ApiResponseDto<StatusDto>>> UpdateStatus(int id, [FromBody] StatusDto dto)
        {
            var result = await _masterService.UpdateStatusAsync(id, dto);
            if (!result.Success)
            {
                if (result.Message == "Status not found.")
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("statuses/{id}")]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteStatus(int id)
        {
            var result = await _masterService.DeleteStatusAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        // ─────────────── DEPARTMENTS ───────────────

        [HttpGet("departments")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<DepartmentDto>>>> GetDepartments()
        {
            var result = await _masterService.GetDepartmentsAsync();
            return Ok(result);
        }

        [HttpGet("departments/{id}")]
        public async Task<ActionResult<ApiResponseDto<DepartmentDto>>> GetDepartmentById(int id)
        {
            var result = await _masterService.GetDepartmentByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("departments")]
        public async Task<ActionResult<ApiResponseDto<DepartmentDto>>> CreateDepartment([FromBody] DepartmentDto dto)
        {
            var result = await _masterService.CreateDepartmentAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("departments/{id}")]
        public async Task<ActionResult<ApiResponseDto<DepartmentDto>>> UpdateDepartment(int id, [FromBody] DepartmentDto dto)
        {
            var result = await _masterService.UpdateDepartmentAsync(id, dto);
            if (!result.Success)
            {
                if (result.Message == "Department not found.")
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("departments/{id}")]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteDepartment(int id)
        {
            var result = await _masterService.DeleteDepartmentAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        // ─────────────── PERSONNEL ───────────────

        [HttpGet("personnel")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<DepartmentPersonnelDto>>>> GetPersonnel([FromQuery] int departmentId)
        {
            var result = await _masterService.GetPersonnelByDepartmentAsync(departmentId);
            return Ok(result);
        }

        [HttpPost("personnel")]
        public async Task<ActionResult<ApiResponseDto<DepartmentPersonnelDto>>> AddPersonnel([FromBody] DepartmentPersonnelDto dto)
        {
            var result = await _masterService.AddPersonnelAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("personnel/{id}")]
        public async Task<ActionResult<ApiResponseDto<object>>> RemovePersonnel(int id)
        {
            var result = await _masterService.RemovePersonnelAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        // ─────────────── SERVICE TYPES ───────────────

        [HttpGet("service-types")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ServiceTypeDto>>>> GetServiceTypes()
        {
            var result = await _masterService.GetServiceTypesAsync();
            return Ok(result);
        }

        [HttpGet("service-types/{id}")]
        public async Task<ActionResult<ApiResponseDto<ServiceTypeDto>>> GetServiceTypeById(int id)
        {
            var result = await _masterService.GetServiceTypeByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("service-types")]
        public async Task<ActionResult<ApiResponseDto<ServiceTypeDto>>> CreateServiceType([FromBody] ServiceTypeDto dto)
        {
            var result = await _masterService.CreateServiceTypeAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("service-types/{id}")]
        public async Task<ActionResult<ApiResponseDto<ServiceTypeDto>>> UpdateServiceType(int id, [FromBody] ServiceTypeDto dto)
        {
            var result = await _masterService.UpdateServiceTypeAsync(id, dto);
            if (!result.Success)
            {
                if (result.Message == "Service type not found.")
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("service-types/{id}")]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteServiceType(int id)
        {
            var result = await _masterService.DeleteServiceTypeAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        // ─────────────── REQUEST TYPES ───────────────

        [HttpGet("request-types")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<RequestTypeDto>>>> GetRequestTypes([FromQuery] int? serviceTypeId = null)
        {
            var result = await _masterService.GetRequestTypesAsync(serviceTypeId);
            return Ok(result);
        }

        [HttpGet("request-types/{id}")]
        public async Task<ActionResult<ApiResponseDto<RequestTypeDto>>> GetRequestTypeById(int id)
        {
            var result = await _masterService.GetRequestTypeByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("request-types")]
        public async Task<ActionResult<ApiResponseDto<RequestTypeDto>>> CreateRequestType([FromBody] RequestTypeDto dto)
        {
            var result = await _masterService.CreateRequestTypeAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("request-types/{id}")]
        public async Task<ActionResult<ApiResponseDto<RequestTypeDto>>> UpdateRequestType(int id, [FromBody] RequestTypeDto dto)
        {
            var result = await _masterService.UpdateRequestTypeAsync(id, dto);
            if (!result.Success)
            {
                if (result.Message == "Request type not found.")
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("request-types/{id}")]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteRequestType(int id)
        {
            var result = await _masterService.DeleteRequestTypeAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        // ─────────────── TECHNICIAN MAPPINGS ───────────────

        [HttpGet("mappings")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<RequestTypeTechnicianMappingDto>>>> GetMappings([FromQuery] int requestTypeId)
        {
            var result = await _masterService.GetMappingsByRequestTypeAsync(requestTypeId);
            return Ok(result);
        }

        [HttpPost("mappings")]
        public async Task<ActionResult<ApiResponseDto<RequestTypeTechnicianMappingDto>>> AddMapping([FromBody] RequestTypeTechnicianMappingDto dto)
        {
            var result = await _masterService.AddMappingAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("mappings/{id}")]
        public async Task<ActionResult<ApiResponseDto<object>>> RemoveMapping(int id)
        {
            var result = await _masterService.RemoveMappingAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
    }
}
