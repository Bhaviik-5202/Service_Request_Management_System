using FluentValidation;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class AssetsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IValidator<CreateAssetDto> _createValidator;
        private readonly IValidator<UpdateAssetDto> _updateValidator;
        private readonly IValidator<AssignAssetDto> _assignValidator;

        public AssetsController(
            AppDbContext context,
            IValidator<CreateAssetDto> createValidator,
            IValidator<UpdateAssetDto> updateValidator,
            IValidator<AssignAssetDto> assignValidator)
        {
            _context = context;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _assignValidator = assignValidator;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<AssetResponseDto>>>> GetAssets()
        {
            var assets = await _context.Assets
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Select(AssetResponseSelector)
                .ToListAsync();

            return Ok(new ApiResponseDto<IEnumerable<AssetResponseDto>>
            {
                Success = true,
                Message = "Assets fetched successfully.",
                Data = assets
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<AssetResponseDto>>> GetAssetById(
            int id)
        {
            var asset = await _context.Assets
                .AsNoTracking()
                .Where(a => a.AssetId == id)
                .Select(AssetResponseSelector)
                .FirstOrDefaultAsync();

            if (asset == null)
            {
                return NotFound(new ApiResponseDto<AssetResponseDto>
                {
                    Success = false,
                    Message = "Asset not found.",
                    Data = null
                });
            }

            return Ok(new ApiResponseDto<AssetResponseDto>
            {
                Success = true,
                Message = "Asset fetched successfully.",
                Data = asset
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult<ApiResponseDto<AssetResponseDto>>> CreateAsset(
            CreateAssetDto dto)
        {
            var validation = await _createValidator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return BadRequest(new ApiResponseDto<AssetResponseDto>
                {
                    Success = false,
                    Message = validation.Errors.First().ErrorMessage,
                    Data = null
                });
            }

            var assetExists = await _context.Assets
                .AnyAsync(a =>
                    a.AssetTag == dto.AssetTag ||
                    a.SerialNumber == dto.SerialNumber);

            if (assetExists)
            {
                return BadRequest(new ApiResponseDto<AssetResponseDto>
                {
                    Success = false,
                    Message = "Asset tag or serial number already exists.",
                    Data = null
                });
            }

            if (dto.DepartmentId.HasValue)
            {
                var departmentExists = await _context.Departments
                    .AnyAsync(d =>
                        d.DepartmentId == dto.DepartmentId.Value &&
                        d.IsActive);

                if (!departmentExists)
                {
                    return BadRequest(new ApiResponseDto<AssetResponseDto>
                    {
                        Success = false,
                        Message = "Invalid or inactive department.",
                        Data = null
                    });
                }
            }

            if (dto.AssignedToUserId.HasValue)
            {
                var userExists = await _context.Users
                    .AnyAsync(u =>
                        u.UserId == dto.AssignedToUserId.Value &&
                        u.Status == UserStatus.Active);

                if (!userExists)
                {
                    return BadRequest(new ApiResponseDto<AssetResponseDto>
                    {
                        Success = false,
                        Message = "Invalid or inactive assigned user.",
                        Data = null
                    });
                }
            }

            var now = DateTime.UtcNow;

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
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Assets.Add(asset);

            await _context.SaveChangesAsync();

            AddAssetAuditLog(
                asset,
                "Create",
                $"Asset '{asset.AssetName}' ({asset.AssetTag}) created.",
                now);

            await _context.SaveChangesAsync();

            var response = ToAssetResponse(asset);

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
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult<ApiResponseDto<AssetResponseDto>>> UpdateAsset(
            int id,
            UpdateAssetDto dto)
        {
            var validation = await _updateValidator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return BadRequest(new ApiResponseDto<AssetResponseDto>
                {
                    Success = false,
                    Message = validation.Errors.First().ErrorMessage,
                    Data = null
                });
            }

            var asset = await _context.Assets
                .FirstOrDefaultAsync(a => a.AssetId == id);

            if (asset == null)
            {
                return NotFound(new ApiResponseDto<AssetResponseDto>
                {
                    Success = false,
                    Message = "Asset not found.",
                    Data = null
                });
            }

            if (dto.DepartmentId.HasValue)
            {
                var departmentExists = await _context.Departments
                    .AnyAsync(d =>
                        d.DepartmentId == dto.DepartmentId.Value &&
                        d.IsActive);

                if (!departmentExists)
                {
                    return BadRequest(new ApiResponseDto<AssetResponseDto>
                    {
                        Success = false,
                        Message = "Invalid or inactive department.",
                        Data = null
                    });
                }
            }

            if (dto.AssignedToUserId.HasValue)
            {
                var userExists = await _context.Users
                    .AnyAsync(u =>
                        u.UserId == dto.AssignedToUserId.Value &&
                        u.Status == UserStatus.Active);

                if (!userExists)
                {
                    return BadRequest(new ApiResponseDto<AssetResponseDto>
                    {
                        Success = false,
                        Message = "Invalid or inactive assigned user.",
                        Data = null
                    });
                }
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

            AddAssetAuditLog(
                asset,
                "Update",
                $"Asset '{asset.AssetName}' ({asset.AssetTag}) updated.",
                DateTime.UtcNow);

            await _context.SaveChangesAsync();

            var response = ToAssetResponse(asset);

            return Ok(new ApiResponseDto<AssetResponseDto>
            {
                Success = true,
                Message = "Asset updated successfully.",
                Data = response
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteAsset(
            int id)
        {
            var asset = await _context.Assets
                .FirstOrDefaultAsync(a => a.AssetId == id);

            if (asset == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Asset not found.",
                    Data = false
                });
            }

            asset.IsDeleted = true;
            asset.DeletedAt = DateTime.UtcNow;
            asset.UpdatedAt = DateTime.UtcNow;

            AddAssetAuditLog(
                asset,
                "Delete",
                $"Asset '{asset.AssetName}' ({asset.AssetTag}) deleted.",
                DateTime.UtcNow);

            await _context.SaveChangesAsync();

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Asset deleted successfully.",
                Data = true
            });
        }

        [HttpPut("{id}/assign")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult<ApiResponseDto<bool>>> AssignAsset(
            int id,
            AssignAssetDto dto)
        {
            var validation = await _assignValidator.ValidateAsync(dto);

            if (!validation.IsValid)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = validation.Errors.First().ErrorMessage,
                    Data = false
                });
            }

            var asset = await _context.Assets
                .FirstOrDefaultAsync(a => a.AssetId == id);

            if (asset == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Asset not found.",
                    Data = false
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
                        u.Status == UserStatus.Active);

                if (!userExists)
                {
                    return BadRequest(new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Invalid or inactive user.",
                        Data = false
                    });
                }

                asset.AssignedToUserId = dto.AssignedToUserId.Value;
                asset.Status = AssetStatus.InUse;
            }

            asset.UpdatedAt = DateTime.UtcNow;

            AddAssetAuditLog(
                asset,
                "Assign",
                dto.AssignedToUserId.HasValue
                    ? $"Asset assigned to user {dto.AssignedToUserId}."
                    : "Asset unassigned.",
                DateTime.UtcNow);

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

        private static readonly System.Linq.Expressions.Expression<
            Func<Asset, AssetResponseDto>> AssetResponseSelector = asset =>
            new AssetResponseDto
            {
                AssetId = asset.AssetId,
                AssetTag = asset.AssetTag,
                AssetName = asset.AssetName,
                Category = asset.Category,
                SerialNumber = asset.SerialNumber,
                AssignedToUserId = asset.AssignedToUserId,
                AssignedTo = asset.AssignedTo != null
                    ? asset.AssignedTo.FullName
                    : null,
                DepartmentId = asset.DepartmentId,
                Department = asset.Department != null
                    ? asset.Department.DepartmentName
                    : null,
                Status = asset.Status,
                PurchaseDate = asset.PurchaseDate,
                WarrantyUntil = asset.WarrantyUntil,
                BookValue = asset.BookValue
            };

        private static AssetResponseDto ToAssetResponse(Asset asset) => new()
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

        private void AddAssetAuditLog(
            Asset asset,
            string action,
            string detail,
            DateTime createdAt)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                ActorUserId = null,
                Action = action,
                TargetType = "Asset",
                TargetId = asset.AssetId.ToString(),
                TargetDisplay = asset.AssetTag,
                Detail = detail,
                CreatedAt = createdAt
            });
        }
    }
}
