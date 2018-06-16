CREATE TABLE [dbo].[ProductType] (
    [id]                    INT           NOT NULL,
    [product_type]          NVARCHAR (50) NOT NULL,
    [inserted_by]           NVARCHAR (50) NOT NULL,
    [inserted_datetime]     DATETIME2 (7) NOT NULL,
    [last_updated_by]       NVARCHAR (50) NOT NULL,
    [last_updated_datetime] DATETIME2 (7) NOT NULL,
    CONSTRAINT [PK_ProductType] PRIMARY KEY CLUSTERED ([id] ASC)
);

