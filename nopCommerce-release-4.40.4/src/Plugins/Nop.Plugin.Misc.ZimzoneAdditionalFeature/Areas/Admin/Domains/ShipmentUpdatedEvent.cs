using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Shipping;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains
{
    public class ShipmentUpdatedEvent
    {
        public ShipmentUpdatedEvent(Shipment entity)
        {
            Entity = entity;
        }
        public Shipment Entity
        {
            get;
        }
    }
}
