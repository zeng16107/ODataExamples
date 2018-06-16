CREATE TABLE [dbo].[ProductBrand] (
    [id]                    INT            IDENTITY (1, 1) NOT NULL,
    [product_brand]         NVARCHAR (100) NOT NULL,
    [inserted_by]           NVARCHAR (50)  NOT NULL,
    [inserted_datetime]     DATETIME2 (7)  CONSTRAINT [DF_ProductBrand_inserted_datetime] DEFAULT (getutcdate()) NOT NULL,
    [last_updated_by]       NVARCHAR (50)  NOT NULL,
    [last_updated_datetime] DATETIME2 (7)  CONSTRAINT [DF_ProductBrand_last_updated_datetime] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_ProductBrand] PRIMARY KEY CLUSTERED ([id] ASC)
);

