using Mensch.Id.API.Models.Shop;

namespace Mensch.Id.API.Models.RequestParameters
{
    public class OrderRequestParameters : GenericItemsRequestParameters
    {
        public OrderStatus? Status { get; set; }
    }
}
