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

        #region Base Customer

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
        public async Task<IHttpActionResult> GetCustomer([FromODataUri] int id){
            try{
                var customer = await _db.Customers.FindAsync(id);
                if (customer == null){
                    return NotFound();
                }
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

            var dbCustomer = await _db.Customers.FindAsync(id);
            if (dbCustomer == null) {
                return NotFound();
            }

            try {
                dbCustomer = customer;
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

        #endregion

        #region Customer Address

        #region GET Customer Addresses

        // GET: addresses
        /// <summary>Query customer address</summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        [EnableQuery(MaxExpansionDepth = 5)]
        [HttpGet]
        [ODataRoute("addresses")]
        [ResponseType(typeof(IEnumerable<Address>))]
        public async Task<IHttpActionResult> GetCustomerAddresses() {
            try {
                var addresses = _db.Addresses;
                if (!await addresses.AnyAsync()){
                    return NotFound();
                }
                return Ok(addresses);
            }
            catch (Exception ex){
                return InternalServerError(ex);
            }
        }

        // GET: addresses(1)
        /// <summary>Query customer address</summary>
        /// <param name="id">Address id</param>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        [EnableQuery(MaxExpansionDepth = 5)]
        [HttpGet]
        [ODataRoute("addresses({id})")]
        [ResponseType(typeof(IEnumerable<Address>))]
        public async Task<IHttpActionResult> GetCustomerAddress([FromODataUri] int id) {
            try {
                var customerAddress = await _db.Addresses.FindAsync(id);
                if (customerAddress == null){
                    return NotFound();
                }
                return Ok(customerAddress);
            }
            catch (Exception ex){
                return InternalServerError(ex);
            }
        }

        #endregion

        #region POST Customer Address

        /// POST: addresses
        /// <summary>Create a new customer address</summary>
        /// <param name="address">Customer Address</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="409">Conflict</response>
        [EnableQuery]
        [HttpPost]
        [ODataRoute("addresses")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PostCustomerAddress([FromBody] Address address) {
            try{
                if (!ModelState.IsValid) {
                    return BadRequest(ModelState);
                }

                var dbAddress = _db.Addresses.Where(c => c.street_address_1 == address.street_address_1);
                if (dbAddress.Any()) {
                    return Conflict();
                }

                _db.Addresses.Add(address);
                await _db.SaveChangesAsync();
                return Created(dbAddress);
            }
            catch (Exception ex) {
                //Send exception detail to insights
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
        }

        #endregion

        #region PATCH Customer Address

        /// PATCH: addresses({addressId})
        /// <summary>Update customer's address</summary>
        /// <param name="id">Address Id</param>
        /// <param name="addressDelta">Address delta</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="409">Conflict</response>
        [AcceptVerbs("PATCH", "MERGE")]
        [EnableQuery]
        [HttpPatch]
        [ODataRoute("addresses({id})")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PatchCustomerAddress([FromODataUri] int id, [FromBody] Delta<Address> addressDelta) {

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var dbAddress = await _db.Addresses.FindAsync(id);
            if (dbAddress == null) {
                return NotFound();
            }

            try {
                addressDelta.Patch(dbAddress);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex) {
                //Send exception detail to insights
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
            return Updated(dbAddress);
        }

        #endregion

        #region PUT Customer Address

        /// PUT: addresses({addressId})
        /// <summary>
        /// Overwrite customer address
        /// </summary>
        /// <param name="id">Address Id</param>
        /// <param name="address">Address</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="409">Conflict</response>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [EnableQuery]
        [HttpPut]
        [ODataRoute("addresses({id})")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutCustomerAddress([FromODataUri] int id, [FromBody] Address address) {

            var dbAddress = await _db.Addresses.FindAsync(id);
            if (dbAddress == null) {
                return NotFound();
            }

            try {
                dbAddress = address;
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex) {
                if (!_db.Addresses.Any(c => c.id == id)) {
                    return NotFound();
                }
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
            catch (Exception ex) {
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
            return Updated(dbAddress);
        }
        #endregion

        #region DELETE Customer Address

        // DELETE: addresses(5)
        /// <summary>Delete address</summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">The address id</param>
        /// <response code="204">No Content</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found</response>
        [EnableQuery]
        [HttpDelete]
        [ODataRoute("addresses({id})")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> DeleteCustomerAddress([FromODataUri] int id) {
            try {
                var dbCustomer = await _db.Addresses.FindAsync(id);
                if (dbCustomer == null) {
                    return NotFound();
                }
                _db.Addresses.Remove(dbCustomer);
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

        #endregion

        protected override void Dispose(bool disposing) {
            if (disposing) {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
