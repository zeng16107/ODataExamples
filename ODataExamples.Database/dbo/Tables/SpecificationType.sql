CREATE TABLE [dbo].[SpecificationType] (
    [id]                    INT           IDENTITY (1, 1) NOT NULL,
    [description]           NVARCHAR (50) NOT NULL,
    [inserted_by]           NVARCHAR (50) NOT NULL,
    [inserted_datetime]     DATETIME2 (7) CONSTRAINT [DF_SpecificationType_inserted_datetime] DEFAULT (getutcdate()) NOT NULL,
    [last_updated_by]       NVARCHAR (50) NOT NULL,
    [last_updated_datetime] DATETIME2 (7) CONSTRAINT [DF_SpecificationType_last_updated_datetime] DEFAULT (getutcdate()) NOT NULL,
    CONSTRAINT [PK_SpecificationType] PRIMARY KEY CLUSTERED ([id] ASC)
);

