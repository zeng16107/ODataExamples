CREATE TABLE [dbo].[Product] (
    [id]                    INT            NOT NULL,
    [type]                  INT            NULL,
    [category]              INT            NULL,
    [brand]                 INT            NULL,
    [name]                  NVARCHAR (100) NULL,
    [description]           NVARCHAR (255) NULL,
    [upc]                   NVARCHAR (15)  NULL,
    [price]                 DECIMAL (18)   NULL,
    [inserted_by]           NVARCHAR (50)  NOT NULL,
    [inserted_datetime]     DATETIME2 (7)  CONSTRAINT [DF_Product_inserted_datetime] DEFAULT (getutcdate()) NOT NULL,
    [last_updated_by]       NVARCHAR (50)  NOT NULL,
    [last_updated_datetime] DATETIME2 (7)  CONSTRAINT [DF_Product_last_updated_datetime] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_Product_ProductBrand] FOREIGN KEY ([brand]) REFERENCES [dbo].[ProductBrand] ([id]),
    CONSTRAINT [FK_Product_ProductType] FOREIGN KEY ([type]) REFERENCES [dbo].[ProductType] ([id])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Product]
    ON [dbo].[Product]([id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Product_Category]
    ON [dbo].[Product]([category] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Product_Brand]
    ON [dbo].[Product]([brand] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Product_Type]
    ON [dbo].[Product]([type] ASC);

