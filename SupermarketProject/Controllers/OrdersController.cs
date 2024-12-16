using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupermarketProject.Data;
using SupermarketProject.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SupermarketProject.Controllers
{
    public class OrdersController : Controller
    {
        private readonly SupermarketDbContext _context;

        public OrdersController(SupermarketDbContext context)
        {
            _context = context;
        }

        // CatalogueBuy action
        public async Task<IActionResult> CatalogueBuy()
        {
            var items = await _context.ItemProjects
                .Where(i => i.Quantity > 0) // Show only items with available stock
                .ToListAsync();

            return View(items);
        }

        // ItemBuyDetail action
        public async Task<IActionResult> ItemBuyDetail(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.ItemProjects.FirstOrDefaultAsync(m => m.Id == id && m.Quantity > 0); // Exclude items with no stock
            if (item == null) return NotFound();

            return View(item);
        }
        public async Task<IActionResult> PurchaseReport()
        {
            var report = await _context.OrderProjects
                .GroupBy(o => o.CustName)
                .Select(g => new
                {
                    CustomerName = g.Key,
                    TotalPurchase = g.Sum(o => o.Total)
                })
                .ToListAsync();

            // Pass the report data to the view
            ViewBag.Report = report;
            return View(report);
        }
        public async Task<IActionResult> OrdersDetail(string customerName)
        {
            if (string.IsNullOrEmpty(customerName))
            {
                return NotFound("Customer name is required.");
            }

            var customerOrders = await _context.OrderProjects
                .Where(o => o.CustName == customerName)
                .Include(o => o.OrderLineProjects) // Include related order lines
                .ToListAsync();

            if (customerOrders == null || !customerOrders.Any())
            {
                return NotFound("No orders found for this customer.");
            }

            ViewBag.CustomerName = customerName; // Pass the customer name to the view
            return View(customerOrders);
        }


        // CartAdd action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CartAdd(int itemId, int quantity)
        {
            var item = _context.ItemProjects.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return NotFound();

            // Check if the requested quantity is available
            if (item.Quantity < quantity)
            {
                TempData["Error"] = $"Only {item.Quantity} units of {item.Name} are available.";
                return RedirectToAction("CatalogueBuy");
            }

            // Retrieve cart or initialize it
            var cart = HttpContext.Session.GetObject<List<OrderLineProjects>>("Cart") ?? new List<OrderLineProjects>();

            // Check if the item already exists in the cart
            var existingCartItem = cart.FirstOrDefault(c => c.ItemName == item.Name);
            if (existingCartItem != null)
            {
                // Ensure the updated quantity does not exceed available stock
                if (existingCartItem.ItemQuant + quantity > item.Quantity)
                {
                    TempData["Error"] = $"Cannot add more than {item.Quantity} units of {item.Name}.";
                    return RedirectToAction("CatalogueBuy");
                }

                existingCartItem.ItemQuant += quantity;
            }
            else
            {
                cart.Add(new OrderLineProjects
                {
                    ItemName = item.Name,
                    ItemQuant = quantity,
                    ItemPrice = item.Price,
                    OrderId = 0 // Temporary, not linked to an order yet
                });
            }

            // Save cart back to session
            HttpContext.Session.SetObject("Cart", cart);

            TempData["Message"] = "Item added to cart!";
            return RedirectToAction("CatalogueBuy");
        }

        // CartBuy action
        public async Task<IActionResult> CartBuy()
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "UserAccount");
            }

            // Retrieve the cart from the session
            var cart = HttpContext.Session.GetObject<List<OrderLineProjects>>("Cart");
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("CatalogueBuy");
            }

            // Validate stock availability
            foreach (var cartItem in cart)
            {
                var item = _context.ItemProjects.FirstOrDefault(i => i.Name == cartItem.ItemName);
                if (item == null || item.Quantity < cartItem.ItemQuant)
                {
                    TempData["Error"] = $"Not enough stock for {cartItem.ItemName}. Available: {item?.Quantity ?? 0}.";
                    return RedirectToAction("ViewCart");
                }
            }

            // Create a new order
            var newOrder = new OrderProjects
            {
                CustName = userName,
                OrderDate = DateTime.Now,
                Total = cart.Sum(c => (int)(c.ItemPrice * c.ItemQuant))
            };

            _context.OrderProjects.Add(newOrder);
            await _context.SaveChangesAsync();

            // Deduct stock and save cart items as order lines
            foreach (var cartItem in cart)
            {
                var item = _context.ItemProjects.FirstOrDefault(i => i.Name == cartItem.ItemName);
                if (item != null)
                {
                    item.Quantity -= cartItem.ItemQuant; // Deduct stock
                    _context.ItemProjects.Update(item);
                }

                cartItem.OrderId = newOrder.Id; // Associate with order
                _context.OrderLineProjects.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            // Clear the session cart
            HttpContext.Session.Remove("Cart");

            TempData["Message"] = "Order placed successfully!";
            return RedirectToAction("MyOrder");
        }
        // Buy action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(int itemId, int quantity)
        {
            var item = await _context.ItemProjects.FindAsync(itemId);
            if (item == null) return NotFound();

            var orderLine = new OrderLineProjects
            {
                ItemName = item.Name,
                ItemQuant = quantity,
                ItemPrice = item.Price,
                OrderId = 0
            };

            _context.OrderLineProjects.Add(orderLine);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Item added to cart. Proceed to checkout!";
            return RedirectToAction("CartBuy");
        }

        // MyOrder action
        public async Task<IActionResult> MyOrder()
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName)) return RedirectToAction("Login", "UserAccount");

            var orders = await _context.OrderProjects
                .Where(o => o.CustName == userName)
                .Include(o => o.OrderLineProjects) // Load related items
                .ToListAsync();

            return View(orders);
        }
        public IActionResult ViewCart()
        {
            // Retrieve cart from session
            var cart = HttpContext.Session.GetObject<List<OrderLineProjects>>("Cart") ?? new List<OrderLineProjects>();

            // Pass cart items to the view
            return View(cart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int itemId)
        {
            // Retrieve cart from session
            var cart = HttpContext.Session.GetObject<List<OrderLineProjects>>("Cart") ?? new List<OrderLineProjects>();

            // Find the item in the cart
            var itemToRemove = cart.FirstOrDefault(c => c.Id == itemId);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove); // Remove item from the cart
                HttpContext.Session.SetObject("Cart", cart); // Update session
            }

            TempData["Message"] = "Item removed from your cart!";
            return RedirectToAction("ViewCart");
        }

        // OrderLine action
        public async Task<IActionResult> OrderLine(int orderId)
        {
            var orderLines = await _context.OrderLineProjects
                .Where(ol => ol.OrderId == orderId)
                .ToListAsync();

            return View(orderLines); // Pass order line items to the view
        }

    }
}
