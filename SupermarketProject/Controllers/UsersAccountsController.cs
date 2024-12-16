using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupermarketProject.Data;
using SupermarketProject.Models;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace SupermarketProject.Controllers
{
    public class UserAccountController : Controller
    {
        private readonly SupermarketDbContext _context;

        public UserAccountController(SupermarketDbContext context)
        {
            _context = context;
        }

        // Index action
        public async Task<IActionResult> Index()
        {
            return View(await _context.UserAccountProjects.ToListAsync());
        }

        // Details action
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.UserAccountProjects.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) return NotFound();

            return View(user);
        }

        // Edit action
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.UserAccountProjects.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Pass,Role")] UserAccountProjects user)
        {
            if (id != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // Delete action
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.UserAccountProjects.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.UserAccountProjects.FindAsync(id);
            _context.UserAccountProjects.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: UserAccount/Register
        public IActionResult Register()
        {
            ViewBag.Message = TempData["Message"]; // For displaying success or error messages
            return View();
        }

        // POST: UserAccount/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(CustomerProjects customer, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                TempData["ErrorMessage"] = "Passwords do not match. Please try again.";
                return RedirectToAction("Register");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Add customer to CustomerProjects table
                    _context.CustomerProjects.Add(customer);
                    await _context.SaveChangesAsync();

                    // Copy relevant data to UserAccountProjects table
                    var userAccount = new UserAccountProjects
                    {
                        Name = customer.Name,
                        Pass = password, // Store the password
                        Role = "Customer" // Default role
                    };

                    _context.UserAccountProjects.Add(userAccount);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Registration successful! Redirecting to login...";
                    return RedirectToAction("Register");
                }
                catch
                {
                    TempData["ErrorMessage"] = "An error occurred during registration. Please try again.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid input. Please check your information and try again.";
            }

            return RedirectToAction("Register");
        }

        // SearchAll action
        public async Task<IActionResult> SearchAll(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return View(await _context.UserAccountProjects.ToListAsync());
            }

            var result = await _context.UserAccountProjects
                .Where(u => u.Name.Contains(search) || u.Role.Contains(search))
                .ToListAsync();
            return View(result);
        }

        // Create (AdminAdd) action
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserAccountProjects user, string ConfirmPassword)
        {
            if (user.Pass != ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Passwords do not match. Please try again.";
                return View(user);
            }

            if (ModelState.IsValid)
            {
                user.Role = "Admin"; // Default role set to Admin
                _context.UserAccountProjects.Add(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Admin user created successfully!";
                return RedirectToAction("Index");
            }

            TempData["ErrorMessage"] = "Failed to create user. Please check the details.";
            return View(user);
        }


        // Customer Home action
        public async Task<IActionResult> Customer_Home()
        {
            var discountedItems = await _context.ItemProjects
                .Where(i => !string.IsNullOrEmpty(i.Discount) && i.Discount != " " && i.Quantity > 0) // Exclude items with no stock
                .ToListAsync();

            return View(discountedItems);
        }

        // Admin Home action
        public IActionResult Admin_Home()
        {
            var adminName = HttpContext.Session.GetString("UserName") ?? "Admin";
            ViewBag.AdminName = adminName;
            return View();
        }


        // Email action
        public IActionResult Email()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Email(string email, string subject, string message)
        {
            try
            {
                // SMTP Configuration
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587, 
                    Credentials = new NetworkCredential("faisalaljumah0@gmail.com", "rpkv dhkj zref ibfd"),
                    EnableSsl = true
                };

                // Email Message 
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("faisalaljumah0@gmail.com"),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true 
                };

                mailMessage.To.Add(email);

                // Send the Email
                smtpClient.Send(mailMessage);

                ViewBag.EmailStatus = $"Email successfully sent to {email}.";
            }
            catch (Exception ex)
            {
                ViewBag.EmailStatus = $"An error occurred while sending the email: {ex.Message}";
            }

            return View();
        }


        // Login action
        public IActionResult Login()
        {
            var autoLoginUser = Request.Cookies["AutoLoginUser"];
            var autoLoginRole = Request.Cookies["AutoLoginRole"];

            if (!string.IsNullOrEmpty(autoLoginUser) && !string.IsNullOrEmpty(autoLoginRole))
            {
                // Save user details in session
                HttpContext.Session.SetString("UserName", autoLoginUser);
                HttpContext.Session.SetString("Role", autoLoginRole);

                // Redirect based on role
                if (autoLoginRole == "Admin")
                {
                    return RedirectToAction("Admin_Home");
                }
                else if (autoLoginRole == "Customer")
                {
                    return RedirectToAction("Customer_Home");
                }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string name, string pass)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(pass))
            {
                ViewBag.ErrorMessage = "Please enter both username and password.";
                return View();
            }

            // Perform case-sensitive comparison for username
            var user = await _context.UserAccountProjects
                .FirstOrDefaultAsync(u => u.Name == name && u.Pass == pass);

            if (user != null)
            {
                // Save user details in session
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("Role", user.Role);

                // Redirect based on role
                if (user.Role == "Admin")
                {
                    return RedirectToAction("Admin_Home", "UserAccount");
                }
                else if (user.Role == "Customer")
                {
                    return RedirectToAction("Customer_Home", "UserAccount");
                }
            }

            // Case-sensitive login failed
            ViewBag.ErrorMessage = "Invalid username or password.";
            return View();
        }


        // Logout action
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            // Remove auto-login cookies
            Response.Cookies.Delete("AutoLoginUser");
            Response.Cookies.Delete("AutoLoginRole");

            return RedirectToAction("Login");
        }


        private bool UserExists(int id)
        {
            return _context.UserAccountProjects.Any(e => e.Id == id);
        }
    }
}
