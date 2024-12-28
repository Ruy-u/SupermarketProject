using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupermarketProject.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SupermarketProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetNameAPIController : ControllerBase
    {
        private readonly SupermarketDbContext _context;

        public GetNameAPIController(SupermarketDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromQuery] string role)
        {
            // Validate the role parameter
            if (string.IsNullOrWhiteSpace(role))
            {
                return BadRequest(new { Message = "Role parameter is required." });
            }

            // Convert role to lowercase for case-insensitive comparison
            var users = await _context.UserAccountProjects
                .Where(u => u.Role.ToLower() == role.ToLower()) // Case-insensitive comparison
                .Select(u => new { u.Name })
                .ToListAsync();

            if (!users.Any())
            {
                return NotFound(new { Message = $"No users found with the role '{role}'." });
            }

            return Ok(users);
        }
    }
}
