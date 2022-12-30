IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='NS_OrderWithCustomStatus' and xtype='U')
CREATE TABLE [dbo].[NS_OrderWithCustomStatus](
    [Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [CustomOrderStatusId] [int] NOT NULL,
    [ParentOrderStatusId] [int] NOT NULL,
    [OrderId] [int] NOT NULL)
