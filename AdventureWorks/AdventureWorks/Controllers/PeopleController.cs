using Microsoft.AspNetCore.Mvc;
using DataContext;

namespace AdventureWorks.Controllers
{
    [ApiController]
    [Route("people")]
    public class PeopleController : ControllerBase
    {
        private readonly ILogger<PeopleController> _logger;
        private readonly AdventureWorks2022Context _context;


        public PeopleController(ILogger<PeopleController> logger, AdventureWorks2022Context context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "get-person")]
        public Person Get(int id)
        {
            return _context.People.Where(x => x.BusinessEntityId == id).FirstOrDefault();
        }
    }
}

