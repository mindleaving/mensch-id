using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Commons.Extensions;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using Mensch.Id.API.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Mensch.Id.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public abstract class RestControllerBase<T> : ControllerBase, IRestController<T> where T: class, IId
    {
        protected readonly IStore<T> store;
        protected readonly IHttpContextAccessor httpContextAccessor;

        protected RestControllerBase(
            IStore<T> store,
            IHttpContextAccessor httpContextAccessor)
        {
            this.store = store;
            this.httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById(
            [FromRoute] string id,
            [FromQuery] Language language = Language.en)
        {
            var item = await store.GetByIdAsync(id);
            if (item == null)
                return NotFound();
            var transformedItem = await TransformItem(item, language);
            return Ok(transformedItem);
        }

        [HttpGet]
        [HttpGet("search")]
        public virtual async Task<IActionResult> GetMany(
            [FromQuery] string searchText, 
            [FromQuery] int? count = null,
            [FromQuery] int? skip = null,
            [FromQuery] string orderBy = null,
            [FromQuery] OrderDirection orderDirection = OrderDirection.Ascending,
            [FromQuery] Language language = Language.en)
        {
            Expression<Func<T, bool>> searchExpression;
            if(!string.IsNullOrWhiteSpace(searchText))
            {
                var searchTerms = SearchTermSplitter.SplitAndToLower(searchText);
                searchExpression = BuildSearchExpression(searchTerms);
            }
            else
            {
                searchExpression = x => true;
            }
            var orderByExpression = BuildOrderByExpression(orderBy);
            var items = store.SearchAsync(searchExpression, count, skip, orderByExpression, orderDirection);
            var transformedItems = TransformItems(items, language);
            return Ok(transformedItems);
        }

        [HttpPut("{id}")]
        public virtual async Task<IActionResult> CreateOrReplace([FromRoute] string id, [FromBody] T item)
        {
            if (id != item.Id)
                return BadRequest("ID of route doesn't match body");
            var username = ControllerHelpers.GetUsername(httpContextAccessor);
            var storageOperation = await store.StoreAsync(item);
            await PublishChange(item, storageOperation, username);
            return Ok(id);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdate([FromRoute] string id, [FromBody] JsonPatchDocument<T> updates)
        {
            var item = await store.GetByIdAsync(id);
            if (item == null)
                return NotFound();
            updates.ApplyTo(item, ModelState);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var username = ControllerHelpers.GetUsername(httpContextAccessor);
            await store.StoreAsync(item);
            await PublishChange(item, StorageOperation.Changed, username);
            return Ok();
        }


        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete([FromRoute] string id)
        {
            await store.DeleteAsync(id);
            return Ok();
        }

        protected IAsyncEnumerable<object> TransformItems(IAsyncEnumerable<T> items, Language language)
        {
            return items.SelectParallelAsync((item, _) => TransformItem(item, language)).Cast<object>();
        }

        protected abstract Task<object> TransformItem(T item, Language language = Language.en);
        protected abstract Expression<Func<T, object>> BuildOrderByExpression(string orderBy);
        protected abstract Expression<Func<T,bool>> BuildSearchExpression(string[] searchTerms);
        protected abstract Task PublishChange(T item, StorageOperation storageOperation, string submitterUsername);
    }
}
