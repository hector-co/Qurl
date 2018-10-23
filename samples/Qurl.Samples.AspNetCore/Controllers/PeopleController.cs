using Microsoft.AspNetCore.Mvc;
using Qurl.Abstractions;
using Qurl.Samples.AspNetCore.Models;

namespace Qurl.Samples.AspNetCore.Controllers
{
    [Produces("application/json")]
    [Route("api/people")]
    public class PeopleController : Controller
    {
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Ok();
        }

        [HttpGet]
        public IActionResult Get([FromQuery]Query<PersonFilter> query)
        {
            return Ok();
        }
    }
}