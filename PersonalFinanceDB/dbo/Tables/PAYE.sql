CREATE TABLE [dbo].[PAYE]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [UserId] INT NULL, 
    [Amount] MONEY NULL, 
    CONSTRAINT [FK_User_ToTable] FOREIGN KEY ([UserId]) REFERENCES [User]([Id]), 
)
