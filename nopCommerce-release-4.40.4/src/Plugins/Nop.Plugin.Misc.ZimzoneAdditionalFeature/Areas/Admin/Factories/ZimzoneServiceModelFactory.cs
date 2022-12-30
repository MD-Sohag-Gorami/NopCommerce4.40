using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Services.Catalog;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories
{

    public class ZimzoneServiceModelFactory : IZimzoneServiceModelFactory
    {
        private readonly IZimzoneServiceEntityService _zimzoneServiceEntityService;
        private readonly IProductService _productService;

        public ZimzoneServiceModelFactory(IZimzoneServiceEntityService zimzoneServiceEntityService, IProductService productService)
        {
            _zimzoneServiceEntityService = zimzoneServiceEntityService;
            _productService = productService;
        }

        public async Task<ServiceListModel> PrepareServiceListModelAsync(ServiceSearchModel searchModel)
        {
            var serviceList = await _zimzoneServiceEntityService.GetAllInfoAsync(searchModel);

            var model = await new ServiceListModel().PrepareToGridAsync(searchModel, serviceList, () =>
            {
                return serviceList.SelectAwait(async m =>
                {

                    var service = new ZimzoneServiceModel
                    {
                        Id = m.Id,
                        ServicePaymentProductId = m.ServicePaymentProductId,
                        IsActive = m.IsActive,
                        IsDeleted = m.IsDeleted,
                        ServiceProductId = m.ServiceProductId,
                        ServiceProductName = m.ServiceProductName,
                        ServicePaymentProductName = m.PaymentProductName
                    };
                    return await Task.FromResult(service);
                });
            });

            return model;
        }
    }
}
