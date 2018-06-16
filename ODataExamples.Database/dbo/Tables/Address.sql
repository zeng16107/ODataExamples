CREATE TABLE [dbo].[Address] (
    [id]                    INT           IDENTITY (1, 1) NOT NULL,
    [address_type]          INT           NOT NULL,
    [street_address_1]      NVARCHAR (50) NOT NULL,
    [street_address_2]      NVARCHAR (50) NULL,
    [city]                  NVARCHAR (50) NULL,
    [statecode]             CHAR (2)      NOT NULL,
    [zipcode]               NVARCHAR (10) NOT NULL,
    [inserted_by]           NVARCHAR (50) NOT NULL,
    [inserted_datetime]     DATETIME2 (7) CONSTRAINT [DF_Address_inserted_datetime] DEFAULT (getutcdate()) NOT NULL,
    [last_updated_by]       NVARCHAR (50) NOT NULL,
    [last_updated_datetime] DATETIME2 (7) CONSTRAINT [DF_Address_last_updated_datetime] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_Address] PRIMARY KEY CLUSTERED ([id] ASC)
);

