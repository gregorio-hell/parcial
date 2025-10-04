using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace parcial.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            var userInfo = new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                UserName = User.Identity?.Name ?? "No Name",
                Roles = User.Claims.Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                                  .Select(c => c.Value).ToList(),
                AllClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            };

            return Json(userInfo);
        }

        [Authorize(Roles = "Coordinador")]
        public IActionResult CoordinatorTest()
        {
            return Json(new { Message = "¡Acceso autorizado como Coordinador!", User = User.Identity?.Name });
        }
    }
}