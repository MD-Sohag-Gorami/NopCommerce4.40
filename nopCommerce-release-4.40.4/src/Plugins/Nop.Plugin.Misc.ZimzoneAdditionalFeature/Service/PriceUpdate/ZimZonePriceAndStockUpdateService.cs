using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.PriceUpdate;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Vendors;
using NUglify.Helpers;
using OfficeOpenXml;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service.PriceUpdate
{
    public class ZimZonePriceAndStockUpdateService : IZimZonePriceAndStockUpdateService
    {
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IVendorService _vendorService;
        private readonly IRepository<ProductUpdateExcelImportLog> _priceUpdateLogRepository;
        private readonly IGenericAttributeService _genericAttributeService;

        public ZimZonePriceAndStockUpdateService(ILogger logger,
            IWorkContext workContext,
            ICustomerService customerService,
            IVendorService vendorService,
            IRepository<ProductUpdateExcelImportLog> priceUpdateLogRepository,
            IGenericAttributeService genericAttributeService)
        {
            _logger = logger;
            _workContext = workContext;
            _customerService = customerService;
            _vendorService = vendorService;
            _priceUpdateLogRepository = priceUpdateLogRepository;
            _genericAttributeService = genericAttributeService;
        }

        public async Task DeleteImportPriceAndStockErrorLogAsync(int vendorId = 0)
        {
            var query = _priceUpdateLogRepository.Table;

            query = query.Where(x => x.VendorId == vendorId);

            var logs = query.ToList();
            foreach (var log in logs)
            {
                await _priceUpdateLogRepository.DeleteAsync(log);
            }
        }
        public async Task<int> ImportPriceAndStockErrorLogCountByVendorIdAsync(int vendorId)
        {
            var query = _priceUpdateLogRepository.Table;
            query = query.Where(x => x.VendorId == vendorId);
            var logs = query.DistinctBy(x => x.Sku).ToList();
            return await Task.FromResult(logs.Count);
        }
        public async Task ImportPriceAndStockInformationsFromXlsxAsync(Stream stream)
        {
            var connString = Nop.Data.DataSettingsManager.LoadSettings().ConnectionString;
            using (SqlConnection connection = new SqlConnection(connString))
            {
                connection.Open();
                try
                {
                    var query = @"Truncate TABLE [dbo].[ProductUpdateExcelImport];";
                    var cmd = new SqlCommand(query, connection);
                    var rowChanged = cmd.ExecuteNonQuery();
                    connection.Close();
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync(ex.Message);
                    connection.Close();
                }
            }
            var totalRow = await WriteStreamInDatabaseAsync(stream, "ProductUpdateExcelImport");
            var updatedAt = DateTime.UtcNow;
            if (totalRow > 0)
            {
                using (SqlConnection connection = new SqlConnection(connString))
                {
                    connection.Open();
                    try
                    {
                        var query = "[dbo].[ProductUpdateExcelImportProcedure]";
                        var cmd = new SqlCommand(query, connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        var rowChanged = cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (Exception ex)
                    {
                        await _logger.ErrorAsync(ex.Message);
                        connection.Close();
                    }
                }
            }
            var customer = await _workContext.GetCurrentCustomerAsync();
            var isVendor = await _customerService.IsVendorAsync(customer);
            if (isVendor)
            {
                var vendorId = customer.VendorId;
                var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
                var successRowCount = totalRow - await ImportPriceAndStockErrorLogCountByVendorIdAsync(vendorId);
                if (vendor != null)
                    await _genericAttributeService.SaveAttributeAsync(vendor, "ProductUpdateExcelImport.LastSuccess", $"<p>Last Update: {updatedAt}</p></br><p>Updated Product Count: {successRowCount}</p>");
            }

        }

        public async Task<int> WriteStreamInDatabaseAsync(Stream stream, string destinationTableName, bool hasHeader = true)
        {
            var connectionString = DataSettingsManager.LoadSettings().ConnectionString;

            var dataTable = GetDatatableFromXlsx(stream, hasHeader = true);

            dataTable.Columns.Add("VendorId");
            var columnNumber = dataTable.Columns.Count - 1;
            var customer = await _workContext.GetCurrentCustomerAsync();
            var isVendor = await _customerService.IsVendorAsync(customer);
            var vendorId = 0;
            if (isVendor)
            {
                vendorId = customer.VendorId;
                await DeleteImportPriceAndStockErrorLogAsync(vendorId);
            }
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                dataTable.Rows[i][columnNumber] = vendorId;
            }

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString))
            {
                bulkCopy.DestinationTableName = destinationTableName; // Your SQL Table name

                // column mappings
                foreach (DataColumn column in dataTable.Columns)
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }
                //Put your column X number here

                bulkCopy.WriteToServer(dataTable);
            }

            return await Task.FromResult(dataTable?.Rows?.Count ?? 0);
        }

        private DataTable GetDatatableFromXlsx(Stream stream, bool hasHeader = true)
        {
            using var xlPackage = new ExcelPackage(stream);
            // get the first worksheet in the workbook
            var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new NopException("No worksheet found");

            var tbl = new DataTable();
            foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.Start.Column + 2])
            {
                tbl.Columns.Add(hasHeader ? GetProcessedNameWithOutSpace(firstRowCell.Text) : string.Format("Column {0}", firstRowCell.Start.Column));
            }
            var startRow = hasHeader ? 2 : 1;
            for (var rowNum = startRow; rowNum <= worksheet.Dimension.End.Row; rowNum++)
            {
                var wsRow = worksheet.Cells[rowNum, 1, rowNum, worksheet.Dimension.Start.Column + 2];
                var row = tbl.Rows.Add();
                var valid = true;
                foreach (var cell in wsRow)
                {
                    if (string.IsNullOrEmpty(cell.Text) || string.IsNullOrWhiteSpace(cell.Text))
                    {
                        valid = false;
                        break;
                    }
                }

                if (!valid)
                {
                    continue;
                }

                foreach (var cell in wsRow)
                {
                    var currentCol = cell.Start.Column - 1;
                    if (currentCol == 2)
                    {
                        row[currentCol] = cell.Text.Replace(",", "");
                    }
                    else
                    {
                        row[currentCol] = cell.Text;
                    }
                }
            }
            return tbl;
        }

        private string GetProcessedNameWithOutSpace(string str)
        {
            var regex = new Regex(@"[\s]+([a-z0-9])", RegexOptions.IgnoreCase);
            str = regex.Replace(str, m => m.ToString().Trim().ToUpper());

            return str;
        }
    }
}
