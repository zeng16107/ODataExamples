using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.OData;
using System.Web.OData.Routing;
using Newtonsoft.Json;
using ODataExamples.API.Classes;
using ODataExamples.Repository.Model;

namespace ODataExamples.API.Controllers
{
    public class CustomersController : ODataController
    {
        // EF Model for database interation(s)
        private readonly ODataSamplesEntities _db = new ODataSamplesEntities();

        // Telemetry tracker to write exceptions to App Insights
        private readonly TelemetryTracker _tracker = new TelemetryTracker();

        #region GETs

        // GET: custoemrs
        /// <summary>Query customers</summary>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        [EnableQuery(MaxExpansionDepth = 5)]
        [HttpGet]
        [ODataRoute("customers")]
        [ResponseType(typeof(IEnumerable<Customer>))]
        public async Task<IHttpActionResult> Get()
        {
            try
            {
                var customers = _db.Customers;
                if (!await customers.AnyAsync())
                {
                    return NotFound();
                }
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: customers(5)
        /// <summary>Query customer by id</summary>
        /// <param name="id">Customer id</param>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        [EnableQuery(MaxExpansionDepth = 4)]
        [HttpGet]
        [ODataRoute("customers({id})")]
        [ResponseType(typeof(Customer))]
        public async Task<IHttpActionResult> Get([FromODataUri] int id)
        {
            try
            {
                var customer = _db.Customers.Where(c => c.id == id);
                if (!await customer.AnyAsync())
                {
                    return NotFound();
                }
                return Ok(SingleResult.Create(customer));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #endregion

        #region POST

        /// <summary>Create a new customer</summary>
        /// <param name="customer"></param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="409">Conflict</response>
        //[Authorize]
        [EnableQuery]
        [HttpPost]
        [ODataRoute("customers")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Post([FromBody] Customer customer)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var dbCustomer = _db.Customers.Where(o => o.email_address == customer.email_address);
                if (dbCustomer.Any())
                {
                    return Conflict();
                }

                _db.Customers.Add(customer);
                await _db.SaveChangesAsync();

                return Created(customer);
            }
            catch (Exception ex)
            {
                //Send exception detail to insights
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
