using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Antiforgery;

namespace Alquileres.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CSRFTokenController : ControllerBase
    {
        private readonly IAntiforgery _antiforgery;

        public CSRFTokenController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        [HttpGet("token")]
        [IgnoreAntiforgeryToken]
        public IActionResult GetCsrfToken()
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            return Ok(new { token = tokens.RequestToken });
        }
    }
}