using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service_Request_Management_System.Data;
using Service_Request_Management_System.Models;

namespace Service_Request_Management_System.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    [Authorize]
    public class AssetCategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AssetCategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/AssetCategories/GetAssetCategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssetCategory>>> GetAssetCategories()
        {
            return await _context.AssetCategories
                .Include(ac => ac.Assets)
                .ToListAsync();
        }

        // GET: api/AssetCategories/GetAssetCategory/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AssetCategory>> GetAssetCategory(int id)
        {
            var category = await _context.AssetCategories
                .Include(ac => ac.Assets)
                .FirstOrDefaultAsync(ac => ac.CategoryId == id);

            if (category == null)
            {
                return NotFound($"Asset Category with ID {id} not found.");
            }

            return Ok(category);
        }

        // POST: api/AssetCategories/PostAssetCategory
        [HttpPost]
        public async Task<ActionResult<AssetCategory>> PostAssetCategory(AssetCategory category)
        {
            if (await _context.AssetCategories.AnyAsync(ac => ac.CategoryName == category.CategoryName))
            {
                return BadRequest($"Asset Category '{category.CategoryName}' already exists.");
            }

            category.CreatedAt = DateTime.UtcNow;

            _context.AssetCategories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAssetCategory), new { id = category.CategoryId }, category);
        }

        // PUT: api/AssetCategories/PutAssetCategory/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssetCategory(int id, AssetCategory category)
        {
            if (id != category.CategoryId)
            {
                return BadRequest("Category ID mismatch.");
            }

            var existing = await _context.AssetCategories.FindAsync(id);
            if (existing == null)
            {
                return NotFound($"Asset Category with ID {id} not found.");
            }

            if (await _context.AssetCategories.AnyAsync(ac => ac.CategoryName == category.CategoryName && ac.CategoryId != id))
            {
                return BadRequest($"Asset Category '{category.CategoryName}' already exists.");
            }

            existing.CategoryName = category.CategoryName;
            existing.IsActive = category.IsActive;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/AssetCategories/DeleteAssetCategory/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssetCategory(int id)
        {
            var category = await _context.AssetCategories
                .Include(ac => ac.Assets)
                .FirstOrDefaultAsync(ac => ac.CategoryId == id);

            if (category == null)
            {
                return NotFound($"Asset Category with ID {id} not found.");
            }

            if (category.Assets.Any(a => !a.IsDeleted))
            {
                return BadRequest($"Cannot delete category '{category.CategoryName}' because it has {category.Assets.Count} assets.");
            }

            _context.AssetCategories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}