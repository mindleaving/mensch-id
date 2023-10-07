using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Commons.Extensions;
using Mensch.Id.API.Models;
using Mensch.Id.API.Models.RequestParameters;

namespace Mensch.Id.API.Workflow.FilterExpressionBuilders;

public class AssignedProfileFillterExpressionBuilder : IFilterExpressionBuilder<AssignerControlledProfile, AssignedProfilesRequestParameters>
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

        // Creation time
        if (queryParameters.CreationTimeRangeStart.HasValue)
        {
            filterExpressions.Add(x => x.CreationDate >= queryParameters.CreationTimeRangeStart.Value);
        }
        if (queryParameters.CreationTimeRangeEnd.HasValue)
        {
            filterExpressions.Add(x => x.CreationDate < queryParameters.CreationTimeRangeEnd.Value);
        }

        // Birth date
        if (queryParameters.BirthDateTimeRangeStart.HasValue)
        {
            var birthDateTimeRangeStart = queryParameters.BirthDateTimeRangeStart.Value.ToString("yyyyMMdd");
            filterExpressions.Add(x => x.Id.CompareTo(birthDateTimeRangeStart) >= 0);
        }
        if (queryParameters.BirthDateTimeRangeEnd.HasValue)
        {
            var birthDateTimeRangeEnd = queryParameters.BirthDateTimeRangeEnd.Value.AddDays(1).ToString("yyyyMMdd");
            filterExpressions.Add(x => x.Id.CompareTo(birthDateTimeRangeEnd) < 0);
        }

        return filterExpressions;
    }
}