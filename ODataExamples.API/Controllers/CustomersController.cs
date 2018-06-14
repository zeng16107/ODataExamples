using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.OData;
using System.Web.OData.Routing;
using ODataExamples.API.Classes;
using ODataExamples.Repository.Model;

namespace ODataExamples.API.Controllers
{
    /// <summary>
    /// Handles operations related to Customer data
    /// </summary>
    public class CustomersController : ODataController
    {
        // EF Model for database interation(s)
        private readonly ODataSamplesEntities _db = new ODataSamplesEntities();

        // Telemetry tracker to write exceptions to App Insights
        private readonly TelemetryTracker _tracker = new TelemetryTracker();

        #region GETs

        // GET: custoemrs
        /// <summary>Query customers</summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        [EnableQuery(MaxExpansionDepth = 5)]
        [HttpGet]
        [ODataRoute("customers")]
        [ResponseType(typeof(IEnumerable<Customer>))]
        public async Task<IHttpActionResult> Get() {
            try {
                var customers = _db.Customers;
                if (!await customers.AnyAsync()){
                    return NotFound();
                }
                return Ok(customers);
            }
            catch (Exception ex){
                return InternalServerError(ex);
            }
        }

        // GET: customers(5)
        /// <summary>Query customer by id</summary>
        /// <param name="id">Customer id</param>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        [EnableQuery(MaxExpansionDepth = 5)]
        [HttpGet]
        [ODataRoute("customers({id})")]
        [ResponseType(typeof(Customer))]
        public async Task<IHttpActionResult> Get([FromODataUri] int id){
            try{
                var customer = _db.Customers.Where(c => c.id == id);
                if (!await customer.AnyAsync()){
                    return NotFound();
                }
                return Ok(SingleResult.Create(customer));
            }
            catch (Exception ex){
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
        [EnableQuery]
        [HttpPost]
        [ODataRoute("customers")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Post([FromBody] Customer customer) {
            try{
                if (!ModelState.IsValid) {
                    return BadRequest(ModelState);
                }

                var dbCustomer = _db.Customers.Where(o => o.email_address == customer.email_address);
                if (dbCustomer.Any()) {
                    return Conflict();
                }

                _db.Customers.Add(customer);
                await _db.SaveChangesAsync();
                return Created(customer);
            }
            catch (Exception ex) {
                //Send exception detail to insights
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
        }

        #endregion

        #region PATCH

        /// <summary>Create a new customer</summary>
        /// <param name="id">Customer Id</param>
        /// <param name="customerDelta">Customer delta</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="409">Conflict</response>
        [AcceptVerbs("PATCH", "MERGE")]
        [EnableQuery]
        [HttpPatch]
        [ODataRoute("customers")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Patch([FromODataUri] int id, [FromBody] Delta<Customer> customerDelta) {

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var dbCustomer = await _db.Customers.FindAsync(id);
            if (dbCustomer == null) {
                return Conflict();
            }

            try {
                customerDelta.Patch(dbCustomer);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex) {
                //Send exception detail to insights
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
            return Updated(dbCustomer);
        }

        #endregion

        #region PUT
        /// <summary>customer
        /// </summary>
        /// <param name="id"></param>
        /// <param name="customer"></param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="409">Conflict</response>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [EnableQuery]
        [HttpPut]
        [ODataRoute("customers({id})")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Put([FromODataUri] int id, [FromBody] Customer customer) {

            var dbCustomer = _db.Customers.Find(id);
            if (dbCustomer == null) {
                return NotFound();
            }

            try {
                dbCustomer = customer;
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex) {
                if (!_db.Customers.Any(c => c.Id == id)) {
                    return NotFound();
                }
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
            catch (Exception ex) {
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
            return Updated(dbCustomer);
        }
        #endregion

        #region DELETE

        // DELETE: customers(5)
        /// <summary>Delete customer</summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">The market id</param>
        /// <response code="204">No Content</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found</response>
        //[Authorize(Roles = "NationalPurchasing,ZonePurchasing,DivisionPurchasing")]
        [EnableQuery]
        [HttpDelete]
        [ODataRoute("customers({id})")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Delete([FromODataUri] int id) {
            try {
                var dbCustomer = await _db.Customers.FindAsync(id);
                if (dbCustomer == null) {
                    return NotFound();
                }
                _db.Customers.Remove(dbCustomer);
                await _db.SaveChangesAsync();
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception ex) {
                //Send exception detail to insights
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
        }

        #endregion

        protected override void Dispose(bool disposing) {
            if (disposing) {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
