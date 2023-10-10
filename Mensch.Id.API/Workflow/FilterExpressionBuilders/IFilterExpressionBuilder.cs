using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mensch.Id.API.Workflow.FilterExpressionBuilders
{
    public interface IFilterExpressionBuilder<TModel, in TParameters>
    {
        List<Expression<Func<TModel, bool>>> Build(
            TParameters queryParameters);
    }
}
