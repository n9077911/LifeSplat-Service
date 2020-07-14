CREATE TABLE [dbo].[PAYE]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Player_Id] INT NULL, 
    [Amount] MONEY NULL, 
    CONSTRAINT [FK_User_ToTable] FOREIGN KEY ([Player_Id]) REFERENCES [Player](Id), 
)
