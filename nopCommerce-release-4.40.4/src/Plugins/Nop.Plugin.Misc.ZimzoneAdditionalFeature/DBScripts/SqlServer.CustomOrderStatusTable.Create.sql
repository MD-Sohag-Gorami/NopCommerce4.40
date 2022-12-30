IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NS_CustomOrderStatus' and xtype='U')
CREATE TABLE [dbo].[NS_CustomOrderStatus](
    [Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [CustomOrderStatusName] [nvarchar](Max) NOT NULL,
    [DisplayOrder] [int] NOT NULL,
    [ParentOrderStatusId] [int] NOT NULL)
