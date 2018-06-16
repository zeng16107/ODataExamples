CREATE TABLE [dbo].[OrderDetail] (
    [id]                    INT           IDENTITY (1, 1) NOT NULL,
    [order_id]              INT           NOT NULL,
    [product_id]            INT           NOT NULL,
    [quantity]              INT           NOT NULL,
    [price]                 DECIMAL (18)  NOT NULL,
    [inserted_by]           NVARCHAR (50) NOT NULL,
    [inserted_datetime]     DATETIME2 (7) CONSTRAINT [DF_OrderDetail_inserted_datetime] DEFAULT (getutcdate()) NOT NULL,
    [last_updated_by]       NVARCHAR (50) NOT NULL,
    [last_updated_datetime] DATETIME2 (7) CONSTRAINT [DF_OrderDetail_last_updated_datetime] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_OrderDetail] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_OrderDetail_Order] FOREIGN KEY ([order_id]) REFERENCES [dbo].[Order] ([id]),
    CONSTRAINT [FK_OrderDetail_Product] FOREIGN KEY ([product_id]) REFERENCES [dbo].[Product] ([id])
);

