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
    public class ContactsController : ODataController
    {
        // EF Model for database interation(s)
        private readonly ODataEntities _db = new ODataEntities();

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
                // Requested customer doesn't exist, return 404
                return NotFound();
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
                // Look for all addresses - can be dangerous if no query properties are provided
                var addresses = _db.Addresses;
                if (!await addresses.AnyAsync()){
                    // Data not available, return 404
                    return NotFound();
                }
                
                // Return addresses
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
        [ResponseType(typeof(Address))]
        public async Task<IHttpActionResult> GetCustomerAddress([FromODataUri] int id) {
            try {
                // Attempt to find customer
                var customerAddress = await _db.Addresses.FindAsync(id);
                if (customerAddress == null){
                    // Address not found, return 404
                    return NotFound();
                }
                // Return customer to caller
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
                // Ensure incoming request adheres to entity expectations
                if (!ModelState.IsValid) {
                    // Something isn't right, return 400
                    return BadRequest(ModelState);
                }

                // Check to see if incoming address already exists based on street address
                // May need to accept multiple "dup" addresses depending on requirements
                var dbAddress = _db.Addresses
                    .Where(c => c.street_address_1 == address.street_address_1 &&
                                c.street_address_2 == address.street_address_2);
                if (dbAddress.Any()) {
                    // Existing address exists, return 409
                    return Conflict();
                }

                // Write tattler values (just for illustration - tie in your authorized claim token for user)
                address.inserted_datetime = DateTime.UtcNow;
                address.inserted_by = "OData Examples API";
                address.last_updated_datetime = DateTime.UtcNow;
                address.last_updated_by = "OData Examples API";

                // Add address to collection
                _db.Addresses.Add(address);

                // Commit creation to database
                await _db.SaveChangesAsync();

                // Return newly added customer to caller
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

            // Ensure incoming request adheres to entity expectations
            if (!ModelState.IsValid) {
                // Something isn't right, return 400
                return BadRequest(ModelState);
            }

            // Attempt to find existing address
            var dbAddress = await _db.Addresses.FindAsync(id);
            if (dbAddress == null) {
                // Requested address doesn't exist, return 404
                return NotFound();
            }

            try {
                // Update date tattler value
                dbAddress.last_updated_datetime = DateTime.UtcNow;

                // Issue Patch to existing record
                addressDelta.Patch(dbAddress);

                // Commit change to database
                await _db.SaveChangesAsync();
            }
            catch (Exception ex) {
                //Send exception detail to insights
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
            // Return updated address record to caller
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
        public async Task<IHttpActionResult> Put([FromODataUri] int id, [FromBody] Address address) {

            // Attempt to find existing addresss
            var dbAddress = await _db.Addresses.FindAsync(id);
            if (dbAddress == null) {
                // Requested address not found, return 404
                return NotFound();
            }

            try {
                // Update tattler values (just for illustration - tie in your authorized claim token for user)
                address.last_updated_datetime = DateTime.UtcNow;
                address.last_updated_by = "OData Examples API";

                // Replace customer data
                dbAddress = address;

                // Commit change to database
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
            // Return updated customer to caller
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
                // Attempt to find existing address
                var dbCustomer = await _db.Addresses.FindAsync(id);
                if (dbCustomer == null) {
                    // Requested address not found, return 404
                    return NotFound();
                }

                // Remove address record
                _db.Addresses.Remove(dbCustomer);

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

        #region Customer Phone Data

        #region GET Customer Phone

        // GET: phones
        /// <summary>Query customer phone data</summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        [EnableQuery(MaxExpansionDepth = 5)]
        [HttpGet]
        [ODataRoute("phones")]
        [ResponseType(typeof(IEnumerable<Phone>))]
        public async Task<IHttpActionResult> GetCustomerPhones() {
            try {
                // Look for all phone records - can be dangerous if no query properties are provided
                var phones = _db.Phones;
                if (!await phones.AnyAsync()){
                    // Data not available, return 404
                    return NotFound();
                }
                
                // Return phone data
                return Ok(phones);
            }
            catch (Exception ex){
                return InternalServerError(ex);
            }
        }

        // GET: phones(1)
        /// <summary>Query customer phone data</summary>
        /// <param name="id">Phone id</param>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        [EnableQuery(MaxExpansionDepth = 5)]
        [HttpGet]
        [ODataRoute("phones({id})")]
        [ResponseType(typeof(Phone))]
        public async Task<IHttpActionResult> GetCustomerPhone([FromODataUri] int id) {
            try {
                // Attempt to find phone data
                var phone = await _db.Phones.FindAsync(id);
                if (phone == null){
                    // Phone record not found, return 404
                    return NotFound();
                }
                // Return phone data to caller
                return Ok(phone);
            }
            catch (Exception ex){
                return InternalServerError(ex);
            }
        }

        #endregion

        #region POST Customer Phone Data

        /// POST: phones
        /// <summary>Create a new customer phone record</summary>
        /// <param name="phone">Customer Phone Data</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="409">Conflict</response>
        [EnableQuery]
        [HttpPost]
        [ODataRoute("phones")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PostCustomerPhone([FromBody] Phone phone) {
            try{
                // Ensure incoming request adheres to entity expectations
                if (!ModelState.IsValid) {
                    // Something isn't right, return 400
                    return BadRequest(ModelState);
                }

                // Check to see if incoming phone record already exists
                var dbPhone = _db.Phones.Where(c => c.phone_number == phone.phone_number);
                if (dbPhone.Any()) {
                    // Existing phone record exists, return 409
                    return Conflict();
                }

                // Write tattler values (just for illustration - tie in your authorized claim token for user)
                phone.inserted_datetime = DateTime.UtcNow;
                phone.inserted_by = "OData Examples API";
                phone.last_updated_datetime = DateTime.UtcNow;
                phone.last_updated_by = "OData Examples API";

                // Add phone data to collection
                _db.Phones.Add(phone);

                // Commit creation to database
                await _db.SaveChangesAsync();

                // Return newly added phone data to caller
                return Created(dbPhone);
            }
            catch (Exception ex) {
                //Send exception detail to insights
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
        }

        #endregion

        #region PATCH Customer Phone Data

        /// PATCH: phones({id})
        /// <summary>Update customer's phone data</summary>
        /// <param name="id">Phone Number Id</param>
        /// <param name="phoneDelta">Phone data delta</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="409">Conflict</response>
        [AcceptVerbs("PATCH", "MERGE")]
        [EnableQuery]
        [HttpPatch]
        [ODataRoute("phones({id})")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PatchCustomerPhone([FromODataUri] int id, [FromBody] Delta<Phone> phoneDelta) {

            // Ensure incoming request adheres to entity expectations
            if (!ModelState.IsValid) {
                // Something isn't right, return 400
                return BadRequest(ModelState);
            }

            // Attempt to find existing phone data
            var dbPhone = await _db.Phones.FindAsync(id);
            if (dbPhone == null) {
                // Requested phone data not found, return 404
                return NotFound();
            }

            try {
                // Update date tattler value
                dbPhone.last_updated_datetime = DateTime.UtcNow;

                // Issue Patch to existing record
                phoneDelta.Patch(dbPhone);

                // Commit change to database
                await _db.SaveChangesAsync();
            }
            catch (Exception ex) {
                //Send exception detail to insights
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
            // Return updated phone record to caller
            return Updated(dbPhone);
        }

        #endregion

        #region PUT Customer Phone Data

        /// PUT: phones({id})
        /// <summary>
        /// Overwrite customer phone data
        /// </summary>
        /// <param name="id">Phone Id</param>
        /// <param name="phone">Phone</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="409">Conflict</response>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [EnableQuery]
        [HttpPut]
        [ODataRoute("phones({id})")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutCustomerPhone([FromODataUri] int id, [FromBody] Phone phone) {

            // Attempt to find existing phone record
            var dbPhone = await _db.Phones.FindAsync(id);
            if (dbPhone == null) {
                // Requested phone record not found, return 404
                return NotFound();
            }

            try {
                // Update tattler values (just for illustration - tie in your authorized claim token for user)
                phone.last_updated_datetime = DateTime.UtcNow;
                phone.last_updated_by = "OData Examples API";

                // Replace phone data
                dbPhone = phone;

                // Commit change to database
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
            // Return updated phone data to caller
            return Updated(dbPhone);
        }
        #endregion

        #region DELETE Customer Phone Data

        // DELETE: phones(5)
        /// <summary>Delete phone record</summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">The phone data id</param>
        /// <response code="204">No Content</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found</response>
        [EnableQuery]
        [HttpDelete]
        [ODataRoute("phones({id})")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> DeleteCustomerPhone([FromODataUri] int id) {
            try {
                // Attempt to find existing phone record
                var dbPhone = await _db.Phones.FindAsync(id);
                if (dbPhone == null) {
                    // Requested phone record not found, return 404
                    return NotFound();
                }

                // Remove phone record
                _db.Phones.Remove(dbPhone);

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
        ///   Create reference between address or phone and customer
        /// </summary>
        /// <param name="key"></param>
        /// <param name="navigationProperty"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri reference) {
            
            // Attempt to find existing customer record
            var customer = await _db.Customers.SingleOrDefaultAsync(p => p.id == key);
            if (customer == null) {
                // Requested customer record not found, return 404
                return NotFound();
            }

            // Create reference based on requested navigation property
            switch (navigationProperty) {
                case "addresses":
                    // Extract address id from incoing request uri
                    var relatedAddressKey = ReferenceHelper.GetKeyFromUri<int>(Request, reference);

                    // Attempt to locate address record
                    var address = _db.Addresses.Where(f => f.id == relatedAddressKey).ToList();
                    if (address.Count == 0) {
                        // Address not found, return 404
                        return NotFound();
                    }

                    // Assign address to customer
                    customer.Addresses = address;
                    break;

                case "phones":
                    // Extract phone id from incoing request uri
                    var relatedPhoneKey = ReferenceHelper.GetKeyFromUri<int>(Request, reference);

                    // Attempt to locate phone record
                    var phone = _db.Phones.Where(f => f.id == relatedPhoneKey).ToList();
                    if (phone.Count == 0) {
                        // Phone record not found, return 404
                        return NotFound();
                    }

                    // Assign phone record to customer
                    customer.Phones = phone;
                    break;

                // In case a requested navigation property is not supported
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }

            // Commit changes to database
            await _db.SaveChangesAsync();

            // Return successful update to caller
            return StatusCode(HttpStatusCode.NoContent);
        }

        #endregion

        #region DeleteRef

        /// <summary>
        ///   Remove reference between address or phone and customer
        /// </summary>
        /// <param name="key"></param>
        /// <param name="navigationProperty"></param>
        /// <param name="relatedKey"></param>
        /// <returns></returns>
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
            string navigationProperty, [FromODataUri] string relatedKey) {

            // Attempt to find existing customer record
            var customer = _db.Customers
                .Include("Addresses")
                .Include("Phones")
                .SingleOrDefault(p => p.id == key);

            // Requested customer record not found, return 404
            if (customer == null) {
                return NotFound();
            }

            switch (navigationProperty) {
                case "addresses":

                    // Attempt to find existing address record
                    var address = customer.Addresses.SingleOrDefault(p => p.id == Convert.ToInt32(relatedKey));
                    if (address == null) {
                        // Address record not found, return 404
                        return NotFound();
                    }

                    // Remove address reference - your requirements may ask to delete address record
                    customer.Addresses.Remove(address);
                    _db.Addresses.Remove(address);
                    break;

                case "phones":

                    // Attempt to find existing phone record
                    var phone = customer.Phones.SingleOrDefault(p => p.id == Convert.ToInt32(relatedKey));
                    if (phone == null) {
                        // Phone record not found, return 404
                        return NotFound();
                    }

                    // Remove phone reference - your requirements may ask to delete address record
                    customer.Phones.Remove(phone);
                    _db.Phones.Remove(phone);
                    break;

                // In case a requested navigation property is not supported
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }

            // Commit changes to database
            await _db.SaveChangesAsync();

            // Return successful delete to caller
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
