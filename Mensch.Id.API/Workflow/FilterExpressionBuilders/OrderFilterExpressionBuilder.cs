using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Mensch.Id.API.Models.RequestParameters;
using Mensch.Id.API.Models.Shop;

namespace Mensch.Id.API.Workflow.FilterExpressionBuilders
{
    public class OrderFilterExpressionBuilder
        : IFilterExpressionBuilder<Order,OrderRequestParameters>
    {
        public List<Expression<Func<Order, bool>>> Build(
            OrderRequestParameters queryParameters)
        {
            var filterExpressions = new List<Expression<Func<Order, bool>>>();
            if (queryParameters.Status != null && queryParameters.Status.Count > 0)
            {
                filterExpressions.Add(x => queryParameters.Status.Contains(x.Status));
            }
            return filterExpressions;
        }
    }
}
