using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SupermarketProject.Data;
using SupermarketProject.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SupermarketProject.Controllers
{
    public class ItemsController : Controller
    {
        private readonly SupermarketDbContext _context;

        public ItemsController(SupermarketDbContext context)
        {
            _context = context;
        }

        // Index action
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("Role");

            var items = await _context.ItemProjects.ToListAsync();

            if (role == "Admin")
            {
                return View("ManageItems", items); // Admin-specific view
            }

            if (role == "Customer")
            {
                return View(items); // Default view for customers
            }

            return RedirectToAction("Login", "UserAccount");
        }

        // Details action
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.ItemProjects.FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();

            return View(item);
        }

        // Edit action
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.ItemProjects.FindAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ItemProjects item, IFormFile? Imgfile)
        {
            if (id != item.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (Imgfile != null && Imgfile.Length > 0)
                    {
                        // Handle image upload
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", Imgfile.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Imgfile.CopyToAsync(stream);
                        }
                        item.Imgfile = Imgfile.FileName; // Update the image if a new one is uploaded
                    }
                    else
                    {
                        // Retain the existing image if no new one is uploaded
                        var existingItem = await _context.ItemProjects.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
                        if (existingItem != null)
                        {
                            item.Imgfile = existingItem.Imgfile;
                        }
                    }

                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        private bool ItemExists(int id)
        {
            return _context.ItemProjects.Any(e => e.Id == id);
        }


        // Delete action
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.ItemProjects.FirstOrDefaultAsync(m => m.Id == id);
            if (item == null) return NotFound();

            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.ItemProjects.FindAsync(id);
            if (item != null)
            {
                _context.ItemProjects.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index)); // Redirect to the items list after deletion
        }

        // Create action
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemProjects item, IFormFile Imgfile)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload
                if (Imgfile != null && Imgfile.Length > 0)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", Imgfile.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Imgfile.CopyToAsync(stream);
                    }
                    item.Imgfile = Imgfile.FileName;
                }

                // If the discount is null or empty, set it to the default value
                if (string.IsNullOrWhiteSpace(item.Discount))
                {
                    item.Discount = " "; // Match the database default
                }

                _context.ItemProjects.Add(item);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }


        // List action
        public async Task<IActionResult> List()
        {
            var items = await _context.ItemProjects.ToListAsync();
            return View(items);
        }

        // Dashboard action
        public async Task<IActionResult> Dashboard()
        {
            // Group by category and count items
            var categoryCounts = await _context.ItemProjects
                .GroupBy(i => i.Category)
                .Select(g => new { Category = g.Key.HasValue ? g.Key.ToString() : "Uncategorized", Count = g.Count() })
                .ToListAsync();

            // Total items
            var totalItems = await _context.ItemProjects.CountAsync();

            // Total quantity from OrderLineProjects
            var totalQuantity = await _context.OrderLineProjects.SumAsync(o => (int?)o.ItemQuant) ?? 0;

            // Serialize category counts to JSON
            ViewBag.CategoryData = JsonConvert.SerializeObject(
                    categoryCounts.Select(c => new object[] { c.Category, c.Count })
                );
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalQuantity = totalQuantity;

            return View();
        }
    }
}
