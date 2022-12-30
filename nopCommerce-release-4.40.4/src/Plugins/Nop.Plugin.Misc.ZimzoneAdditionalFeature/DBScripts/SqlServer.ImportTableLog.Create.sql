IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ProductUpdateExcelImportLog' and xtype='U')
CREATE TABLE [dbo].[ProductUpdateExcelImportLog](
    [Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [Sku] [nvarchar](400) NOT NULL,
    [Price] [decimal](18, 2) NOT NULL,
    [Stock] [int] NOT NULL,
    [VendorId] [int] NULL,
    [ErrorMessage] [nvarchar](Max) NULL)
