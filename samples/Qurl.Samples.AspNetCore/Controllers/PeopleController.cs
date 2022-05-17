using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qurl.Queryable;
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
        public IActionResult Get(int id, System.Threading.CancellationToken cancellationToken)
        {
            var result = _context.Set<Person>().Include(p => p.Group).FirstOrDefault(p => p.Id == id);
            return Ok(result);
        }

        [HttpGet]
        public IActionResult Get([FromQuery] Query<PersonFilter> query)
        {
            var queryable = _context.Set<Person>().Include(p => p.Group).ApplyQuery(query);
            queryable = queryable.ApplySortAndPaging(query);
            var result = queryable.ToList();
            return Ok(result);
        }
    }
}