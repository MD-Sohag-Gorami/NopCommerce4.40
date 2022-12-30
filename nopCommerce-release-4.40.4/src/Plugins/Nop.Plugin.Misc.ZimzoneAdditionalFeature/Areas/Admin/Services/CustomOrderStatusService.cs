using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Data;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.CustomOrderStatus;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services
{
    public class CustomOrderStatusService : ICustomOrderStatusService
    {
        private readonly IRepository<CustomOrderStatus> _customOrderStatusRepository;
        private readonly IRepository<OrderWithCustomStatus> _orderWithCustomStatusRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<OrderNote> _orderNoteRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IOrderService _orderService;

        public CustomOrderStatusService(IRepository<CustomOrderStatus> customOrderStatusRepository,
                                        IRepository<OrderWithCustomStatus> orderWithCustomStatusRepository,
                                        IRepository<Order> orderRepository,
                                        IRepository<OrderItem> orderItemRepository,
                                        IRepository<Product> productRepository,
                                        IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
                                        IRepository<Address> addressRepository,
                                        IRepository<OrderNote> orderNoteRepository,
                                        IEventPublisher eventPublisher,
                                        IOrderService orderService)
        {
            _customOrderStatusRepository = customOrderStatusRepository;
            _orderWithCustomStatusRepository = orderWithCustomStatusRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
            _addressRepository = addressRepository;
            _orderNoteRepository = orderNoteRepository;
            _eventPublisher = eventPublisher;
            _orderService = orderService;
        }

        public async Task InsertCustomOrderStatusAsync(CustomOrderStatusModel model)
        {
            var customOrderStaus = model.ToEntity<CustomOrderStatus>();
            await _customOrderStatusRepository.InsertAsync(customOrderStaus);
        }

        public async Task<List<CustomOrderStatus>> GetAllCustomOrderStatusAsync()
        {
            var query = _customOrderStatusRepository.Table;
            return await query.ToListAsync();
        }

        public async Task<IPagedList<CustomOrderStatus>> GetAllPagedCustomOrderStatusAsync(string customOrderStatusName = "", int parentOrderStatusId = 0)
        {
            var query = _customOrderStatusRepository.Table;

            if (!string.IsNullOrEmpty(customOrderStatusName))
            {
                query = query.Where(x => x.CustomOrderStatusName.Contains(customOrderStatusName));
            }
            if (parentOrderStatusId != 0)
            {
                query = query.Where(x => x.ParentOrderStatusId == parentOrderStatusId);
            }

            query = query.OrderByDescending(x => x.Id);

            return await query.ToPagedListAsync(pageIndex: 0, pageSize: int.MaxValue);
        }

        public Task<CustomOrderStatus> GetCustomOrderStatusByIdAsync(int id)
        {
            var query = _customOrderStatusRepository.Table;
            query = query.Where(x => x.Id == id);
            var customOrderStatus = query.FirstOrDefault();
            return Task.FromResult(customOrderStatus);
        }
        public async Task DeleteCustomOrderStatusAsync(int id)
        {
            var domain = await GetCustomOrderStatusByIdAsync(id);
            try
            {
                await _customOrderStatusRepository.DeleteAsync(domain);
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException(nameof(domain));
            }
        }

        public async Task DeleteCustomOrderStatusAsync(List<int> ids)
        {
            foreach (var id in ids)
            {
                await DeleteCustomOrderStatusAsync(id);
            }
        }

        public async Task UpdateCustomOrderStatusAsync(CustomOrderStatusModel model)
        {
            var customOrderStaus = await GetCustomOrderStatusByIdAsync(model.Id);

            if (customOrderStaus != null)
            {
                customOrderStaus.CustomOrderStatusName = model.CustomOrderStatusName;
                customOrderStaus.DisplayOrder = model.DisplayOrder;
            }

            await _customOrderStatusRepository.UpdateAsync(customOrderStaus);
        }

        public Task<bool> ValidateNameAndParentStatus(CustomOrderStatusModel model)
        {
            var query = _customOrderStatusRepository.Table;
            query = query.Where(x => x.CustomOrderStatusName == model.CustomOrderStatusName && x.ParentOrderStatusId == model.ParentOrderStatusId);
            var customStatus = query.FirstOrDefault();
            return Task.FromResult(customStatus == null);
        }

        public async Task<List<CustomOrderStatus>> GetCustomOrderStatusByParentOrderStatusIdAsync(int parentOrderStatusId)
        {
            var query = _customOrderStatusRepository.Table.Where(x => x.ParentOrderStatusId == parentOrderStatusId).OrderBy(y => y.DisplayOrder);

            var customOrderStatus = await query.ToListAsync();

            return customOrderStatus;
        }

        private async Task InsertOrderNoteInCustomOrderStatusChangeAsync(OrderWithCustomStatus orderWithCustomStatus)
        {
            var customOrderStatus = await GetCustomOrderStatusByIdAsync(orderWithCustomStatus.CustomOrderStatusId);

            var note = $"Custom Order status has been edited. New status: {customOrderStatus.CustomOrderStatusName}";
            await _orderService.InsertOrderNoteAsync(new OrderNote()
            {
                OrderId = orderWithCustomStatus.OrderId,
                Note = note,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
        }

        public async Task<CustomStatusListWithOrderModel> InsertOrderWithCustomStatusAsync(CustomStatusListWithOrderModel model)
        {
            var orderWithCustomStatus = model.ToEntity<OrderWithCustomStatus>();

            await _orderWithCustomStatusRepository.InsertAsync(orderWithCustomStatus);

            if (model.IsMarkedToSendEmail)
            {
                await _eventPublisher.PublishAsync(new CustomOrderStatusChangedEvent(orderWithCustomStatus));
            }

            await InsertOrderNoteInCustomOrderStatusChangeAsync(orderWithCustomStatus);

            model.Id = orderWithCustomStatus.Id;
            return model;
        }

        public async Task UpdateOrderWithCustomStatusAsync(CustomStatusListWithOrderModel model)
        {
            var orderWithCustomStatus = await GetOrderWithCustomStatusAsync(id: model.Id);

            if (orderWithCustomStatus != null)
            {
                orderWithCustomStatus.ParentOrderStatusId = model.ParentOrderStatusId;
                orderWithCustomStatus.CustomOrderStatusId = model.CustomOrderStatusId;
            }
            await _orderWithCustomStatusRepository.UpdateAsync(orderWithCustomStatus);

            if (model.IsMarkedToSendEmail)
            {
                await _eventPublisher.PublishAsync(new CustomOrderStatusChangedEvent(orderWithCustomStatus));
            }

            await InsertOrderNoteInCustomOrderStatusChangeAsync(orderWithCustomStatus);
        }

        public async Task<OrderWithCustomStatus> GetOrderWithCustomStatusAsync(int id = 0, int parentOrderStatusId = 0, int orderId = 0)
        {
            var query = _orderWithCustomStatusRepository.Table;
            if (id != 0)
            {
                query = query.Where(x => x.Id == id);
            }
            if (parentOrderStatusId != 0)
            {
                query = query.Where(x => x.ParentOrderStatusId == parentOrderStatusId);
            }
            if (orderId != 0)
            {
                query = query.Where(x => x.OrderId == orderId);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteOrderWithCustomStatusAsync(int id = 0, OrderWithCustomStatus orderWithCustomStatus = null)
        {
            if (id > 0)
            {
                orderWithCustomStatus = await GetOrderWithCustomStatusAsync(id: id);
            }
            if (orderWithCustomStatus == null)
            {
                return false;
            }
            await _orderWithCustomStatusRepository.DeleteAsync(orderWithCustomStatus);
            return true;
        }

        public async Task<List<OrderWithCustomStatus>> GetAllOrderWithCustomStatusAsync(int customOrderStatusId = 0, int parentOrderStatusId = 0)
        {
            var query = _orderWithCustomStatusRepository.Table;
            if (parentOrderStatusId != 0)
            {
                query = query.Where(x => x.ParentOrderStatusId == parentOrderStatusId);
            }
            if (customOrderStatusId != 0)
            {
                query = query.Where(x => x.CustomOrderStatusId == customOrderStatusId);
            }

            return await query.ToListAsync();
        }

        public async Task<IPagedList<OrderCustom>> SearchOrderWithCustomStatusAsync(int storeId = 0,
            int vendorId = 0, int customerId = 0,
            int productId = 0, int affiliateId = 0, int warehouseId = 0,
            int billingCountryId = 0, string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            List<int> osIds = null, List<int> cosIds = null, List<int> psIds = null, List<int> ssIds = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "",
            string orderNotes = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var query = _orderRepository.Table;

            if (storeId > 0)
                query = query.Where(o => o.StoreId == storeId);

            if (vendorId > 0)
            {
                query = from o in query
                        join oi in _orderItemRepository.Table on o.Id equals oi.OrderId
                        join p in _productRepository.Table on oi.ProductId equals p.Id
                        where p.VendorId == vendorId
                        select o;

                query = query.Distinct();
            }

            if (customerId > 0)
                query = query.Where(o => o.CustomerId == customerId);

            if (productId > 0)
                query = from o in query
                        join oi in _orderItemRepository.Table on o.Id equals oi.OrderId
                        where oi.ProductId == productId
                        select o;

            if (warehouseId > 0)
            {
                var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;

                query = from o in query
                        join oi in _orderItemRepository.Table on o.Id equals oi.OrderId
                        join p in _productRepository.Table on oi.ProductId equals p.Id
                        join pwi in _productWarehouseInventoryRepository.Table on p.Id equals pwi.ProductId into ps
                        from pwi in ps.DefaultIfEmpty()
                        where
                        //"Use multiple warehouses" enabled
                        //we search in each warehouse
                        (p.ManageInventoryMethodId == manageStockInventoryMethodId && p.UseMultipleWarehouses && pwi.WarehouseId == warehouseId) ||
                        //"Use multiple warehouses" disabled
                        //we use standard "warehouse" property
                        ((p.ManageInventoryMethodId != manageStockInventoryMethodId || !p.UseMultipleWarehouses) && p.WarehouseId == warehouseId)
                        select o;
            }

            if (!string.IsNullOrEmpty(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);

            if (affiliateId > 0)
                query = query.Where(o => o.AffiliateId == affiliateId);

            if (createdFromUtc.HasValue)
                query = query.Where(o => createdFromUtc.Value <= o.CreatedOnUtc);

            if (createdToUtc.HasValue)
                query = query.Where(o => createdToUtc.Value >= o.CreatedOnUtc);

            if (osIds != null && osIds.Any())
                query = query.Where(o => osIds.Contains(o.OrderStatusId));

            if (psIds != null && psIds.Any())
                query = query.Where(o => psIds.Contains(o.PaymentStatusId));

            if (ssIds != null && ssIds.Any())
                query = query.Where(o => ssIds.Contains(o.ShippingStatusId));

            if (!string.IsNullOrEmpty(orderNotes))
                query = query.Where(o => _orderNoteRepository.Table.Any(oNote => oNote.OrderId == o.Id && oNote.Note.Contains(orderNotes)));

            query = from o in query
                    join oba in _addressRepository.Table on o.BillingAddressId equals oba.Id
                    where
                        (billingCountryId <= 0 || (oba.CountryId == billingCountryId)) &&
                        (string.IsNullOrEmpty(billingPhone) || (!string.IsNullOrEmpty(oba.PhoneNumber) && oba.PhoneNumber.Contains(billingPhone))) &&
                        (string.IsNullOrEmpty(billingEmail) || (!string.IsNullOrEmpty(oba.Email) && oba.Email.Contains(billingEmail))) &&
                        (string.IsNullOrEmpty(billingLastName) || (!string.IsNullOrEmpty(oba.LastName) && oba.LastName.Contains(billingLastName)))
                    select o;

            query = query.Where(o => !o.Deleted);
            query = query.OrderByDescending(o => o.CreatedOnUtc);

            //first join with order with custom status table to get custom status id and select new object of order custom
            var newQuery = from o in query
                           join owcs in _orderWithCustomStatusRepository.Table
                           on o.Id equals owcs.OrderId
                           into orderswithcustomstatus
                           from cs in orderswithcustomstatus.DefaultIfEmpty()
                           select new OrderCustom { Order = o, CustomOrderStatusId = cs.CustomOrderStatusId };

            // join with custom order status table to get custom status name.
            newQuery = from owcs in newQuery
                       join cs in _customOrderStatusRepository.Table
                       on owcs.CustomOrderStatusId equals cs.Id into owcsGroup
                       from s in owcsGroup.DefaultIfEmpty()
                       select new OrderCustom { Order = owcs.Order, CustomOrderStatusId = owcs.CustomOrderStatusId, CustomOrderStatusName = s.CustomOrderStatusName };


            if (cosIds != null && cosIds.Any())
            {
                newQuery = newQuery.Where(x => cosIds.Contains(x.CustomOrderStatusId));
            }

            //database layer paging
            //return await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
            return await newQuery.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
        }

    }
}
