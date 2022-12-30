IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NS_OrderWithCustomStatus]') AND type in (N'U'))
DROP TABLE [dbo].[NS_OrderWithCustomStatus];
