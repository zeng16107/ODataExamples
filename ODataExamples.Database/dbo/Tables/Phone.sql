CREATE TABLE [dbo].[Phone] (
    [id]                    INT           IDENTITY (1, 1) NOT NULL,
    [phone_type]            INT           NOT NULL,
    [phone_number]          NVARCHAR (15) NULL,
    [inserted_by]           NVARCHAR (50) NOT NULL,
    [inserted_datetime]     DATETIME2 (7) CONSTRAINT [DF_Phone_inserted_datetime] DEFAULT (getutcdate()) NOT NULL,
    [last_updated_by]       NVARCHAR (50) NOT NULL,
    [last_updated_datetime] DATETIME2 (7) CONSTRAINT [DF_Phone_last_updated_datetime] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_Phone] PRIMARY KEY CLUSTERED ([id] ASC)
);

