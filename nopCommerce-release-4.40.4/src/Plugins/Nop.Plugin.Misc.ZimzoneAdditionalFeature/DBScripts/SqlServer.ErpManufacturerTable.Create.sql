IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NS_ErpManufacturer' and xtype='U')

CREATE TABLE [dbo].[NS_ErpManufacturer]
(
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BrandName] [nvarchar](max) NULL,
	[Active] [bit] NULL,
	[NopManufacturerId] [int] NOT NULL,
	[ErpManufacturerId] [nvarchar](max) NOT NULL,
	[ImageLink] [nvarchar](max) NULL,
	[CreatedOn] [datetime2](7) NULL,
	[ModifiedOn] [datetime2](7) NULL,
 CONSTRAINT [PK_NS_ErpManufacturer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO