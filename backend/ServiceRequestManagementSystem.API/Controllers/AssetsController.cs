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
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AssetsController(AppDbContext context)
        {
            _context = context;
        }

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
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 10;

            if (pageSize > 100)
                pageSize = 100;

            var query = _context.Assets
                .Where(a => !a.IsDeleted)
                .Include(a => a.AssignedTo)
                .Include(a => a.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                var categoryText = category.Trim().ToLower();
                query = query.Where(a =>
                    a.Category.ToLower().Contains(categoryText));
            }

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            if (departmentId.HasValue)
                query = query.Where(a => a.DepartmentId == departmentId.Value);

            if (assignedToUserId.HasValue)
                query = query.Where(a => a.AssignedToUserId == assignedToUserId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchText = search.Trim().ToLower();

                query = query.Where(a =>
                    a.AssetTag.ToLower().Contains(searchText) ||
                    a.AssetName.ToLower().Contains(searchText) ||
                    a.SerialNumber.ToLower().Contains(searchText));
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var assets = await query
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
                    AssignedToUserId = a.AssignedToUserId,
                    AssignedTo = a.AssignedTo != null ? a.AssignedTo.FullName : null,
                    DepartmentId = a.DepartmentId,
                    Department = a.Department != null ? a.Department.DepartmentName : null,
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
                Data = assets,
                Pagination = new PaginationMetadataDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<AssetResponseDto>>> GetAssetById(int id)
        {
            var asset = await _context.Assets
                .Where(a => !a.IsDeleted)
                .Include(a => a.AssignedTo)
                .Include(a => a.Department)
                .FirstOrDefaultAsync(a => a.AssetId == id);

            if (asset == null)
            {
                return NotFound(new ApiResponseDto<AssetResponseDto>
                {
                    Success = false,
                    Message = "Asset not found."
                });
            }

            var response = new AssetResponseDto
            {
                AssetId = asset.AssetId,
                AssetTag = asset.AssetTag,
                AssetName = asset.AssetName,
                Category = asset.Category,
                SerialNumber = asset.SerialNumber,
                AssignedToUserId = asset.AssignedToUserId,
                AssignedTo = asset.AssignedTo?.FullName,
                DepartmentId = asset.DepartmentId,
                Department = asset.Department?.DepartmentName,
                Status = asset.Status,
                PurchaseDate = asset.PurchaseDate,
                WarrantyUntil = asset.WarrantyUntil,
                BookValue = asset.BookValue
            };

            return Ok(new ApiResponseDto<AssetResponseDto>
            {
                Success = true,
                Message = "Asset fetched successfully.",
                Data = response
            });
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<AssetResponseDto>>> CreateAsset(
            [FromBody] CreateAssetDto dto)
        {
            var assetExists = await _context.Assets
                .AnyAsync(a =>
                    !a.IsDeleted &&
                    (a.AssetTag == dto.AssetTag ||
                     a.SerialNumber == dto.SerialNumber));

            if (assetExists)
            {
                return BadRequest(new ApiResponseDto<AssetResponseDto>
                {
                    Success = false,
                    Message = "Asset tag or serial number already exists."
                });
            }

            if (dto.DepartmentId.HasValue)
            {
                var departmentExists = await _context.Departments
                    .AnyAsync(d =>
                        d.DepartmentId == dto.DepartmentId.Value &&
                        !d.IsDeleted &&
                        d.IsActive);

                if (!departmentExists)
                {
                    return BadRequest(new ApiResponseDto<AssetResponseDto>
                    {
                        Success = false,
                        Message = "Invalid or inactive department."
                    });
                }
            }

            if (dto.AssignedToUserId.HasValue)
            {
                var userExists = await _context.Users
                    .AnyAsync(u =>
                        u.UserId == dto.AssignedToUserId.Value &&
                        !u.IsDeleted &&
                        u.Status == UserStatus.Active);

                if (!userExists)
                {
                    return BadRequest(new ApiResponseDto<AssetResponseDto>
                    {
                        Success = false,
                        Message = "Invalid or inactive assigned user."
                    });
                }
            }

            if (dto.WarrantyUntil < dto.PurchaseDate)
            {
                return BadRequest(new ApiResponseDto<AssetResponseDto>
                {
                    Success = false,
                    Message = "Warranty date cannot be earlier than purchase date."
                });
            }

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

            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = dto.AssignedToUserId,
                Action = "Create",
                TargetType = "Asset",
                TargetId = asset.AssetId.ToString(),
                TargetDisplay = asset.AssetTag,
                Detail = $"Asset '{asset.AssetName}' ({asset.AssetTag}) created.",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var response = new AssetResponseDto
            {
                AssetId = asset.AssetId,
                AssetTag = asset.AssetTag,
                AssetName = asset.AssetName,
                Category = asset.Category,
                SerialNumber = asset.SerialNumber,
                AssignedToUserId = asset.AssignedToUserId,
                DepartmentId = asset.DepartmentId,
                Status = asset.Status,
                PurchaseDate = asset.PurchaseDate,
                WarrantyUntil = asset.WarrantyUntil,
                BookValue = asset.BookValue
            };

            return CreatedAtAction(
                nameof(GetAssetById),
                new { id = asset.AssetId },
                new ApiResponseDto<AssetResponseDto>
                {
                    Success = true,
                    Message = "Asset created successfully.",
                    Data = response
                });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<AssetResponseDto>>> UpdateAsset(
            int id,
            [FromBody] UpdateAssetDto dto)
        {
            var asset = await _context.Assets
                .FirstOrDefaultAsync(a =>
                    a.AssetId == id &&
                    !a.IsDeleted);

            if (asset == null)
            {
                return NotFound(new ApiResponseDto<AssetResponseDto>
                {
                    Success = false,
                    Message = "Asset not found."
                });
            }

            if (dto.DepartmentId.HasValue)
            {
                var departmentExists = await _context.Departments
                    .AnyAsync(d =>
                        d.DepartmentId == dto.DepartmentId.Value &&
                        !d.IsDeleted &&
                        d.IsActive);

                if (!departmentExists)
                {
                    return BadRequest(new ApiResponseDto<AssetResponseDto>
                    {
                        Success = false,
                        Message = "Invalid or inactive department."
                    });
                }
            }

            if (dto.AssignedToUserId.HasValue)
            {
                var userExists = await _context.Users
                    .AnyAsync(u =>
                        u.UserId == dto.AssignedToUserId.Value &&
                        !u.IsDeleted &&
                        u.Status == UserStatus.Active);

                if (!userExists)
                {
                    return BadRequest(new ApiResponseDto<AssetResponseDto>
                    {
                        Success = false,
                        Message = "Invalid or inactive assigned user."
                    });
                }
            }

            if (dto.WarrantyUntil < dto.PurchaseDate)
            {
                return BadRequest(new ApiResponseDto<AssetResponseDto>
                {
                    Success = false,
                    Message = "Warranty date cannot be earlier than purchase date."
                });
            }

            asset.AssetName = dto.AssetName;
            asset.Category = dto.Category;
            asset.DepartmentId = dto.DepartmentId;
            asset.AssignedToUserId = dto.AssignedToUserId;
            asset.Status = dto.Status;
            asset.PurchaseDate = dto.PurchaseDate;
            asset.WarrantyUntil = dto.WarrantyUntil;
            asset.BookValue = dto.BookValue;
            asset.UpdatedAt = DateTime.UtcNow;

            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = dto.AssignedToUserId,
                Action = "Update",
                TargetType = "Asset",
                TargetId = asset.AssetId.ToString(),
                TargetDisplay = asset.AssetTag,
                Detail = $"Asset '{asset.AssetName}' ({asset.AssetTag}) updated.",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var response = new AssetResponseDto
            {
                AssetId = asset.AssetId,
                AssetTag = asset.AssetTag,
                AssetName = asset.AssetName,
                Category = asset.Category,
                SerialNumber = asset.SerialNumber,
                AssignedToUserId = asset.AssignedToUserId,
                DepartmentId = asset.DepartmentId,
                Status = asset.Status,
                PurchaseDate = asset.PurchaseDate,
                WarrantyUntil = asset.WarrantyUntil,
                BookValue = asset.BookValue
            };

            return Ok(new ApiResponseDto<AssetResponseDto>
            {
                Success = true,
                Message = "Asset updated successfully.",
                Data = response
            });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteAsset(int id)
        {
            var asset = await _context.Assets
                .FirstOrDefaultAsync(a =>
                    a.AssetId == id &&
                    !a.IsDeleted);

            if (asset == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Asset not found."
                });
            }

            asset.IsDeleted = true;
            asset.DeletedAt = DateTime.UtcNow;
            asset.UpdatedAt = DateTime.UtcNow;

            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = asset.AssignedToUserId,
                Action = "Delete",
                TargetType = "Asset",
                TargetId = asset.AssetId.ToString(),
                TargetDisplay = asset.AssetTag,
                Detail = $"Asset '{asset.AssetName}' ({asset.AssetTag}) deleted.",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Asset deleted successfully.",
                Data = true
            });
        }

        [HttpPut("{id}/assign")]
        public async Task<ActionResult<ApiResponseDto<bool>>> AssignAsset(
            int id,
            [FromBody] AssignAssetDto dto)
        {
            var asset = await _context.Assets
                .FirstOrDefaultAsync(a =>
                    a.AssetId == id &&
                    !a.IsDeleted);

            if (asset == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Asset not found."
                });
            }

            if (!dto.AssignedToUserId.HasValue)
            {
                asset.AssignedToUserId = null;
                asset.Status = AssetStatus.Available;
            }
            else
            {
                var userExists = await _context.Users
                    .AnyAsync(u =>
                        u.UserId == dto.AssignedToUserId.Value &&
                        !u.IsDeleted &&
                        u.Status == UserStatus.Active);

                if (!userExists)
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Invalid or inactive user."
                    });
                }

                asset.AssignedToUserId = dto.AssignedToUserId.Value;
                asset.Status = AssetStatus.InUse;
            }

            asset.UpdatedAt = DateTime.UtcNow;

            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = dto.AssignedToUserId,
                Action = "Assign",
                TargetType = "Asset",
                TargetId = asset.AssetId.ToString(),
                TargetDisplay = asset.AssetTag,
                Detail = dto.AssignedToUserId.HasValue ? $"Asset assigned to user {dto.AssignedToUserId}." : "Asset unassigned.",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = dto.AssignedToUserId.HasValue
                    ? "Asset assigned successfully."
                    : "Asset unassigned successfully.",
                Data = true
            });
        }
    }
}
