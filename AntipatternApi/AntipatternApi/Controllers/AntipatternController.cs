using AntipatternApi;
using DataContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Text;
using static AntiPatternApi.Controllers.AntipatternController;

namespace AntiPatternApi.Controllers
{
    [ApiController]
    [Route("antipattern")]
    public class AntipatternController : ControllerBase
    {
        private readonly ILogger<AntipatternController> _logger;
        private readonly AdventureWorks2022Context _context;

        public AntipatternController(ILogger<AntipatternController> logger, AdventureWorks2022Context context)
        {
            _logger = logger;
            _context = context;
        }

        #region 1. Chatty IO
        [HttpGet("chatty-io/get-products-in-subcategory")]
        public async Task<IActionResult> GetProductsInSubCategoryAsync(int subcategoryId)
        {
            // Get product subcategory.
            var productSubcategory = await _context.ProductSubcategories
                    .Where(psc => psc.ProductSubcategoryId == subcategoryId)
                    .FirstOrDefaultAsync();

            // Find products in that category.
            productSubcategory.Products = await _context.Products
                .Where(p => subcategoryId == p.ProductSubcategoryId)
                .ToListAsync();

            // Find price history for each product.
            foreach (var prod in productSubcategory.Products)
            {
                int productId = prod.ProductId;
                var productListPriceHistory = await _context.ProductListPriceHistories
                    .Where(pl => pl.ProductId == productId)
                    .ToListAsync();
                prod.ProductListPriceHistories = productListPriceHistory;
            }
            return Ok(productSubcategory);
        }
        #endregion 1. Chatty IO

        #region 2. Extraneous Fetching
        [HttpGet("extraneous-fetching/get-product-by-colour")]
        public async Task<IActionResult> GetProductByColour(string colour)
        {
            // Execute the query. This happens at the database.
            var products = _context.Products.ToList().Where(x => !x.Color.IsNullOrEmpty()
                                                      && x.Color.Contains(colour));

            // Project fields from the query results. This happens in application memory.
            var result = products.Select(p => new ProductInformation { Id = p.ProductId, Name = p.Name });
            return Ok(result);
        }
        #endregion 2. Extraneous Fetching

        #region 3. No Caching
        [HttpGet("no-caching/get-person-by-id")]
        public async Task<IActionResult> GetAsync(int id)
        {
            var person = await _context.People
                    .Where(p => p.BusinessEntityId == id)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);

            return Ok(person);
        }
        #endregion 3. No Caching

        #region 4. Retry Storm
        [HttpGet("retry-storm/fail")]
        public async Task<IActionResult> RetryStorm()
        {
            return StatusCode(500, "Simulated failure");
        }
        #endregion 4. Retry Storm

        #region 5. Busy Database
        [HttpGet("busy-database/get-xml")]
        public async Task<IActionResult> Get(int territoryId)
        {
            var sqlQuery = $"EXEC GetSalesOrdersByTerritory @TerritoryId = {territoryId}";

            string xmlResult = string.Empty;

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sqlQuery;
                _context.Database.OpenConnection();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        xmlResult += reader[0]?.ToString();
                    }
                }
            }            

            return new ContentResult
            {
                Content = xmlResult,
                ContentType = "application/xml",
                StatusCode = 200
            };
        }
        #endregion 5. Busy Database

        #region 6. Synchronous IO
        [HttpGet("synchronous-io/get-people-named-jim")]
        public IActionResult SynchronousIO()
        {
            Helper.DoSynchronousOperation();

            var people = _context.People.Where(x => x.FirstName == "Jim");

            return Ok(people);
        }
        #endregion 6. Synchronous IO

        #region 7. Busy Front End
        [HttpGet("busy-front-end")]
        public IActionResult BusyFrontEnd()
        {
            new Thread(() =>
            {
                //Simulate processing
                Thread.SpinWait(Int32.MaxValue / 100);
            }).Start();

            return Ok();
        }
        #endregion 7. Busy Front End
    }
}
