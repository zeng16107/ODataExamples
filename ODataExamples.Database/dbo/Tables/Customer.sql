CREATE TABLE [dbo].[Customer] (
    [id]                    INT           IDENTITY (1, 1) NOT NULL,
    [first_name]            NVARCHAR (50) NOT NULL,
    [last_name]             NVARCHAR (50) NOT NULL,
    [suffix]                NVARCHAR (5)  NULL,
    [email_address]         NVARCHAR (50) NOT NULL,
    [inserted_by]           NVARCHAR (50) NOT NULL,
    [inserted_datetime]     DATETIME2 (7) CONSTRAINT [DF_Customer_inserted_datetime] DEFAULT (getutcdate()) NOT NULL,
    [last_updated_by]       NVARCHAR (50) NOT NULL,
    [last_updated_datetime] DATETIME2 (7) CONSTRAINT [DF_Customer_last_updated_datetime] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED ([id] ASC)
);

