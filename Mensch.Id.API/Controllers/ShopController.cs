using System;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
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
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IReadonlyStore<Account> accountStore;

        public ShopController(
            IStore<Order> orderStore,
            IReadonlyStore<Product> productStore,
            IFilterExpressionBuilder<Product,ProductRequestParameters> productFilterExpressionBuilder,
            IHttpContextAccessor httpContextAccessor,
            IReadonlyStore<Account> accountStore)
        {
            this.orderStore = orderStore;
            this.productStore = productStore;
            this.productFilterExpressionBuilder = productFilterExpressionBuilder;
            this.httpContextAccessor = httpContextAccessor;
            this.accountStore = accountStore;
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProducts(
            [FromQuery] ProductRequestParameters queryParameters)
        {
            var filterExpressions = productFilterExpressionBuilder.Build(queryParameters);
            var combinedFilter = filterExpressions.Count > 0 ? SearchExpressionBuilder.And(filterExpressions.ToArray()) : x => true;
            var orderByExpression = BuildOrderByExpression(queryParameters.OrderBy);
            var items = productStore.SearchAsync(
                combinedFilter,
                queryParameters.Count,
                queryParameters.Skip,
                orderByExpression,
                queryParameters.OrderDirection);
            return Ok(items);
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var accountId = ControllerHelpers.GetAccountId(httpContextAccessor);
            var orders = orderStore.SearchAsync(x => x.OrderedByAccountId == accountId);
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
            order.CreationTimestamp = DateTime.UtcNow;
            order.Status = OrderStatus.Placed;
            order.InvoiceAddress ??= assignerAccount.Contact;
            order.SendInvoiceSeparately = false;
            order.ShippingAddress ??= assignerAccount.Contact;
            await orderStore.StoreAsync(order);
            return Ok();
        }

        private Expression<Func<Product, object>> BuildOrderByExpression(
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
    }
}
