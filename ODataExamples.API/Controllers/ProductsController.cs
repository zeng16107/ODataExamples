#region Using

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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
    /// Handles operations related to Product data
    /// </summary>
    public class ProductsController : ODataController
    {
        // EF Model for database interation(s)
        private readonly ODataEntities _db = new ODataEntities();

        // Telemetry tracker to write exceptions to App Insights
        private readonly TelemetryTracker _tracker = new TelemetryTracker();

        #region Products
        
        #region GETs

        // GET: produts
        /// <summary>Query products</summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        [EnableQuery(MaxExpansionDepth = 5)]
        [HttpGet]
        [ODataRoute("products")]
        [ResponseType(typeof(IEnumerable<Product>))]
        public async Task<IHttpActionResult> Get() {
            try {
                // Look for all products - can be dangerous if no query properties are provided
                var products = _db.Products;
                if (!await products.AnyAsync()){
                    // Data not available, return 404
                    return NotFound();
                }
                // Return products
                return Ok(products);
            }
            catch (Exception ex){
                return InternalServerError(ex);
            }
        }

        // GET: products(5)
        /// <summary>Query product by id</summary>
        /// <param name="id">Product id</param>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        [EnableQuery(MaxExpansionDepth = 5)]
        [HttpGet]
        [ODataRoute("products({id})")]
        [ResponseType(typeof(Product))]
        public async Task<IHttpActionResult> Get([FromODataUri] int id){
            try{
                // Attempt to find product
                var product = await _db.Products.FindAsync(id);
                if (product == null){
                    // Prodcut not found, return 404
                    return NotFound();
                }
                // Return product to caller
                return Ok(product);
            }
            catch (Exception ex){
                return InternalServerError(ex);
            }
        }

        #endregion

        #region POST

        /// <summary>Create a new product</summary>
        /// <param name="product"></param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="409">Conflict</response>
        [EnableQuery]
        [HttpPost]
        [ODataRoute("products")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Post([FromBody] Product product) {
            try{
                // Ensure incoming request adheres to entity expectations
                if (!ModelState.IsValid) {
                    // Something isn't right, return 400
                    return BadRequest(ModelState);
                }

                // Check to see if incoming product already exists based on upc
                var dbProduct = _db.Products.Where(p => p.upc == product.upc);
                if (dbProduct.Any()) {
                    // Existing product exists, return 409
                    return Conflict();
                }

                // Write tattler values (just for illustration - tie in your authorized claim token for user)
                product.inserted_datetime = DateTime.UtcNow;
                product.inserted_by = "OData Examples API";
                product.last_updated_datetime = DateTime.UtcNow;
                product.last_updated_by = "OData Examples API";

                // Add product to collection
                _db.Products.Add(product);

                // Commit creation to database
                await _db.SaveChangesAsync();

                // Return newly added product to caller
                return Created(product);
            }
            catch (Exception ex) {
                //Send exception detail to insights
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
        }

        #endregion

        #region PATCH

        /// <summary>Patch existing product</summary>
        /// <param name="id">Product Id</param>
        /// <param name="productDelta">Product delta</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="409">Conflict</response>
        [AcceptVerbs("PATCH", "MERGE")]
        [EnableQuery]
        [HttpPatch]
        [ODataRoute("products({id})")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Patch([FromODataUri] int id, [FromBody] Delta<Product> productDelta) {

            // Ensure incoming request adheres to entity expectations
            if (!ModelState.IsValid) {
                // Something isn't right, return 400
                return BadRequest(ModelState);
            }

            // Attempt to find existing product
            var dbProduct = await _db.Products.FindAsync(id);
            if (dbProduct == null) {
                // Requested product doesn't exist, return 404
                return NotFound();
            }

            try {
                // Update tattler values
                dbProduct.last_updated_datetime = DateTime.UtcNow;
                dbProduct.last_updated_by = "OData Examples API";

                // Issue Patch to existing record
                productDelta.Patch(dbProduct);

                // Commit change to database
                await _db.SaveChangesAsync();
            }
            catch (Exception ex) {
                //Send exception detail to insights
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
            // Return updated product record to caller
            return Updated(dbProduct);
        }

        #endregion

        #region PUT
        // PUT: products(5)
        /// <summary>
        /// Overwrite existing product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="product"></param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="409">Conflict</response>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [EnableQuery]
        [HttpPut]
        [ODataRoute("products({id})")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Put([FromODataUri] int id, [FromBody] Product product) {

            // Attempt to find existing product
            var dbProduct = _db.Products.Find(id);
            if (dbProduct == null) {
                // Requested product not found, return 404
                return NotFound();
            }

            try {
                // Update tattler values (just for illustration - tie in your authorized claim token for user)
                product.last_updated_datetime = DateTime.UtcNow;
                product.last_updated_by = "OData Examples API";

                // Replace product data
                dbProduct = product;

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
            return Updated(dbProduct);
        }
        #endregion

        #region DELETE

        // DELETE: products(5)
        /// <summary>Delete product</summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">The product id</param>
        /// <response code="204">No Content</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found</response>
        [EnableQuery]
        [HttpDelete]
        [ODataRoute("products({id})")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Delete([FromODataUri] int id) {
            try {
                // Attempt to find existing product
                var dbProduct = await _db.Products.FindAsync(id);
                if (dbProduct == null) {
                    // Requested product not found, return 404
                    return NotFound();
                }

                // Remove product record
                _db.Products.Remove(dbProduct);

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

        protected override void Dispose(bool disposing) {
            if (disposing) {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
