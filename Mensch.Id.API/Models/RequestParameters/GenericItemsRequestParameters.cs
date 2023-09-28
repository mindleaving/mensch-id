using TypescriptGenerator.Attributes;

namespace Mensch.Id.API.Models.RequestParameters;

public class GenericItemsRequestParameters
{
    public int? Count { get; set; }
    public int Skip { get; set; } = 0;
    [TypescriptIsOptional]
    public string SearchText { get; set; }
    [TypescriptIsOptional]
    public string OrderBy { get; set; }
    public OrderDirection OrderDirection { get; set; } = OrderDirection.Ascending;
}