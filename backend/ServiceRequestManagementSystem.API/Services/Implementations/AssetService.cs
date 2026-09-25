using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ServiceRequestManagementSystem.API.Data;
using ServiceRequestManagementSystem.API.DTOs.Assets;
using ServiceRequestManagementSystem.API.DTOs.Common;
using ServiceRequestManagementSystem.API.Enums;
using ServiceRequestManagementSystem.API.Models;
using ServiceRequestManagementSystem.API.Repositories.Interfaces;
using ServiceRequestManagementSystem.API.Services.Interfaces;

namespace ServiceRequestManagementSystem.API.Services.Implementations
{
    public class AssetService : IAssetService
    {
        private readonly IUnitOfWork _uow;
        private readonly AppDbContext _context;
        private readonly IValidator<CreateAssetDto> _createValidator;
        private readonly IValidator<UpdateAssetDto> _updateValidator;
        private readonly IValidator<AssignAssetDto> _assignValidator;

        public AssetService(
            IUnitOfWork uow,
            AppDbContext context,
            IValidator<CreateAssetDto> createValidator,
            IValidator<UpdateAssetDto> updateValidator,
            IValidator<AssignAssetDto> assignValidator)
        {
            _uow = uow;
            _context = context;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _assignValidator = assignValidator;
        }

        public async Task<ApiResponseDto<IEnumerable<AssetResponseDto>>> GetAssetsAsync()
        {
            var assets = await _context.Assets
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt)
                .Select(AssetResponseSelector)
                .ToListAsync();

            return Ok("Assets fetched successfully.", (IEnumerable<AssetResponseDto>)assets);
        }

        public async Task<ApiResponseDto<AssetResponseDto>> GetAssetByIdAsync(int id)
        {
            var asset = await _context.Assets
                .AsNoTracking()
                .Where(a => a.AssetId == id)
                .Select(AssetResponseSelector)
                .FirstOrDefaultAsync();

            if (asset == null)
                return Fail<AssetResponseDto>("Asset not found.");

            return Ok("Asset fetched successfully.", asset);
        }

        public async Task<ApiResponseDto<AssetResponseDto>> CreateAssetAsync(CreateAssetDto dto, string? ipAddress)
        {
            var validation = await _createValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return Fail<AssetResponseDto>(validation.Errors.First().ErrorMessage);

            var assetExists = await _context.Assets.AnyAsync(a =>
                a.AssetTag == dto.AssetTag || a.SerialNumber == dto.SerialNumber);

            if (assetExists)
                return Fail<AssetResponseDto>("Asset tag or serial number already exists.");

            if (dto.DepartmentId.HasValue)
            {
                var departmentExists = await _uow.Departments.AnyAsync(d =>
                    d.DepartmentId == dto.DepartmentId.Value && d.IsActive);

                if (!departmentExists)
                    return Fail<AssetResponseDto>("Invalid or inactive department.");
            }

            if (dto.AssignedToUserId.HasValue)
            {
                var userExists = await _uow.Users.AnyAsync(u =>
                    u.UserId == dto.AssignedToUserId.Value && u.Status == UserStatus.Active);

                if (!userExists)
                    return Fail<AssetResponseDto>("Invalid or inactive assigned user.");
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

            await _uow.Assets.AddAsync(asset);
            await _uow.SaveChangesAsync();

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                ActorUserId = null,
                Action = "Create",
                TargetType = "Asset",
                TargetId = asset.AssetId.ToString(),
                TargetDisplay = asset.AssetTag,
                Detail = $"Asset '{asset.AssetName}' ({asset.AssetTag}) created.",
                IpAddress = ipAddress,
                CreatedAt = now
            });
            await _uow.SaveChangesAsync();

            var response = ToAssetResponse(asset);
            return Ok("Asset created successfully.", response);
        }

        public async Task<ApiResponseDto<AssetResponseDto>> UpdateAssetAsync(int id, UpdateAssetDto dto, string? ipAddress)
        {
            var validation = await _updateValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return Fail<AssetResponseDto>(validation.Errors.First().ErrorMessage);

            var asset = await _uow.Assets.FirstOrDefaultAsync(a => a.AssetId == id);
            if (asset == null)
                return Fail<AssetResponseDto>("Asset not found.");

            if (dto.DepartmentId.HasValue)
            {
                var departmentExists = await _uow.Departments.AnyAsync(d =>
                    d.DepartmentId == dto.DepartmentId.Value && d.IsActive);

                if (!departmentExists)
                    return Fail<AssetResponseDto>("Invalid or inactive department.");
            }

            if (dto.AssignedToUserId.HasValue)
            {
                var userExists = await _uow.Users.AnyAsync(u =>
                    u.UserId == dto.AssignedToUserId.Value && u.Status == UserStatus.Active);

                if (!userExists)
                    return Fail<AssetResponseDto>("Invalid or inactive assigned user.");
            }

            var now = DateTime.UtcNow;
            asset.AssetName = dto.AssetName;
            asset.Category = dto.Category;
            asset.DepartmentId = dto.DepartmentId;
            asset.AssignedToUserId = dto.AssignedToUserId;
            asset.Status = dto.Status;
            asset.PurchaseDate = dto.PurchaseDate;
            asset.WarrantyUntil = dto.WarrantyUntil;
            asset.BookValue = dto.BookValue;
            asset.UpdatedAt = now;

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                ActorUserId = null,
                Action = "Update",
                TargetType = "Asset",
                TargetId = asset.AssetId.ToString(),
                TargetDisplay = asset.AssetTag,
                Detail = $"Asset '{asset.AssetName}' ({asset.AssetTag}) updated.",
                IpAddress = ipAddress,
                CreatedAt = now
            });

            await _uow.SaveChangesAsync();

            var response = ToAssetResponse(asset);
            return Ok("Asset updated successfully.", response);
        }

        public async Task<ApiResponseDto<object>> DeleteAssetAsync(int id, string? ipAddress)
        {
            var asset = await _uow.Assets.FirstOrDefaultAsync(a => a.AssetId == id);
            if (asset == null)
                return Fail<object>("Asset not found.");

            var now = DateTime.UtcNow;
            asset.IsDeleted = true;
            asset.DeletedAt = now;
            asset.UpdatedAt = now;

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                ActorUserId = null,
                Action = "Delete",
                TargetType = "Asset",
                TargetId = asset.AssetId.ToString(),
                TargetDisplay = asset.AssetTag,
                Detail = $"Asset '{asset.AssetName}' ({asset.AssetTag}) deleted.",
                IpAddress = ipAddress,
                CreatedAt = now
            });

            await _uow.SaveChangesAsync();
            return Ok<object>("Asset deleted successfully.", true);
        }

        public async Task<ApiResponseDto<AssetResponseDto>> AssignAssetAsync(int id, AssignAssetDto dto, string? ipAddress)
        {
            var validation = await _assignValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return Fail<AssetResponseDto>(validation.Errors.First().ErrorMessage);

            var asset = await _uow.Assets.FirstOrDefaultAsync(a => a.AssetId == id);
            if (asset == null)
                return Fail<AssetResponseDto>("Asset not found.");

            if (!dto.AssignedToUserId.HasValue)
            {
                asset.AssignedToUserId = null;
                asset.Status = AssetStatus.Available;
            }
            else
            {
                var userExists = await _uow.Users.AnyAsync(u =>
                    u.UserId == dto.AssignedToUserId.Value && u.Status == UserStatus.Active);

                if (!userExists)
                    return Fail<AssetResponseDto>("Invalid or inactive user.");

                asset.AssignedToUserId = dto.AssignedToUserId.Value;
                asset.Status = AssetStatus.InUse;
            }

            var now = DateTime.UtcNow;
            asset.UpdatedAt = now;

            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                ActorUserId = null,
                Action = "Assign",
                TargetType = "Asset",
                TargetId = asset.AssetId.ToString(),
                TargetDisplay = asset.AssetTag,
                Detail = dto.AssignedToUserId.HasValue
                    ? $"Asset assigned to user {dto.AssignedToUserId}."
                    : "Asset unassigned.",
                IpAddress = ipAddress,
                CreatedAt = now
            });

            await _uow.SaveChangesAsync();
            var response = ToAssetResponse(asset);
            return Ok(dto.AssignedToUserId.HasValue ? "Asset assigned successfully." : "Asset unassigned successfully.", response);
        }

        public async Task<ApiResponseDto<AssetResponseDto>> UnassignAssetAsync(int id, string? ipAddress)
        {
            return await AssignAssetAsync(id, new AssignAssetDto { AssignedToUserId = null }, ipAddress);
        }

        private static readonly System.Linq.Expressions.Expression<Func<Asset, AssetResponseDto>> AssetResponseSelector = asset =>
            new AssetResponseDto
            {
                AssetId = asset.AssetId,
                AssetTag = asset.AssetTag,
                AssetName = asset.AssetName,
                Category = asset.Category,
                SerialNumber = asset.SerialNumber,
                AssignedToUserId = asset.AssignedToUserId,
                AssignedTo = asset.AssignedTo != null ? asset.AssignedTo.FullName : null,
                DepartmentId = asset.DepartmentId,
                Department = asset.Department != null ? asset.Department.DepartmentName : null,
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

        private static ApiResponseDto<T> Ok<T>(string message, T data) =>
            new() { Success = true, Message = message, Data = data };

        private static ApiResponseDto<T> Fail<T>(string message) =>
            new() { Success = false, Message = message, Data = default };
    }
}
