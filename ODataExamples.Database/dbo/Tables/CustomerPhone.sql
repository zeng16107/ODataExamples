CREATE TABLE [dbo].[CustomerPhone] (
    [customer_id] INT NOT NULL,
    [phone_id]    INT NOT NULL,
    CONSTRAINT [PK_CustomerPhone] PRIMARY KEY CLUSTERED ([customer_id] ASC, [phone_id] ASC),
    CONSTRAINT [FK_CustomerPhone_Customer] FOREIGN KEY ([customer_id]) REFERENCES [dbo].[Customer] ([id]),
    CONSTRAINT [FK_CustomerPhone_Phone] FOREIGN KEY ([phone_id]) REFERENCES [dbo].[Phone] ([id])
);

