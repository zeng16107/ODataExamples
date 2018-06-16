CREATE TABLE [dbo].[Order] (
    [id]                    INT           IDENTITY (1, 1) NOT NULL,
    [customer_id]           INT           NOT NULL,
    [order_number]          NVARCHAR (50) NOT NULL,
    [order_status]          INT           NOT NULL,
    [inserted_by]           NVARCHAR (50) NOT NULL,
    [inserted_datetime]     DATETIME2 (7) CONSTRAINT [DF_Order_inserted_datetime2] DEFAULT (getutcdate()) NOT NULL,
    [last_updated_by]       NVARCHAR (50) NOT NULL,
    [last_updated_datetime] DATETIME2 (7) CONSTRAINT [DF_Order_last_updated_datetime] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_Order_Customer] FOREIGN KEY ([customer_id]) REFERENCES [dbo].[Customer] ([id]),
    CONSTRAINT [FK_Order_Customer1] FOREIGN KEY ([customer_id]) REFERENCES [dbo].[Customer] ([id])
);

