using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceRequestManagementSystem.API.DTOs.Assets;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetService _assetService;

        public AssetsController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<AssetResponseDto>>>> GetAssets()
        {
            var result = await _assetService.GetAssetsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<AssetResponseDto>>> GetAssetById(int id)
        {
            var result = await _assetService.GetAssetByIdAsync(id);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult<ApiResponseDto<AssetResponseDto>>> CreateAsset([FromBody] CreateAssetDto dto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _assetService.CreateAssetAsync(dto, ipAddress);
            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetAssetById), new { id = result.Data?.AssetId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult<ApiResponseDto<AssetResponseDto>>> UpdateAsset(int id, [FromBody] UpdateAssetDto dto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _assetService.UpdateAssetAsync(id, dto, ipAddress);
            if (!result.Success)
            {
                if (result.Message == "Asset not found.")
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult<ApiResponseDto<object>>> DeleteAsset(int id)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _assetService.DeleteAssetAsync(id, ipAddress);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPut("{id}/assign")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult<ApiResponseDto<AssetResponseDto>>> AssignAsset(int id, [FromBody] AssignAssetDto dto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _assetService.AssignAssetAsync(id, dto, ipAddress);
            if (!result.Success)
            {
                if (result.Message == "Asset not found.")
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
