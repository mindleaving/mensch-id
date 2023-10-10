using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Commons.Extensions;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models.RequestParameters;
using Mensch.Id.API.Models.Shop;

namespace Mensch.Id.API.Workflow.FilterExpressionBuilders
{
    public class ProductFilterExpressionBuilder : IFilterExpressionBuilder<Product, ProductRequestParameters>
    {
        public List<Expression<Func<Product, bool>>> Build(
            ProductRequestParameters queryParameters)
        {
            var filterExpressions = new List<Expression<Func<Product, bool>>>();

            if (!string.IsNullOrWhiteSpace(queryParameters.SearchText))
            {
                var searchTerms = SearchTermSplitter.SplitAndToLower(queryParameters.SearchText);
                var searchTextFilter = SearchExpressionBuilder.ContainsAll<Product>(x => x.Name.ToLower(), searchTerms);
                filterExpressions.Add(searchTextFilter);
            }

            if (!string.IsNullOrWhiteSpace(queryParameters.Category))
            {
                var normalizedCategory = queryParameters.Category.ToLower();
                filterExpressions.Add(x => x.Category.ToLower() == normalizedCategory);
            }

            return filterExpressions;
        }
    }
}
