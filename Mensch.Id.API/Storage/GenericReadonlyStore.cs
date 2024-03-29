﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;
using MongoDB.Driver;

namespace Mensch.Id.API.Storage
{
    public class GenericReadonlyStore<T> : IReadonlyStore<T> where T : IId
    {
        protected readonly IMongoDatabase database;
        protected readonly IMongoCollection<T> collection;

        public GenericReadonlyStore(
            IMongoDatabase mongoDatabase, 
            string collectionName = null)
        {
            database = mongoDatabase;
            collection = mongoDatabase.GetCollection<T>(collectionName ?? typeof(T).Name);
        }

        public IAsyncEnumerable<T> GetAllAsync()
        {
            return collection.Find(FilterDefinition<T>.Empty).ToAsyncEnumerable();
        }

        public IAsyncEnumerable<T> SearchAsync(
            Expression<Func<T, bool>> filter,
            int? count = null,
            int? skip = null,
            Expression<Func<T, object>> orderBy = null,
            OrderDirection orderDirection = OrderDirection.Ascending)
        {
            var findExpression = collection.Find(filter ?? (x => true));
            if (orderBy != null)
            {
                findExpression = orderDirection == OrderDirection.Ascending 
                    ? findExpression.SortBy(orderBy) 
                    : findExpression.SortByDescending(orderBy);
            }
            return findExpression.Skip(skip).Limit(count).ToAsyncEnumerable();
        }

        public Task<bool> ExistsAsync(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            return collection.Find(x => x.Id == id).AnyAsync();
        }

        public Task<T> GetByIdAsync(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            return collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public Task<T> FirstOrDefaultAsync(
            Expression<Func<T, bool>> filter)
        {
            return collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}