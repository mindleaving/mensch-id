using System.Collections.Generic;
using Mensch.Id.API.Models.Shop;

namespace Mensch.Id.API.Models.RequestParameters
{
    public class OrderRequestParameters : GenericItemsRequestParameters
    {
        public List<OrderStatus> Status { get; set; }
    }
}
