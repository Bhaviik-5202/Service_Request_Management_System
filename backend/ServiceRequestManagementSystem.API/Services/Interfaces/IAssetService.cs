using ServiceRequestManagementSystem.API.DTOs.Assets;
using ServiceRequestManagementSystem.API.DTOs.Common;

namespace ServiceRequestManagementSystem.API.Services.Interfaces
{
    public interface IAssetService
    {
        Task<ApiResponseDto<IEnumerable<AssetResponseDto>>> GetAssetsAsync();
        Task<ApiResponseDto<AssetResponseDto>> GetAssetByIdAsync(int id);
        Task<ApiResponseDto<AssetResponseDto>> CreateAssetAsync(CreateAssetDto dto, string? ipAddress);
        Task<ApiResponseDto<AssetResponseDto>> UpdateAssetAsync(int id, UpdateAssetDto dto, string? ipAddress);
        Task<ApiResponseDto<object>> DeleteAssetAsync(int id, string? ipAddress);
        Task<ApiResponseDto<AssetResponseDto>> AssignAssetAsync(int id, AssignAssetDto dto, string? ipAddress);
        Task<ApiResponseDto<AssetResponseDto>> UnassignAssetAsync(int id, string? ipAddress);
    }
}
