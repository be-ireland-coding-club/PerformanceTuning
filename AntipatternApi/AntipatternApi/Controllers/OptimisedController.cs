using AntipatternApi;
using DataContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using System.Globalization;
using System.Xml.Linq;
using Azure.Core.Pipeline;
using System.Net.Http;
using System.Drawing;
using Microsoft.IdentityModel.Tokens;

namespace AntiPatternApi.Controllers
{
    [ApiController]
    [Route("optimised")]
    public class OptimisedController : ControllerBase
    {
        private readonly ILogger<OptimisedController> _logger;
        private readonly AdventureWorks2022Context _context;
        private readonly IMemoryCache _cache;


        public OptimisedController(ILogger<OptimisedController> logger, AdventureWorks2022Context context, IMemoryCache memoryCache)
        {
            _logger = logger;
            _context = context;
            _cache = memoryCache;
        }

        #region 1. Chatty IO
        [HttpGet("chatty-io/get-products-in-subcategory")]
        public async Task<IActionResult> GetProductCategoryDetailsAsync(int subCategoryId)
        {
                var subCategory = await _context.ProductSubcategories
                        .Where(psc => psc.ProductSubcategoryId == subCategoryId)
                        .Include("Products.ProductListPriceHistories")
                        .FirstOrDefaultAsync();

                if (subCategory == null)
                    return NotFound();

                return Ok(subCategory);            
        }
        #endregion 1. Chatty IO

        #region 2. Extraneous Fetching
        [HttpGet("extraneous-fetching/get-product-by-colour")]
        public async Task<IActionResult> GetProductByColour(string colour)
        {
            // Execute the query. This happens at the database.
            var products = _context.Products.Where(x => x.Color != null
                                                      && x.Color.Contains(colour));

            // Project fields from the query results. This happens in application memory.
            var result = products.Select(p => new ProductInformation { Id = p.ProductId, Name = p.Name }).ToList();
            return Ok(result);
        }
        #endregion 2. Extraneous Fetching

        #region 3. No Caching
        [HttpGet("no-caching/get-person-by-id")]
        public async Task<IActionResult> GetAsync(int id)
        {
            // Attempt to retrieve the person from cache
            if (_cache.TryGetValue($"Person:{id}", out Person cachedPerson))
            {
                return Ok(cachedPerson);
            }

            // Person not found in cache, fetch from database
            var person = await _context.People
                .Where(p => p.BusinessEntityId == id)
                .FirstOrDefaultAsync();

            // Cache the person for future requests
            if (person != null)
            {
                _cache.Set($"Person:{id}", person, TimeSpan.FromMinutes(10)); // Example: cache for 10 minutes
            }

            return Ok(person);
        }
        #endregion 3. No Caching

        #region 5. Busy Database
        [HttpGet("busy-database/get-xml")]
        public async Task<IActionResult> Get(int territoryId)
        {
            var orders = _context.SalesOrderHeaders
                                .Where(soh => soh.TerritoryId == territoryId)
                                .OrderByDescending(soh => soh.TotalDue)
                                .Take(20)
                                .Include(soh => soh.Customer)
                                .ThenInclude(c => c.Person)
                                .Include(soh => soh.SalesOrderDetails)
                                .Select(soh => new
                                {
                                    OrderNumber = soh.SalesOrderNumber,
                                    Status = soh.Status,
                                    ShipDate = soh.ShipDate,
                                    OrderDateYear = soh.OrderDate.Year,
                                    OrderDateMonth = soh.OrderDate.Month,
                                    DueDate = soh.DueDate,
                                    SubTotal = soh.SubTotal,
                                    TaxAmt = soh.TaxAmt,
                                    TotalDue = soh.TotalDue,
                                    ReviewRequired = soh.TotalDue > 5000 ? "Y" : "N",
                                    Customer = soh.Customer,
                                    LineItems = soh.SalesOrderDetails
                                        .OrderBy(sod => sod.SalesOrderDetailId)
                                        .Select(sod => new
                                        {
                                            Quantity = sod.OrderQty,
                                            UnitPrice = sod.UnitPrice,
                                            LineTotal = sod.LineTotal,
                                            ProductId = sod.ProductId,
                                            InventoryCheckRequired = (sod.ProductId >= 710 && sod.ProductId <= 720 && sod.OrderQty >= 5) ? "Y" : "N"
                                        }).ToList()
                                }).ToList();

            var xml = new XElement("Orders",
                orders.Select(o => new XElement("Order",
                    new XAttribute("OrderNumber", o.OrderNumber ?? string.Empty),
                    new XAttribute("Status", o.Status),
                    new XAttribute("ShipDate", o.ShipDate?.ToString("o") ?? string.Empty),
                    new XAttribute("OrderDateYear", o.OrderDateYear),
                    new XAttribute("OrderDateMonth", o.OrderDateMonth),
                    new XAttribute("DueDate", o.DueDate.ToString("o")),
                    new XAttribute("SubTotal", o.SubTotal.ToString("C", CultureInfo.CurrentCulture)),
                    new XAttribute("TaxAmt", o.TaxAmt.ToString("C", CultureInfo.CurrentCulture)),
                    new XAttribute("TotalDue", o.TotalDue.ToString("C", CultureInfo.CurrentCulture)),
                    new XAttribute("ReviewRequired", o.ReviewRequired),
                    o.Customer != null ? new XElement("Customer",
                        new XAttribute("AccountNumber", o.Customer.AccountNumber ?? string.Empty),
                        new XAttribute("FullName", $"{(o.Customer.Person?.Title ?? string.Empty)} {(o.Customer.Person?.FirstName ?? string.Empty)} {(o.Customer.Person?.MiddleName ?? string.Empty)} {(o.Customer.Person?.LastName ?? string.Empty)} {(o.Customer.Person?.Suffix ?? string.Empty)}".Trim().ToUpper())
                    ) : null,
                    new XElement("OrderLineItems",
                        o.LineItems.Select(li => new XElement("LineItem",
                            new XAttribute("Quantity", li.Quantity),
                            new XAttribute("UnitPrice", li.UnitPrice.ToString("C", CultureInfo.CurrentCulture)),
                            new XAttribute("LineTotal", li.LineTotal.ToString("C", CultureInfo.CurrentCulture)),
                            new XAttribute("ProductId", li.ProductId),
                            new XAttribute("InventoryCheckRequired", li.InventoryCheckRequired)
                        ))
                    )
                ))
            );

            return new ContentResult
            {
                Content = xml.ToString(),
                ContentType = "application/xml",
                StatusCode = 200
            };        
        }
        #endregion 5. Busy Database

        #region 6. Synchronous IO
        [HttpGet("synchronous-io/get-people-named-jim")]
        public IActionResult AsynchronousIO()
        {
            Helper.DoAsynchronousOperation();

            var people = _context.People.Where(x => x.FirstName == "Jim");

            return Ok(people);
        }
        #endregion 6. Synchronous IO

        #region 7. Busy Front End
        //public OptimisedController()
        //{
        //var serviceBusConnectionString = ...;
        //QueueName = ...;
        //ServiceBusQueueHandler = new ServiceBusQueueHandler(serviceBusConnectionString);
        //QueueClient = ServiceBusQueueHandler.GetQueueClientAsync(QueueName).Result;
        //}

        [HttpGet("busy-front-end")]
        public async Task<IActionResult> NotBusyFrontEnd()
        {
            //var response = await ServiceBusQueueHandler.AddWorkLoadToQueueAsync(QueueClient, QueueName, 0);

            return Ok();
        }
        #endregion 7. Busy Front End

    }
}
