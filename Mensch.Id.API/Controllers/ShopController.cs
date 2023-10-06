using System;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Commons.Extensions;
using Mensch.Id.API.AccessControl.Policies;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models.AccessControl;
using Mensch.Id.API.Models.RequestParameters;
using Mensch.Id.API.Models.Shop;
using Mensch.Id.API.Storage;
using Mensch.Id.API.Workflow.FilterExpressionBuilders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mensch.Id.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ShopController : ControllerBase
    {
        private readonly IStore<Order> orderStore;
        private readonly IReadonlyStore<Product> productStore;
        private readonly IFilterExpressionBuilder<Product, ProductRequestParameters> productFilterExpressionBuilder;
        private readonly IFilterExpressionBuilder<Order, OrderRequestParameters> orderFilterExpressionBuilder;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IReadonlyStore<Account> accountStore;

        public ShopController(
            IStore<Order> orderStore,
            IReadonlyStore<Product> productStore,
            IFilterExpressionBuilder<Product,ProductRequestParameters> productFilterExpressionBuilder,
            IFilterExpressionBuilder<Order, OrderRequestParameters> orderFilterExpressionBuilder,
            IHttpContextAccessor httpContextAccessor,
            IReadonlyStore<Account> accountStore)
        {
            this.orderStore = orderStore;
            this.productStore = productStore;
            this.productFilterExpressionBuilder = productFilterExpressionBuilder;
            this.orderFilterExpressionBuilder = orderFilterExpressionBuilder;
            this.httpContextAccessor = httpContextAccessor;
            this.accountStore = accountStore;
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProducts(
            [FromQuery] ProductRequestParameters queryParameters)
        {
            var filterExpressions = productFilterExpressionBuilder.Build(queryParameters);
            var combinedFilter = filterExpressions.Count > 0 ? SearchExpressionBuilder.And(filterExpressions.ToArray()) : x => true;
            var orderByExpression = BuildOrderByExpressionForProduct(queryParameters.OrderBy);
            var items = productStore.SearchAsync(
                combinedFilter,
                queryParameters.Count,
                queryParameters.Skip,
                orderByExpression,
                queryParameters.OrderDirection);
            return Ok(items);
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders(
            [FromQuery] OrderRequestParameters queryParameters)
        {
            var filterExpressions = orderFilterExpressionBuilder.Build(queryParameters);
            var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
            filterExpressions.Add(x => x.OrderedByAccountId == accountId);
            var filter = SearchExpressionBuilder.And(filterExpressions.ToArray());
            var orderByExpression = BuildOrderByExpressionForOrder(queryParameters.OrderBy);
            var orders = orderStore.SearchAsync(
                filter,
                queryParameters.Count,
                queryParameters.Skip,
                orderByExpression,
                queryParameters.OrderDirection);
            return Ok(orders);
        }

        [HttpPost("submit-order")]
        public async Task<IActionResult> StoreOrder(
            [FromBody] Order order)
        {
            if (await orderStore.ExistsAsync(order.Id))
                return StatusCode((int)HttpStatusCode.Conflict, "An order with that ID already exists");
            var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
            var account = await accountStore.GetByIdAsync(accountId);
            if (account is not AssignerAccount assignerAccount)
                return StatusCode((int)HttpStatusCode.Forbidden, "Only assigners can order");
            order.OrderedByAccountId = accountId;
            order.SetStatus(OrderStatus.Placed);
            order.InvoiceAddress ??= assignerAccount.Contact;
            order.SendInvoiceSeparately = false;
            order.ShippingAddress ??= assignerAccount.Contact;
            await orderStore.StoreAsync(order);
            return Ok();
        }

        [Authorize(Policy = AccountTypeRequirement.AdminPolicyName)]
        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders(
            [FromQuery] OrderRequestParameters queryParameters)
        {
            var filterExpressions = orderFilterExpressionBuilder.Build(queryParameters);
            var filter = filterExpressions.Count > 0
                ? SearchExpressionBuilder.And(filterExpressions.ToArray())
                : x => true;
            var orderByExpressions = BuildOrderByExpressionForOrder(queryParameters.OrderBy);
            var orders = orderStore.SearchAsync(
                filter,
                queryParameters.Count,
                queryParameters.Skip,
                orderByExpressions,
                queryParameters.OrderDirection);
            return Ok(orders);
        }

        [HttpPost("orders/{id}/cancel")]
        public async Task<IActionResult> CancelOrder(
            [FromRoute] string id)
        {
            if (!ControllerInputSanitizer.ValidateAndSanitizeMandatoryId(id, out id))
                return BadRequest("Invalid order-ID");
            var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
            var order = await orderStore.GetByIdAsync(id);
            if (order == null || order.OrderedByAccountId != accountId)
                return NotFound();
            order.SetStatus(OrderStatus.Cancelled);
            await orderStore.StoreAsync(order);
            return Ok(order);
        }

        [HttpPost("orders/{id}/received")]
        public async Task<IActionResult> MarkOrderAsReceived(
            [FromRoute] string id)
        {
            if (!ControllerInputSanitizer.ValidateAndSanitizeMandatoryId(id, out id))
                return BadRequest("Invalid order-ID");
            var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
            var order = await orderStore.GetByIdAsync(id);
            if (order == null || order.OrderedByAccountId != accountId)
                return NotFound();
            order.SetStatus(OrderStatus.Received);
            await orderStore.StoreAsync(order);
            return Ok(order);
        }

        [Authorize(Policy = AccountTypeRequirement.AdminPolicyName)]
        [HttpPost("orders/{id}/accept")]
        public async Task<IActionResult> AcceptOrder(
            [FromRoute] string id)
        {
            if (!ControllerInputSanitizer.ValidateAndSanitizeMandatoryId(id, out id))
                return BadRequest("Invalid order-ID");
            var order = await orderStore.GetByIdAsync(id);
            if (order == null)
                return NotFound();
            order.SetStatus(OrderStatus.Accepted);
            await orderStore.StoreAsync(order);
            return Ok(order);
        }

        [Authorize(Policy = AccountTypeRequirement.AdminPolicyName)]
        [HttpPost("orders/{id}/fulfill")]
        public async Task<IActionResult> FulfillOrder(
            [FromRoute] string id)
        {
            if (!ControllerInputSanitizer.ValidateAndSanitizeMandatoryId(id, out id))
                return BadRequest("Invalid order-ID");
            var order = await orderStore.GetByIdAsync(id);
            if (order == null)
                return NotFound();
            order.SetStatus(OrderStatus.Shipped);
            await orderStore.StoreAsync(order);
            return Ok(order);
        }

        [Authorize(Policy = AccountTypeRequirement.AdminPolicyName)]
        [HttpDelete("orders/{id}")]
        public async Task<IActionResult> DeleteOrder(
            [FromRoute] string id)
        {
            if (!ControllerInputSanitizer.ValidateAndSanitizeMandatoryId(id, out id))
                return BadRequest("Invalid order-ID");
            await orderStore.DeleteAsync(id);
            return Ok();
        }

        private Expression<Func<Product, object>> BuildOrderByExpressionForProduct(
            string orderBy)
        {
            return orderBy?.ToLower() switch
            {
                "category" => x => x.Category,
                "name" => x => x.Name,
                "price" => x => x.Price.Value,
                _ => x => x.Category
            };
        }

        private Expression<Func<Order, object>> BuildOrderByExpressionForOrder(
            string orderBy)
        {
            return orderBy?.ToLower() switch
            {
                "status" => x => x.Status,
                "time" => x => x.StatusChanges[0].Timestamp,
                _ => x => x.StatusChanges[0].Timestamp
            };
        }
    }
}
