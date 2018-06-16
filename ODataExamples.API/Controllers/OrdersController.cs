﻿using System;
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
    /// Handles operations related to Order data
    /// </summary>
    public class OrdersController : ODataController
    {
        // EF Model for database interation(s)
        private readonly ODataSamplesEntities _db = new ODataSamplesEntities();

        // Telemetry tracker to write exceptions to App Insights
        private readonly TelemetryTracker _tracker = new TelemetryTracker();

        #region GETs

        // GET: orders
        /// <summary>Query orders</summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        [EnableQuery(MaxExpansionDepth = 5)]
        [HttpGet]
        [ODataRoute("orders")]
        [ResponseType(typeof(IEnumerable<Order>))]
        public async Task<IHttpActionResult> Get() {
            try {
                var orders = _db.Orders;
                if (!await orders.AnyAsync()){
                    return NotFound();
                }
                return Ok(orders);
            }
            catch (Exception ex){
                return InternalServerError(ex);
            }
        }

        // GET: orders(5)
        /// <summary>Query order by id</summary>
        /// <param name="id">Orders id</param>
        /// <response code="200">Ok</response>
        /// <response code="404">Not Found</response>
        [EnableQuery(MaxExpansionDepth = 5)]
        [HttpGet]
        [ODataRoute("orders({id})")]
        [ResponseType(typeof(Order))]
        public async Task<IHttpActionResult> Get([FromODataUri] int id){
            try{
                var orders = _db.Orders.Where(o => o.id == id);
                if (!await orders.AnyAsync()){
                    return NotFound();
                }
                return Ok(SingleResult.Create(orders));
            }
            catch (Exception ex){
                return InternalServerError(ex);
            }
        }

        #endregion

        #region POST

        /// <summary>Create a new order</summary>
        /// <param name="order"></param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="409">Conflict</response>
        [EnableQuery]
        [HttpPost]
        [ODataRoute("orders")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Post([FromBody] Order order) {
            try{
                if (!ModelState.IsValid) {
                    return BadRequest(ModelState);
                }

                var dbOrder = _db.Orders.Where(o => o.order_number == order.order_number);
                if (dbOrder.Any()) {
                    return Conflict();
                }

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();
                return Created(order);
            }
            catch (Exception ex) {
                //Send exception detail to insights
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
        }

        #endregion

        #region PATCH

        /// <summary>Patch existing order</summary>
        /// <param name="id">Order Id</param>
        /// <param name="orderDelta">Order delta</param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="409">Conflict</response>
        [AcceptVerbs("PATCH", "MERGE")]
        [EnableQuery]
        [HttpPatch]
        [ODataRoute("orders")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Patch([FromODataUri] int id, [FromBody] Delta<Order> orderDelta) {

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var dbOrder = await _db.Orders.FindAsync(id);
            if (dbOrder == null) {
                return Conflict();
            }

            try {
                orderDelta.Patch(dbOrder);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex) {
                //Send exception detail to insights
                _tracker.TrackException(ex);
                return InternalServerError(ex);
            }
            return Updated(dbOrder);
        }

        #endregion

        #region PUT
        // PUT: orders(5)
        /// <summary>
        /// Overwrite existing order
        /// </summary>
        /// <param name="id"></param>
        /// <param name="order"></param>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="409">Conflict</response>
        /// <returns></returns>
        [AcceptVerbs("PUT")]
        [EnableQuery]
        [HttpPut]
        [ODataRoute("orders({id})")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Put([FromODataUri] int id, [FromBody] Order order) {

            var dbOrder = _db.Orders.Find(id);
            if (dbOrder == null) {
                return NotFound();
            }

            try {
                dbOrder = order;
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
            return Updated(dbOrder);
        }
        #endregion

        #region DELETE

        // DELETE: orders(5)
        /// <summary>Delete order</summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">The order id</param>
        /// <response code="204">No Content</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found</response>
        [EnableQuery]
        [HttpDelete]
        [ODataRoute("orders({id})")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Delete([FromODataUri] int id) {
            try {
                var dbOrder = await _db.Orders.FindAsync(id);
                if (dbOrder == null) {
                    return NotFound();
                }
                _db.Orders.Remove(dbOrder);
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
