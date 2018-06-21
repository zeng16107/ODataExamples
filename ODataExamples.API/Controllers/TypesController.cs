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
    /// Handles operations related to Type/Lookup data
    /// </summary>
    public class TypesController : ODataController{
        // EF Model for database interation(s)
        private readonly ODataEntities _db = new ODataEntities();

        // Telemetry tracker to write exceptions to App Insights
        private readonly TelemetryTracker _tracker = new TelemetryTracker();

        #region Product Brands

        #region GET Product Brands

        // GET: Product Brand Data
        /// <summary>Query Product Brands</summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        [EnableQuery(MaxExpansionDepth = 5)]
        [HttpGet]
        [ODataRoute("productbrands")]
        [ResponseType(typeof(IEnumerable<ProductBrand>))]
        public async Task<IHttpActionResult> GetProductBrands() {
            try {
                // Look for all product brands
                var productBrands = _db.ProductBrands;
                if (!await productBrands.AnyAsync()) {
                    // Data not available, return 404
                    return NotFound();
                }
                // Return product brands
                return Ok(productBrands);
            }
            catch (Exception ex) {
                return InternalServerError(ex);
            }
        }

        // GET: productbrands(5)
        /// <summary>Query Product Brand by id</summary>
        /// <param name="id">Product Brand id</param>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        [EnableQuery(MaxExpansionDepth = 5)]
        [HttpGet]
        [ODataRoute("productbrands({id})")]
        [ResponseType(typeof(Customer))]
        public async Task<IHttpActionResult> GetCustomer([FromODataUri] int id) {
            try {
                // Attempt to find product brand
                var productBrand = await _db.ProductBrands.FindAsync(id);
                if (productBrand == null) {
                    // Product Brand not found, return 404
                    return NotFound();
                }
                // Return product brand to caller
                return Ok(productBrand);
            }
            catch (Exception ex) {
                return InternalServerError(ex);
            }
        }

        #endregion

        #region POST Product Brand

        /// <summary>Create a new product brand</summary>
        /// <param name="productBrand"></param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="409">Conflict</response>
        [EnableQuery]
        [HttpPost]
        [ODataRoute("productbrands")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PostProductBrand([FromBody] ProductBrand productBrand) {
            try {
                // Ensure incoming request adheres to entity expectations
                if (!ModelState.IsValid) {
                    // Something isn't right, return 400
                    return BadRequest(ModelState);
                }

                // Check to see if incoming product brand already exists based on brand name
                var dbProductBrand = _db.ProductBrands.Where(o => o.product_brand == productBrand.product_brand);
                if (dbProductBrand.Any()) {
                    // Existing product brand exists, return 409
                    return Conflict();
                }

                // Write tattler values (just for illustration - tie in your authorized claim token for user)
                productBrand.inserted_datetime = DateTime.UtcNow;
                productBrand.inserted_by = "OData Examples API";
                productBrand.last_updated_datetime = DateTime.UtcNow;
                productBrand.last_updated_by = "OData Examples API";

                // Add produt brand to collection
                _db.ProductBrands.Add(productBrand);

                // Commit creation to database
                await _db.SaveChangesAsync();

                // Return newly added customer to caller
                return Created(productBrand);
            }
            catch (Exception ex) {
                //Send exception detail to insights
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
        }

        #endregion

        #region PATCH Product Brand

        /// <summary>Update product brand</summary>
        /// <param name="id">Product Brand Id</param>
        /// <param name="productBrandDelta">Product brand delta</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="409">Conflict</response>
        [AcceptVerbs("PATCH", "MERGE")]
        [EnableQuery]
        [HttpPatch]
        [ODataRoute("productbrands({id})")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PatchProductBrand([FromODataUri] int id, [FromBody] Delta<ProductBrand> productBrandDelta) {

            // Ensure incoming request adheres to entity expectations
            if (!ModelState.IsValid) {
                // Something isn't right, return 400
                return BadRequest(ModelState);
            }

            // Attempt to find existing product brand
            var dbProductBrand = await _db.ProductBrands.FindAsync(id);
            if (dbProductBrand == null) {
                // Requested product brand doesn't exist, return 404
                return NotFound();
            }

            try {
                // Update date tattler value
                dbProductBrand.last_updated_datetime = DateTime.UtcNow;

                // Issue Patch to existing record
                productBrandDelta.Patch(dbProductBrand);

                // Commit change to database
                await _db.SaveChangesAsync();
            }
            catch (Exception ex) {
                //Send exception detail to insights
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
            // Return updated product brand record to caller
            return Updated(dbProductBrand);
        }

        #endregion

        #region PUT Product Brand

        // PUT: productbrands(5)
        /// <summary>
        /// Overwrite product brand
        /// </summary>
        /// <param name="id"></param>
        /// <param name="productBrand"></param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="409">Conflict</response>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [EnableQuery]
        [HttpPut]
        [ODataRoute("productbrands({id})")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutCustomer([FromODataUri] int id, [FromBody] ProductBrand productBrand) {

            // Attempt to find existing customer
            var dbProductBrand = await _db.ProductBrands.FindAsync(id);
            if (dbProductBrand == null) {
                // Requested product brand not found, return 404
                return NotFound();
            }

            try {
                // Update tattler values (just for illustration - tie in your authorized claim token for user)
                productBrand.last_updated_datetime = DateTime.UtcNow;
                productBrand.last_updated_by = "OData Examples API";

                // Replace product brand data
                dbProductBrand = productBrand;

                // Commit change to database
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex) {
                if (!_db.ProductBrands.Any(c => c.id == id)) {
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
            return Updated(dbProductBrand);
        }
        #endregion

        #region DELETE Product Brand

        // DELETE: productbrands(5)
        /// <summary>Delete product brand</summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">The product brand id</param>
        /// <response code="204">No Content</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found</response>
        [EnableQuery]
        [HttpDelete]
        [ODataRoute("productbrands({id})")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> DeleteProductBrand([FromODataUri] int id) {
            try {
                // Attempt to find existing product brand
                var dbProductBrand = await _db.ProductBrands.FindAsync(id);
                if (dbProductBrand == null) {
                    // Requested product brand not found, return 404
                    return NotFound();
                }

                // Remove customer record
                _db.ProductBrands.Remove(dbProductBrand);

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
