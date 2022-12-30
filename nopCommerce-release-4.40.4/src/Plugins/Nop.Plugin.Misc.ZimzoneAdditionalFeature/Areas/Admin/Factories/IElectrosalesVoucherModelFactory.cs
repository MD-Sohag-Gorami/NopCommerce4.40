using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Voucher;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories
{
    public interface IElectrosalesVoucherModelFactory
    {
        Task<ElectrosalesVoucherListModel> PrepareElectrosalesVoucherListModelAsync(GiftCardSearchModel searchModel);
        Task<ElectrosalesVoucherModel> PrepareElectrosalesVoucherModelAsync(GiftCard giftCard);
    }
}