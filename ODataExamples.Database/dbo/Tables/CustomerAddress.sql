CREATE TABLE [dbo].[CustomerAddress] (
    [customer_id] INT NOT NULL,
    [address_id]  INT NOT NULL,
    CONSTRAINT [PK_CustomerAddress] PRIMARY KEY CLUSTERED ([customer_id] ASC, [address_id] ASC),
    CONSTRAINT [FK_CustomerAddress_Address] FOREIGN KEY ([address_id]) REFERENCES [dbo].[Address] ([id]),
    CONSTRAINT [FK_CustomerAddress_Customer] FOREIGN KEY ([customer_id]) REFERENCES [dbo].[Customer] ([id])
);

