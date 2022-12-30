using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service
{
    public class OverriddenOrderReportService : OrderReportService
    {
        private readonly CurrencySettings _currencySettings;
        private readonly ICurrencyService _currencyService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<OrderNote> _orderNoteRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;
        private readonly IZimzoneServiceEntityService _zimzoneServiceEntityService;
        private readonly IProductService _productService;

        public OverriddenOrderReportService(CurrencySettings currencySettings,
            ICurrencyService currencyService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            IRepository<Address> addressRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<OrderNote> orderNoteRepository,
            IRepository<Product> productRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            IZimzoneServiceEntityService zimzoneServiceEntityService,
            IProductService productService) : base(currencySettings,
                                             currencyService,
                                             dateTimeHelper,
                                             priceFormatter,
                                             addressRepository,
                                             orderRepository,
                                             orderItemRepository,
                                             orderNoteRepository,
                                             productRepository,
                                             productCategoryRepository,
                                             productManufacturerRepository,
                                             productWarehouseInventoryRepository,
                                             storeMappingService,
                                             workContext)
        {
            _currencySettings = currencySettings;
            _currencyService = currencyService;
            _dateTimeHelper = dateTimeHelper;
            _priceFormatter = priceFormatter;
            _addressRepository = addressRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _orderNoteRepository = orderNoteRepository;
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
            _zimzoneServiceEntityService = zimzoneServiceEntityService;
            _productService = productService;
        }

        //copied from OrderReportService as its a private method
        private IQueryable<OrderItem> OverriddenSearchOrderItems(
            int categoryId = 0,
            int manufacturerId = 0,
            int storeId = 0,
            int vendorId = 0,
            DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null,
            OrderStatus? os = null,
            PaymentStatus? ps = null,
            ShippingStatus? ss = null,
            int billingCountryId = 0,
            bool showHidden = false)
        {
            int? orderStatusId = null;
            if (os.HasValue)
                orderStatusId = (int)os.Value;

            int? paymentStatusId = null;
            if (ps.HasValue)
                paymentStatusId = (int)ps.Value;

            int? shippingStatusId = null;
            if (ss.HasValue)
                shippingStatusId = (int)ss.Value;

            var orderItems = from orderItem in _orderItemRepository.Table
                             join o in _orderRepository.Table on orderItem.OrderId equals o.Id
                             join p in _productRepository.Table on orderItem.ProductId equals p.Id
                             join oba in _addressRepository.Table on o.BillingAddressId equals oba.Id
                             where (storeId == 0 || storeId == o.StoreId) &&
                                 (!createdFromUtc.HasValue || createdFromUtc.Value <= o.CreatedOnUtc) &&
                                 (!createdToUtc.HasValue || createdToUtc.Value >= o.CreatedOnUtc) &&
                                 (!orderStatusId.HasValue || orderStatusId == o.OrderStatusId) &&
                                 (!paymentStatusId.HasValue || paymentStatusId == o.PaymentStatusId) &&
                                 (!shippingStatusId.HasValue || shippingStatusId == o.ShippingStatusId) &&
                                 !o.Deleted && !p.Deleted &&
                                 (vendorId == 0 || p.VendorId == vendorId) &&
                                 (billingCountryId == 0 || oba.CountryId == billingCountryId) &&
                                 (showHidden || p.Published)
                             select orderItem;

            if (categoryId > 0)
            {
                orderItems = from orderItem in orderItems
                             join p in _productRepository.Table on orderItem.ProductId equals p.Id
                             join pc in _productCategoryRepository.Table on p.Id equals pc.ProductId
                             into p_pc
                             from pc in p_pc.DefaultIfEmpty()
                             where pc.CategoryId == categoryId
                             select orderItem;
            }

            if (manufacturerId > 0)
            {
                orderItems = from orderItem in orderItems
                             join p in _productRepository.Table on orderItem.ProductId equals p.Id
                             join pm in _productManufacturerRepository.Table on p.Id equals pm.ProductId
                             into p_pm
                             from pm in p_pm.DefaultIfEmpty()
                             where pm.ManufacturerId == manufacturerId
                             select orderItem;
            }

            return orderItems;
        }

        public async override Task<IPagedList<BestsellersReportLine>> BestSellersReportAsync(int categoryId = 0, int manufacturerId = 0, int storeId = 0, int vendorId = 0, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, OrderStatus? os = null, PaymentStatus? ps = null, ShippingStatus? ss = null, int billingCountryId = 0, OrderByEnum orderBy = OrderByEnum.OrderByQuantity, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var bestSellers = OverriddenSearchOrderItems(categoryId, manufacturerId, storeId, vendorId, createdFromUtc, createdToUtc, os, ps, ss, billingCountryId, showHidden);

            var bsReport =
                //group by products
                from orderItem in bestSellers
                group orderItem by orderItem.ProductId into g
                select new BestsellersReportLine
                {
                    ProductId = g.Key,
                    TotalAmount = g.Sum(x => x.PriceExclTax),
                    TotalQuantity = g.Sum(x => x.Quantity)
                };

            bsReport = orderBy switch
            {
                OrderByEnum.OrderByQuantity => bsReport.OrderByDescending(x => x.TotalQuantity),
                OrderByEnum.OrderByTotalAmount => bsReport.OrderByDescending(x => x.TotalAmount),
                _ => throw new ArgumentException("Wrong orderBy parameter", nameof(orderBy)),
            };

            #region Filtering Service Product and Gift card

            var paymentProductIds = await _zimzoneServiceEntityService.GetAllPaymentProductIdAsync(true);

            bsReport = bsReport.Where(x => !paymentProductIds.Contains(x.ProductId));


            bsReport = from b in bsReport
                       join p in _productRepository.Table on b.ProductId equals p.Id
                       where p.IsGiftCard == false
                       select b;

            #endregion

            var result = await bsReport.ToPagedListAsync(pageIndex, pageSize);

            return result;
        }
    }
}
