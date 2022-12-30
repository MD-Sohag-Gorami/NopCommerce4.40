
CREATE PROCEDURE [dbo].[ProductUpdateExcelImportProcedure]
AS
BEGIN
-- current UTCDateTime
DECLARE @CurrentUTCDATE datetime = GETUTCDATE();
-- Declare the variables to store the values returned by FETCH.  
DECLARE @Sku nvarchar (400),
	@Price decimal (18, 2),
    @Stock int,
    @VendorId int;

	-- cursor for import user
	DECLARE product_update_excel_import_cursor CURSOR FAST_FORWARD FOR  
	SELECT  [Sku]
			,[Price]
            ,[Stock]
            ,[VendorId]
		FROM [dbo].[ProductUpdateExcelImport];

	OPEN product_update_excel_import_cursor;

	-- Perform the first fetch
	FETCH NEXT FROM product_update_excel_import_cursor INTO  @Sku, @Price, @Stock, @VendorId;

	-- Check @@FETCH_STATUS to see if there are any more rows to fetch.  
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DECLARE @productId int = 0
		SET @productId = (SELECT Top(1) Id FROM [dbo].[Product] product with (NOLOCK) where product.Sku = @Sku AND Deleted=0);

		IF @productId > 0 
		    BEGIN
                BEGIN TRY

                    IF EXISTS (SELECT TOP(1) * FROM [dbo].[Product] WHERE Id = @productId)
			            BEGIN
                            IF @VendorId is NOT NULL AND @VendorId>0
								BEGIN
									DECLARE @productVendorId int = 0
									SET @productVendorId = (SELECT Top(1) VendorId FROM [dbo].[Product] product with (NOLOCK) where Id = @productId);
									IF  @productVendorId = @VendorId
										BEGIN
											Update [dbo].[Product]
											set Price=@Price,
											StockQuantity=@Stock
											where Id = @productId
										END
									ELSE 
										BEGIN
												INSERT INTO [dbo].[ProductUpdateExcelImportLog]
												([Sku],
												[Price],
												[Stock],
												[VendorId],
												[ErrorMessage])
												VALUES
													(@Sku
													,@Price
													,@Stock
													,@VendorId
													,'Product is not associated with the current vendor')
										END
								END
                             ELSE
                                BEGIN
                                    Update [dbo].[Product]
				                    set Price=@Price,
                                    StockQuantity=@Stock
					                where Id = @productId
                                END 
			            END
                END TRY

                BEGIN CATCH
                    DECLARE @ErrorMessage nvarchar(max) = isnull(ERROR_MESSAGE(), '')
                    INSERT INTO [dbo].[ProductUpdateExcelImportLog]
					([Sku],
                    [Price],
                    [Stock],
                    [VendorId],
                    [ErrorMessage])
				    VALUES
					    (@Sku
					    ,@Price
					    ,@Stock
					    ,@VendorId
					    ,@ErrorMessage)
                END CATCH
		    END
        ELSE
            BEGIN
                INSERT INTO [dbo].[ProductUpdateExcelImportLog]
					([Sku],
                    [Price],
                    [Stock],
                    [VendorId],
                    [ErrorMessage])
				    VALUES
					    (@Sku
					    ,@Price
					    ,@Stock
					    ,@VendorId
					    ,'Not found')
			END
        FETCH NEXT FROM product_update_excel_import_cursor INTO @Sku, @Price, @Stock, @VendorId;
	END
		
	CLOSE product_update_excel_import_cursor;  
	DEALLOCATE product_update_excel_import_cursor; 

	TRUNCATE TABLE [dbo].[ProductUpdateExcelImport];
END
