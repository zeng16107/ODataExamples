CREATE TABLE [dbo].[ProductOverview] (
    [id]                    INT            IDENTITY (1, 1) NOT NULL,
    [product_id]            INT            NULL,
    [overview_description]  NVARCHAR (MAX) NULL,
    [inserted_by]           NVARCHAR (50)  NOT NULL,
    [inserted_datetime]     DATETIME2 (7)  CONSTRAINT [DF_ProductOverview_inserted_datetime] DEFAULT (getutcdate()) NOT NULL,
    [last_updated_by]       NVARCHAR (50)  NOT NULL,
    [last_updated_datetime] DATETIME2 (7)  CONSTRAINT [DF_ProductOverview_last_updated_datetime] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_ProductOverview] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_ProductOverview_Product] FOREIGN KEY ([product_id]) REFERENCES [dbo].[Product] ([id])
);

