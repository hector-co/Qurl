using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qurl.Samples.AspNetCore.Models;

namespace Qurl.Samples.AspNetCore.Controllers
{
    [Produces("application/json")]
    [Route("api/people")]
    public class PeopleController : Controller
    {
        private readonly SampleContext _context;
        private readonly QueryBuilder _queryBuilder;

        public PeopleController(SampleContext context, QueryBuilder queryBuilder)
        {
            _context = context;
            _queryBuilder = queryBuilder;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var result = _context.Set<Person>().Include(p => p.Group).FirstOrDefault(p => p.Id == id);
            return Ok(result);
        }

        [HttpGet]
        public IActionResult Get([FromQuery] QueryParams queryParams)
        {
            var query = _queryBuilder.CreateQuery<PersonFilter>(queryParams);
            var result = _context.Set<Person>().Include(p => p.Group)
                .ApplyQuery(query).ToList();
            return Ok(result);
        }
    }
}