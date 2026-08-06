using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class AssetsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AssetsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Assets/GetAssets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Asset>>> GetAssets()
        {
            return await _context.Assets
                .Where(a => !a.IsDeleted)
                .Include(a => a.Category)
                .Include(a => a.AssignedToUser)
                .Include(a => a.Department)
                .ToListAsync();
        }

        // GET: api/Assets/GetAsset/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Asset>> GetAsset(int id)
        {
            var asset = await _context.Assets
                .Where(a => !a.IsDeleted)
                .Include(a => a.Category)
                .Include(a => a.AssignedToUser)
                .Include(a => a.Department)
                .FirstOrDefaultAsync(a => a.AssetId == id);

            if (asset == null)
            {
                return NotFound($"Asset with ID {id} not found.");
            }

            return Ok(asset);
        }

        // GET: api/Assets/GetAssetByTag/tag/{assetTag}
        [HttpGet("tag/{assetTag}")]
        public async Task<ActionResult<Asset>> GetAssetByTag(string assetTag)
        {
            var asset = await _context.Assets
                .Where(a => !a.IsDeleted && a.AssetTag == assetTag)
                .Include(a => a.Category)
                .Include(a => a.AssignedToUser)
                .Include(a => a.Department)
                .FirstOrDefaultAsync();

            if (asset == null)
            {
                return NotFound($"Asset with tag '{assetTag}' not found.");
            }

            return Ok(asset);
        }

        // GET: api/Assets/GetAssetBySerial/serial/{serialNumber}
        [HttpGet("serial/{serialNumber}")]
        public async Task<ActionResult<Asset>> GetAssetBySerial(string serialNumber)
        {
            var asset = await _context.Assets
                .Where(a => !a.IsDeleted && a.SerialNumber == serialNumber)
                .Include(a => a.Category)
                .Include(a => a.AssignedToUser)
                .Include(a => a.Department)
                .FirstOrDefaultAsync();

            if (asset == null)
            {
                return NotFound($"Asset with Serial Number '{serialNumber}' not found.");
            }

            return Ok(asset);
        }

        // GET: api/Assets/GetAssetsByUser/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Asset>>> GetAssetsByUser(int userId)
        {
            var assets = await _context.Assets
                .Where(a => !a.IsDeleted && a.AssignedToUserId == userId)
                .Include(a => a.Category)
                .Include(a => a.AssignedToUser)
                .Include(a => a.Department)
                .ToListAsync();

            return Ok(assets);
        }

        // GET: api/Assets/GetAssetsByDepartment/department/5
        [HttpGet("department/{departmentId}")]
        public async Task<ActionResult<IEnumerable<Asset>>> GetAssetsByDepartment(int departmentId)
        {
            var assets = await _context.Assets
                .Where(a => !a.IsDeleted && a.DepartmentId == departmentId)
                .Include(a => a.Category)
                .Include(a => a.AssignedToUser)
                .Include(a => a.Department)
                .ToListAsync();

            return Ok(assets);
        }

        // GET: api/Assets/GetAssetsByCategory/category/5
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Asset>>> GetAssetsByCategory(int categoryId)
        {
            var assets = await _context.Assets
                .Where(a => !a.IsDeleted && a.CategoryId == categoryId)
                .Include(a => a.Category)
                .Include(a => a.AssignedToUser)
                .Include(a => a.Department)
                .ToListAsync();

            return Ok(assets);
        }

        // GET: api/Assets/GetAssetsByStatus/status/{status}
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Asset>>> GetAssetsByStatus(string status)
        {
            var assets = await _context.Assets
                .Where(a => !a.IsDeleted && a.Status == status)
                .Include(a => a.Category)
                .Include(a => a.AssignedToUser)
                .Include(a => a.Department)
                .ToListAsync();

            return Ok(assets);
        }

        // POST: api/Assets/PostAsset
        [HttpPost]
        public async Task<ActionResult<Asset>> PostAsset(Asset asset)
        {
            if (await _context.Assets.AnyAsync(a => a.AssetTag == asset.AssetTag && !a.IsDeleted))
            {
                return BadRequest($"Asset with tag '{asset.AssetTag}' already exists.");
            }

            if (await _context.Assets.AnyAsync(a => a.SerialNumber == asset.SerialNumber && !a.IsDeleted))
            {
                return BadRequest($"Asset with serial number '{asset.SerialNumber}' already exists.");
            }

            if (!await _context.AssetCategories.AnyAsync(c => c.CategoryId == asset.CategoryId && c.IsActive))
            {
                return BadRequest($"Asset Category with ID {asset.CategoryId} does not exist or is inactive.");
            }

            if (asset.AssignedToUserId.HasValue && !await _context.Users.AnyAsync(u => u.UserId == asset.AssignedToUserId && !u.IsDeleted))
            {
                return BadRequest($"User with ID {asset.AssignedToUserId} does not exist or is deleted.");
            }

            if (asset.DepartmentId.HasValue && !await _context.Departments.AnyAsync(d => d.DepartmentId == asset.DepartmentId && !d.IsDeleted))
            {
                return BadRequest($"Department with ID {asset.DepartmentId} does not exist or is deleted.");
            }

            var validStatuses = new[] { "In Use", "Available", "Under Repair", "Retired" };
            if (!validStatuses.Contains(asset.Status))
            {
                return BadRequest($"Invalid status. Valid values: {string.Join(", ", validStatuses)}");
            }

            if (asset.WarrantyUntil < asset.PurchaseDate)
            {
                return BadRequest("Warranty date cannot be earlier than purchase date.");
            }

            asset.CreatedAt = DateTime.UtcNow;
            asset.UpdatedAt = DateTime.UtcNow;
            asset.IsDeleted = false;

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAsset), new { id = asset.AssetId }, asset);
        }

        // PUT: api/Assets/PutAsset/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsset(int id, Asset asset)
        {
            if (id != asset.AssetId)
            {
                return BadRequest("Asset ID mismatch.");
            }

            var existingAsset = await _context.Assets.FirstOrDefaultAsync(a => a.AssetId == id && !a.IsDeleted);
            if (existingAsset == null)
            {
                return NotFound($"Asset with ID {id} not found.");
            }

            if (await _context.Assets.AnyAsync(a => a.AssetTag == asset.AssetTag && a.AssetId != id && !a.IsDeleted))
            {
                return BadRequest($"Asset with tag '{asset.AssetTag}' already exists.");
            }

            if (await _context.Assets.AnyAsync(a => a.SerialNumber == asset.SerialNumber && a.AssetId != id && !a.IsDeleted))
            {
                return BadRequest($"Asset with serial number '{asset.SerialNumber}' already exists.");
            }

            if (!await _context.AssetCategories.AnyAsync(c => c.CategoryId == asset.CategoryId && c.IsActive))
            {
                return BadRequest($"Asset Category with ID {asset.CategoryId} does not exist or is inactive.");
            }

            if (asset.AssignedToUserId.HasValue && !await _context.Users.AnyAsync(u => u.UserId == asset.AssignedToUserId && !u.IsDeleted))
            {
                return BadRequest($"User with ID {asset.AssignedToUserId} does not exist or is deleted.");
            }

            if (asset.DepartmentId.HasValue && !await _context.Departments.AnyAsync(d => d.DepartmentId == asset.DepartmentId && !d.IsDeleted))
            {
                return BadRequest($"Department with ID {asset.DepartmentId} does not exist or is deleted.");
            }

            var validStatuses = new[] { "In Use", "Available", "Under Repair", "Retired" };
            if (!validStatuses.Contains(asset.Status))
            {
                return BadRequest($"Invalid status. Valid values: {string.Join(", ", validStatuses)}");
            }

            if (asset.WarrantyUntil < asset.PurchaseDate)
            {
                return BadRequest("Warranty date cannot be earlier than purchase date.");
            }

            existingAsset.AssetTag = asset.AssetTag;
            existingAsset.AssetName = asset.AssetName;
            existingAsset.CategoryId = asset.CategoryId;
            existingAsset.SerialNumber = asset.SerialNumber;
            existingAsset.AssignedToUserId = asset.AssignedToUserId;
            existingAsset.DepartmentId = asset.DepartmentId;
            existingAsset.Status = asset.Status;
            existingAsset.PurchaseDate = asset.PurchaseDate;
            existingAsset.WarrantyUntil = asset.WarrantyUntil;
            existingAsset.BookValue = asset.BookValue;
            existingAsset.UpdatedAt = DateTime.UtcNow;

            _context.Entry(existingAsset).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Assets/DeleteAsset/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            var asset = await _context.Assets.FirstOrDefaultAsync(a => a.AssetId == id && !a.IsDeleted);

            if (asset == null)
            {
                return NotFound($"Asset with ID {id} not found.");
            }

            asset.IsDeleted = true;
            asset.Status = "Retired";

            _context.Entry(asset).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Assets/RestoreAsset/5/restore
        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreAsset(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                return NotFound($"Asset with ID {id} not found.");
            }

            if (!asset.IsDeleted)
            {
                return BadRequest("Asset is not deleted.");
            }

            asset.IsDeleted = false;
            asset.Status = "Available";
            asset.UpdatedAt = DateTime.UtcNow;

            _context.Entry(asset).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}