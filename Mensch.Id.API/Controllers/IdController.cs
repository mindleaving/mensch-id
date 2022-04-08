using System;
using System.Threading.Tasks;
using Mensch.Id.API.Workflow;
using Microsoft.AspNetCore.Mvc;

namespace Mensch.Id.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdController : ControllerBase
    {
        [HttpGet("random")]
        public async Task<IActionResult> GetRandomId([FromQuery] DateTime birthdate)
        {
            var idGenerator = new MenschIdGenerator();
            var id = idGenerator.Generate(birthdate);
            return Ok(id);
        }

    }
}
