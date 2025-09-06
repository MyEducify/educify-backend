using Database.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.DBContext;

namespace UserService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly UserDbContext _db;
        public ProfilesController(UserDbContext db) { _db = db; }

        [HttpGet("me")]
        public async Task<IActionResult> Me(string User_Id)
        {

            var profile = await _db.user.Where(p => p.Id == User_Id).FirstOrDefaultAsync();
            if (profile == null) return NotFound();

            return Ok(profile);
        }
    }
}
