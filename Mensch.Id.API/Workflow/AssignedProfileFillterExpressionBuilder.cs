using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Mensch.Id.API.Helpers;
using Mensch.Id.API.Models;

namespace Mensch.Id.API.Workflow
{
    public class AssignedProfileFillterExpressionBuilder
    {
        public List<Expression<Func<AssignerControlledProfile, bool>>> Build(
            AssignedProfilesRequestParameters queryParameters)
        {
            var filterExpressions = new List<Expression<Func<AssignerControlledProfile, bool>>>();

            if (!string.IsNullOrWhiteSpace(queryParameters.SearchText))
            {
                var searchTerms = new [] { queryParameters.SearchText.ToLower() };
                var searchTextFilter = SearchExpressionBuilder.ContainsAll<AssignerControlledProfile>(x => x.Id.ToLower(), searchTerms);
                filterExpressions.Add(searchTextFilter);
            }

            if (queryParameters.TimeRangeStart.HasValue)
            {
                filterExpressions.Add(x => x.CreationDate >= queryParameters.TimeRangeStart.Value);
            }
            if (queryParameters.TimeRangeEnd.HasValue)
            {
                filterExpressions.Add(x => x.CreationDate < queryParameters.TimeRangeEnd.Value);
            }

            return filterExpressions;
        }
    }
}
