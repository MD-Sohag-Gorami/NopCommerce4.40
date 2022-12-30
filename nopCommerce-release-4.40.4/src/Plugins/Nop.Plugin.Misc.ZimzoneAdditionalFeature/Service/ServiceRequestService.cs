using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly IRepository<ZimzoneServiceRequestEntity> _repository;
        private readonly ILogger _logger;
        private readonly IZimzoneServiceEntityService _zimzoneServiceEntityService;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ServiceAttributeSettings _serviceAttributeSettings;
        private readonly ICustomerService _customerService;
        private readonly IRepository<ShoppingCartItem> _sciRepository;

        public ServiceRequestService(IRepository<ZimzoneServiceRequestEntity> repository,
            ILogger logger,
            IZimzoneServiceEntityService zimzoneServiceEntityService,
            IProductService productService,
            IStoreContext storeContext,
            IEventPublisher eventPublisher,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            ServiceAttributeSettings serviceAttributeSettings,
            ICustomerService customerService,
            IRepository<ShoppingCartItem> sciRepository)
        {
            _repository = repository;
            _logger = logger;
            _zimzoneServiceEntityService = zimzoneServiceEntityService;
            _productService = productService;
            _storeContext = storeContext;
            _eventPublisher = eventPublisher;
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
            _serviceAttributeSettings = serviceAttributeSettings;
            _customerService = customerService;
            _sciRepository = sciRepository;
        }

        public async Task<bool> AddToCartAsync(ZimzoneServiceRequestEntity request, Customer customer)
        {
            var service = await _zimzoneServiceEntityService.GetById(request.ZimZoneServiceId);
            if (service == null)
            {
                return false;
            }
            var product = await _productService.GetProductByIdAsync(service.ServicePaymentProductId);
            if (product == null)
            {
                return false;
            }
            //we can add only simple products
            if (!product.CustomerEntersPrice)
            {
                return false;
            }

            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

            var shoppingCartService = EngineContext.Current.Resolve<IShoppingCartService>();

            var cart = await shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, storeId);

            var updatecartitem = cart.Where(x => x.AttributesXml == request.Id.ToString()).FirstOrDefault();

            if (updatecartitem != null)
            {
                if (updatecartitem.CustomerEnteredPrice != request.AgreedAmount)
                {
                    updatecartitem.CustomerEnteredPrice = request.AgreedAmount;
                    updatecartitem.AttributesXml = request.Id.ToString();
                    await shoppingCartService.UpdateShoppingCartItemAsync(customer, updatecartitem.Id, updatecartitem.AttributesXml, request.AgreedAmount);
                }
                return true;
            }

            var addToCartWarnings = new List<string>();

            //customer entered price
            var customerEnteredPriceConverted = request.AgreedAmount;

            //entered quantity
            var quantity = 1;
            //product and gift card attributes
            var cartType = ShoppingCartType.ShoppingCart;
            //if the item to update is found, then we ignore the specified "shoppingCartTypeId" parameter

            var attributeXml = request.Id.ToString();
            var warnings = await shoppingCartService.AddToCartAsync(customer,
                    product, cartType, storeId,
                    attributeXml, customerEnteredPriceConverted,
                    null, null, quantity, true);
            if (warnings.Any())
            {
                return false;
            }
            return true;
        }

        public async Task DeleteRequestAsync(ZimzoneServiceRequestEntity request)
        {
            await _repository.DeleteAsync(request);
        }

        public async Task<IList<ZimzoneServiceRequestEntity>> GetAllRequestsAsync(int customerId = 0)
        {
            var query = _repository.Table;
            if (customerId != 0)
            {
                query = query.Where(x => x.CustomerId == customerId && !x.IsDeleted);
                query = query.OrderByDescending(x => x.Id);
            }
            var requests = query.ToList();
            return await Task.FromResult(requests);
        }
        public async Task<IList<ZimzoneServiceRequestEntity>> GetAllRequestsAsync(Customer customer)
        {
            if (customer == null)
            {
                return new List<ZimzoneServiceRequestEntity>();
            }
            var query = _repository.Table;
            if (await _customerService.IsRegisteredAsync(customer))
            {
                query = query.Where(x => (x.CustomerEmail == customer.Email || customer.Id == x.CustomerId) && !x.IsDeleted);
                query = query.OrderByDescending(x => x.Id);
            }
            var requests = query.ToList();
            return await Task.FromResult(requests);
        }
        public async Task<IList<ZimzoneServiceRequestEntity>> GetAllRequestsAsync(ServiceRequestSearchModel searchModel)
        {
            var query = _repository.Table;
            if (!string.IsNullOrEmpty(searchModel.CustomerEmail))
            {
                query = query.Where(x => x.CustomerEmail == searchModel.CustomerEmail);
            }
            if (!string.IsNullOrEmpty(searchModel.CustomerName))
            {
                query = query.Where(x => x.CustomerName.Contains(searchModel.CustomerName));
            }
            if (searchModel.StatusId != 0)
            {
                query = searchModel.StatusId == 1 ? query.Where(x => !x.IsAgreed) : query.Where(x => x.IsAgreed);
            }
            if (searchModel.ServiceId != 0)
            {
                query = query.Where(x => x.ZimZoneServiceId == searchModel.ServiceId);
            }
            query = query.OrderByDescending(x => x.Id);
            var requests = query.ToList();
            return await Task.FromResult(requests);
        }
        public async Task<ZimzoneServiceRequestEntity> GetRequestByIdAsync(int id)
        {
            if (id == 0)
                return null;
            var query = _repository.Table;
            query = query.Where(x => x.Id == id);
            var request = query.FirstOrDefault();
            return await Task.FromResult(request);
        }

        public async Task InsertRequestAsync(ZimzoneServiceRequestEntity request)
        {
            await _repository.InsertAsync(request);
            await _eventPublisher.PublishAsync(new ServiceRequestSubmittedEvent(request));
        }

        public async Task<(string name, string email, string address, string description, string downloadId)> ParseServiceRequestAttributeAsync(string attributeXml, Product product, string description = "")
        {
            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product?.Id ?? 0);
            var name = string.Empty;
            var email = string.Empty;
            var address = string.Empty;
            var fileDownloadGuid = string.Empty;
            //var productAttribute
            foreach (var mapping in productAttributeMapping)
            {
                var value = (_productAttributeParser.ParseValues(attributeXml, mapping?.Id ?? 0)).FirstOrDefault();
                if (!string.IsNullOrEmpty(value))
                {
                    var attribute = await _productAttributeService.GetProductAttributeByIdAsync(mapping.ProductAttributeId);
                    if (attribute != null)
                    {
                        if (attribute.Id == _serviceAttributeSettings.NameAttributeId)
                        {
                            name = value;
                        }
                        else if (attribute.Id == _serviceAttributeSettings.EmailAttributeId)
                        {
                            email = value;
                        }
                        else if (attribute.Id == _serviceAttributeSettings.AddressAttributeId)
                        {
                            address = value;
                        }
                        else if (attribute.Id == _serviceAttributeSettings.DescriptionAttributeId && string.IsNullOrEmpty(description))
                        {
                            description = value;
                        }
                        else if (attribute.Id == _serviceAttributeSettings.FileAttributeId)
                        {
                            fileDownloadGuid = value;
                        }
                    }
                }
            }
            return (name, email, address, description, fileDownloadGuid);
        }

        public async Task<(string name, string email, string address, string description, string downloadId)> ParseServiceRequestAttributeAsync(ZimzoneServiceRequestEntity request)
        {
            var service = await _zimzoneServiceEntityService.GetById(request?.ZimZoneServiceId ?? 0);
            var product = await _productService.GetProductByIdAsync(service?.ServiceProductId ?? 0);
            var (name, email, _, _, downloadId) = await ParseServiceRequestAttributeAsync(request?.ServiceDetailsAttr, product, request.Description);
            return (name, email, request.CustomerAddress, request.Description, downloadId);
        }

        //public async Task<(string name, string email, string description, string fileDownloadGuid)> ParseRequestInfoAsync(OverridenProductAttributeParser product, ServiceAttributeSettings serviceAttributeSettings)
        //{
        //    if(product==null || serviceAttributeSettings==null)
        //    {
        //        return await Task.FromResult((string name, string email, string description, string fileDownloadGuid));
        //    }
        //}

        public async Task UpdateRequestAsync(ZimzoneServiceRequestEntity request)
        {
            await _repository.UpdateAsync(request);
        }
    }
}
