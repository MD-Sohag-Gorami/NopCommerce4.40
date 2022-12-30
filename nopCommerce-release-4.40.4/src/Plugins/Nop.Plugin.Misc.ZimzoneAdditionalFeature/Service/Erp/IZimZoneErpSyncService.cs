using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Misc.ErpSync.Services
{
    public interface IZimZoneErpSyncService
    {
        Task<bool> SyncProductsAsync(string sku = "", DateTime? lastUpdatedTime = null);
        Task<bool> SyncCategoriesAsync();
        Task<int> SyncErpStockAsync(string sku);
        Task<Product> SyncErpStockAsync(Product product);
        Task SyncErpStocksAsync(IList<string> skus);
        Task SyncAsync(DateTime? lastUpdatedTime = null);
    }
}