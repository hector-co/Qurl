using Microsoft.AspNetCore.Mvc;
using Qurl.Abstractions;
using Sample.Models;

namespace Sample.Controllers
{
    [Produces("application/json")]
    [Route("api/People")]
    public class PeopleController : Controller
    {
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Ok(new Person
            {
                Id = id
            });
        }

        [HttpGet]
        public IActionResult Get([FromQuery]Query<PersonFilter> query)
        {
            return Ok();
        }
    }
}