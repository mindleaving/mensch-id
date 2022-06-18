using System;
using System.Threading.Tasks;
using Mensch.Id.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Mensch.Id.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssignerController : ControllerBase
    {
        [HttpPost("create")]
        public async Task<IActionResult> CreateNew(
            [FromBody] Person body)
        {
            throw new NotImplementedException();
        }

    }
}
