CREATE TABLE [dbo].[Specification] (
    [id]                    INT            IDENTITY (1, 1) NOT NULL,
    [product_id]            INT            NOT NULL,
    [spec_type]             INT            NOT NULL,
    [spec_detail]           NVARCHAR (255) NOT NULL,
    [inserted_by]           NVARCHAR (50)  NOT NULL,
    [inserted_datetime]     DATETIME2 (7)  CONSTRAINT [DF_Specification_inserted_datetime] DEFAULT (getutcdate()) NOT NULL,
    [last_updated_by]       NVARCHAR (50)  NOT NULL,
    [last_updated_datetime] DATETIME2 (7)  CONSTRAINT [DF_Specification_last_updated_datetime] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_Specification] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_Specification_Product] FOREIGN KEY ([product_id]) REFERENCES [dbo].[Product] ([id]),
    CONSTRAINT [FK_Specification_SpecificationType] FOREIGN KEY ([spec_type]) REFERENCES [dbo].[SpecificationType] ([id])
);

