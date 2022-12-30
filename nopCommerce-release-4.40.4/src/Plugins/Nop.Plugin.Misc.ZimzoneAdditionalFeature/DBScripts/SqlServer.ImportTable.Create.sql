IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ProductUpdateExcelImport' and xtype='U')
CREATE TABLE [dbo].[ProductUpdateExcelImport](
    [Sku] [nvarchar](400) NOT NULL,
    [Price] [decimal](18, 2) NOT NULL,
    [Stock] [int] NOT NULL,
    [VendorId] [int] NULL)
