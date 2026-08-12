using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Assets;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;

namespace ServiceRequestManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/v1/assets")]
    public class AssetsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AssetsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET /api/v1/assets
        /// Get paginated asset inventory list with optional filters.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<AssetResponseDto>>>> GetAssets(
            [FromQuery] string? category,
            [FromQuery] AssetStatus? status,
            [FromQuery] int? departmentId,
            [FromQuery] int? assignedToUserId,
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Assets
                .Include(a => a.Department)
                .Include(a => a.AssignedTo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(a => a.Category.ToLower() == category.Trim().ToLower());

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            if (departmentId.HasValue)
                query = query.Where(a => a.DepartmentId == departmentId.Value);

            if (assignedToUserId.HasValue)
                query = query.Where(a => a.AssignedToUserId == assignedToUserId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(a => a.AssetTag.ToLower().Contains(s) ||
                                          a.AssetName.ToLower().Contains(s) ||
                                          a.SerialNumber.ToLower().Contains(s));
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AssetResponseDto
                {
                    AssetId = a.AssetId,
                    AssetTag = a.AssetTag,
                    AssetName = a.AssetName,
                    Category = a.Category,
                    SerialNumber = a.SerialNumber,
                    AssignedTo = a.AssignedTo != null ? a.AssignedTo.FullName : null,
                    AssignedToUserId = a.AssignedToUserId,
                    Department = a.Department != null ? a.Department.DepartmentName : null,
                    DepartmentId = a.DepartmentId,
                    Status = a.Status,
                    PurchaseDate = a.PurchaseDate,
                    WarrantyUntil = a.WarrantyUntil,
                    BookValue = a.BookValue
                })
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<AssetResponseDto>>
            {
                Success = true,
                Message = "Assets fetched successfully.",
                Data = items,
                Pagination = new PaginationMetadataDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords
                }
            });
        }

        /// <summary>
        /// GET /api/v1/assets/{id}
        /// Get detailed asset specifications by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<AssetResponseDto>>> GetAssetById(int id)
        {
            var a = await _context.Assets
                .Include(ast => ast.Department)
                .Include(ast => ast.AssignedTo)
                .FirstOrDefaultAsync(ast => ast.AssetId == id);

            if (a == null)
                return NotFound(new ApiResponseDto<AssetResponseDto> { Success = false, Message = "Asset not found." });

            var response = new AssetResponseDto
            {
                AssetId = a.AssetId,
                AssetTag = a.AssetTag,
                AssetName = a.AssetName,
                Category = a.Category,
                SerialNumber = a.SerialNumber,
                AssignedTo = a.AssignedTo?.FullName,
                AssignedToUserId = a.AssignedToUserId,
                Department = a.Department?.DepartmentName,
                DepartmentId = a.DepartmentId,
                Status = a.Status,
                PurchaseDate = a.PurchaseDate,
                WarrantyUntil = a.WarrantyUntil,
                BookValue = a.BookValue
            };

            return Ok(new ApiResponseDto<AssetResponseDto> { Success = true, Data = response });
        }

        /// <summary>
        /// POST /api/v1/assets
        /// Create a new asset item in inventory.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<AssetResponseDto>>> CreateAsset([FromBody] CreateAssetDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDto<AssetResponseDto> { Success = false, Message = "Validation failed." });

            var tagExists = await _context.Assets.AnyAsync(a => a.AssetTag == dto.AssetTag || a.SerialNumber == dto.SerialNumber);
            if (tagExists)
                return BadRequest(new ApiResponseDto<AssetResponseDto> { Success = false, Message = "Asset tag or serial number already exists." });

            var asset = new Asset
            {
                AssetTag = dto.AssetTag,
                AssetName = dto.AssetName,
                Category = dto.Category,
                SerialNumber = dto.SerialNumber,
                DepartmentId = dto.DepartmentId,
                AssignedToUserId = dto.AssignedToUserId,
                Status = dto.Status,
                PurchaseDate = dto.PurchaseDate,
                WarrantyUntil = dto.WarrantyUntil,
                BookValue = dto.BookValue,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            var response = new AssetResponseDto
            {
                AssetId = asset.AssetId,
                AssetTag = asset.AssetTag,
                AssetName = asset.AssetName,
                Category = asset.Category,
                SerialNumber = asset.SerialNumber,
                Status = asset.Status,
                BookValue = asset.BookValue
            };

            return CreatedAtAction(nameof(GetAssetById), new { id = asset.AssetId }, new ApiResponseDto<AssetResponseDto> { Success = true, Message = "Asset created successfully.", Data = response });
        }

        /// <summary>
        /// PUT /api/v1/assets/{id}
        /// Update asset inventory details.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<AssetResponseDto>>> UpdateAsset(int id, [FromBody] UpdateAssetDto dto)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
                return NotFound(new ApiResponseDto<AssetResponseDto> { Success = false, Message = "Asset not found." });

            asset.AssetName = dto.AssetName;
            asset.Category = dto.Category;
            asset.DepartmentId = dto.DepartmentId;
            asset.AssignedToUserId = dto.AssignedToUserId;
            asset.Status = dto.Status;
            asset.PurchaseDate = dto.PurchaseDate;
            asset.WarrantyUntil = dto.WarrantyUntil;
            asset.BookValue = dto.BookValue;
            asset.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = new AssetResponseDto
            {
                AssetId = asset.AssetId,
                AssetTag = asset.AssetTag,
                AssetName = asset.AssetName,
                Category = asset.Category,
                SerialNumber = asset.SerialNumber,
                Status = asset.Status,
                BookValue = asset.BookValue
            };

            return Ok(new ApiResponseDto<AssetResponseDto> { Success = true, Message = "Asset updated successfully.", Data = response });
        }

        /// <summary>
        /// DELETE /api/v1/assets/{id}
        /// Soft delete asset from inventory.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteAsset(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
                return NotFound(new ApiResponseDto<bool> { Success = false, Message = "Asset not found." });

            asset.IsDeleted = true;
            asset.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool> { Success = true, Message = "Asset soft deleted successfully.", Data = true });
        }

        /// <summary>
        /// PUT /api/v1/assets/{id}/assign
        /// Assign or unassign asset to employee user.
        /// </summary>
        [HttpPut("{id}/assign")]
        public async Task<ActionResult<ApiResponseDto<bool>>> AssignAsset(int id, [FromBody] AssignAssetDto dto)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
                return NotFound(new ApiResponseDto<bool> { Success = false, Message = "Asset not found." });

            asset.AssignedToUserId = dto.AssignedToUserId;
            asset.Status = dto.AssignedToUserId.HasValue ? AssetStatus.InUse : AssetStatus.Available;
            asset.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = dto.AssignedToUserId.HasValue ? "Asset assigned to employee successfully." : "Asset unassigned successfully.",
                Data = true
            });
        }
    }
}
