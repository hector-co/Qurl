using Microsoft.AspNetCore.Mvc;
using Qurl.Abstractions;
using Qurl.Abstractions.Queryable;
using Qurl.Samples.AspNetCore.Models;
using System.Linq;

namespace Qurl.Samples.AspNetCore.Controllers
{
    [Produces("application/json")]
    [Route("api/people")]
    public class PeopleController : Controller
    {
        private readonly SampleContext _context;

        public PeopleController(SampleContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Ok();
        }

        [HttpGet]
        public IActionResult Get([FromQuery]Query<PersonFilter> query)
        {
            var result = _context.Set<Person>().ApplyQuery(query);
            return Ok(result.ToList());
        }
    }
}