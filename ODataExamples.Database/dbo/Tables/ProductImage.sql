CREATE TABLE [dbo].[ProductImage] (
    [id]                    INT            IDENTITY (1, 1) NOT NULL,
    [product_id]            INT            NOT NULL,
    [image_url]             NVARCHAR (255) NULL,
    [inserted_by]           NVARCHAR (50)  NOT NULL,
    [inserted_datetime]     DATETIME2 (7)  CONSTRAINT [DF_ProductImage_inserted_datetime] DEFAULT (getutcdate()) NOT NULL,
    [last_updated_by]       NVARCHAR (50)  NOT NULL,
    [last_updated_datetime] DATETIME2 (7)  CONSTRAINT [DF_ProductImage_last_updated_datetime] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_ProductImage] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_ProductImage_Product] FOREIGN KEY ([product_id]) REFERENCES [dbo].[Product] ([id])
);

