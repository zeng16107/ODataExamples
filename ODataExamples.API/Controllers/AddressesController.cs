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
    /// Handles operations related to Address data
    /// </summary>
    public class AddressesController : ODataController
    {
        // EF Model for database interation(s)
        private readonly ODataSamplesEntities _db = new ODataSamplesEntities();

        // Telemetry tracker to write exceptions to App Insights
        private readonly TelemetryTracker _tracker = new TelemetryTracker();

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

        #region Handle References

        #region CreateRef

        /// <summary>
        ///   Create reference between address and customer
        /// </summary>
        /// <param name="key"></param>
        /// <param name="navigationProperty"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri reference) {
            var address = await _db.Addresses.SingleOrDefaultAsync(p => p.id == key);
            if (address == null) {
                return NotFound();
            }
            switch (navigationProperty) {
                case "customer":
                    var relatedKey = ReferenceHelper.GetKeyFromUri<int>(Request, reference);
                    var customer = _db.Customers.Where(f => f.id == relatedKey).ToList();
                    if (customer.Count == 0) {
                        return NotFound();
                    }

                    address.Customers = customer;
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
        ///   Remove reference between address and customer
        /// </summary>
        /// <param name="key"></param>
        /// <param name="navigationProperty"></param>
        /// <param name="relatedKey"></param>
        /// <returns></returns>
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
            string navigationProperty, [FromODataUri] string relatedKey) {
            var address = _db.Addresses.Include("Customers").SingleOrDefault(p => p.id == key);
            if (address == null) {
                return NotFound();
            }

            switch (navigationProperty) {
                case "customer":
                    var customer = address.Customers.SingleOrDefault(p => p.id == Convert.ToInt32(relatedKey));
                    if (customer == null) {
                        return NotFound();
                    }

                    address.Customers.Remove(customer);
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
