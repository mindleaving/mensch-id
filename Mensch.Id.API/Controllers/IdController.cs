using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mensch.Id.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdController : ControllerBase
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public IdController(
            IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("random")]
        public async Task<IActionResult> GetRandomId([FromQuery] DateTime birthdate)
        {
            var idGenerator = new MenschIdGenerator();
            var id = idGenerator.Generate(birthdate);
            return Ok(id);
        }

        [HttpGet("anonymous/random")]
        public async Task<IActionResult> GetAnonymousRandomId()
        {
            var idGenerator = new MenschIdGenerator();
            var id = idGenerator.GenerateAnonymous();
            return Ok(id);
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> MyId()
        {
            var username = ControllerHelpers.GetUsername(httpContextAccessor);
            var name = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
            return Ok(name?.Value ?? username);
        }

    }
}
