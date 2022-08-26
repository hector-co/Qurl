using Microsoft.AspNetCore.Mvc;

namespace Qurl.Samples.AspNetCore.Controllers
{
    [Produces("application/json")]
    [Route("api/groups")]
    public class GroupsController : Controller
    {
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Ok();
        }

        [HttpGet]
        public IActionResult Get([FromQuery]QueryParams query)
        {
            return Ok();
        }
    }
}