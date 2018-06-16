#region Using

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.OData;
using System.Web.OData.Routing;
using ODataExamples.API.Classes;
using ODataExamples.Repository.Model;

#endregion

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

        #region Customer

        #region GET Customers

        // GET: customers
        /// <summary>Query customers</summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        [EnableQuery(MaxExpansionDepth = 5)]
        [HttpGet]
        [ODataRoute("customers")]
        [ResponseType(typeof(IEnumerable<Customer>))]
        public async Task<IHttpActionResult> GetCustomers() {
            try {
                // Look for all customers - can be dangerous if no query properties are provided
                var customers = _db.Customers;
                if (!await customers.AnyAsync()){
                    // Data not available, return 404
                    return NotFound();
                }
                // Return customers
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
        public async Task<IHttpActionResult> GetCustomer([FromODataUri] int id){
            try{
                // Attempt to find customer
                var customer = await _db.Customers.FindAsync(id);
                if (customer == null){
                    // Customer not found, return 404
                    return NotFound();
                }
                // Return customer to caller
                return Ok(customer);
            }
            catch (Exception ex){
                return InternalServerError(ex);
            }
        }

        #endregion

        #region POST Customer

        /// <summary>Create a new customer</summary>
        /// <param name="customer"></param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="409">Conflict</response>
        [EnableQuery]
        [HttpPost]
        [ODataRoute("customers")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PostCustomer([FromBody] Customer customer) {
            try{
                // Ensure incoming request adheres to entity expectations
                if (!ModelState.IsValid) {
                    // Something isn't right, return 400
                    return BadRequest(ModelState);
                }

                // Check to see if incoming customer already exists based on email address
                var dbCustomer = _db.Customers.Where(o => o.email_address == customer.email_address);
                if (dbCustomer.Any()) {
                    // Existing customer exists, return 409
                    return Conflict();
                }

                // Write tattler values (just for illustration - tie in your authorized claim token for user)
                customer.inserted_datetime = DateTime.UtcNow;
                customer.inserted_by = "OData Examples API";
                customer.last_updated_datetime = DateTime.UtcNow;
                customer.last_updated_by = "OData Examples API";

                // Add customer to collection
                _db.Customers.Add(customer);

                // Commit creation to database
                await _db.SaveChangesAsync();

                // Return newly added customer to caller
                return Created(customer);
            }
            catch (Exception ex) {
                //Send exception detail to insights
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
        }

        #endregion

        #region PATCH Customer

        /// <summary>Update customer</summary>
        /// <param name="id">Customer Id</param>
        /// <param name="customerDelta">Customer delta</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="409">Conflict</response>
        [AcceptVerbs("PATCH", "MERGE")]
        [EnableQuery]
        [HttpPatch]
        [ODataRoute("customers({id})")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PatchCustomer([FromODataUri] int id, [FromBody] Delta<Customer> customerDelta) {

            // Ensure incoming request adheres to entity expectations
            if (!ModelState.IsValid) {
                // Something isn't right, return 400
                return BadRequest(ModelState);
            }

            // Attempt to find existing customer
            var dbCustomer = await _db.Customers.FindAsync(id);
            if (dbCustomer == null) {
                // Requested customer already exists, return 409
                return Conflict();
            }

            try {
                // Update date tattler value
                dbCustomer.last_updated_datetime = DateTime.UtcNow;

                // Issue Patch to existing record
                customerDelta.Patch(dbCustomer);
                
                // Commit change to database
                await _db.SaveChangesAsync();
            }
            catch (Exception ex) {
                //Send exception detail to insights
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
            // Return updated customer record to caller
            return Updated(dbCustomer);
        }

        #endregion

        #region PUT Customer

        // PUT: customers(5)
        /// <summary>
        /// Overwrite customer
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
        public async Task<IHttpActionResult> PutCustomer([FromODataUri] int id, [FromBody] Customer customer) {

            // Attempt to find existing customer
            var dbCustomer = await _db.Customers.FindAsync(id);
            if (dbCustomer == null) {
                // Requested customer not found, return 404
                return NotFound();
            }

            try {
                // Update tattler values (just for illustration - tie in your authorized claim token for user)
                customer.last_updated_datetime = DateTime.UtcNow;
                customer.last_updated_by = "OData Examples API";

                // Replace customer data
                dbCustomer = customer;

                // Commit change to database
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex) {
                if (!_db.Customers.Any(c => c.id == id)) {
                    return NotFound();
                }
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
            catch (Exception ex) {
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
            // Return updated customer to caller
            return Updated(dbCustomer);
        }
        #endregion

        #region DELETE Customer

        // DELETE: customers(5)
        /// <summary>Delete customer</summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">The market id</param>
        /// <response code="204">No Content</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found</response>
        [EnableQuery]
        [HttpDelete]
        [ODataRoute("customers({id})")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> DeleteCustomer([FromODataUri] int id) {
            try {
                // Attempt to find existing customer
                var dbCustomer = await _db.Customers.FindAsync(id);
                if (dbCustomer == null) {
                    // Requested customer not found, return 404
                    return NotFound();
                }

                // Remove customer record
                _db.Customers.Remove(dbCustomer);

                // Commit change to database
                await _db.SaveChangesAsync();

                // Return successful deletion code (204)
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception ex) {
                //Send exception detail to insights
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
        }

        #endregion

        #endregion

        #region Handle References

        #region CreateRef

        /// <summary>
        ///   Create reference between customer and address
        /// </summary>
        /// <param name="key"></param>
        /// <param name="navigationProperty"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri reference) {
            var customer = await _db.Customers.SingleOrDefaultAsync(p => p.id == key);
            if (customer == null) {
                return NotFound();
            }
            switch (navigationProperty) {
                case "Addresses":
                    var relatedKey = ReferenceHelper.GetKeyFromUri<int>(Request, reference);
                    var addresses = _db.Addresses.Where(f => f.id == relatedKey).ToList();
                    if (addresses.Count == 0) {
                        return NotFound();
                    }

                    customer.Addresses = addresses;
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await _db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        #endregion

        #region DeleteRef

        /// <summary>
        ///   Remove reference between customer and address
        /// </summary>
        /// <param name="key"></param>
        /// <param name="navigationProperty"></param>
        /// <param name="relatedKey"></param>
        /// <returns></returns>
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
            string navigationProperty, [FromODataUri] string relatedKey) {
            var customer = _db.Customers.Include("Addresses").SingleOrDefault(p => p.id == key);
            if (customer == null) {
                return NotFound();
            }

            switch (navigationProperty) {
                case "addresses":
                    var addresses = customer.Addresses.SingleOrDefault(p => p.id == Convert.ToInt32(relatedKey));
                    if (addresses == null) {
                        return NotFound();
                    }

                    customer.Addresses.Remove(addresses);
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await _db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        #endregion

        #endregion

        protected override void Dispose(bool disposing) {
            if (disposing) {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
