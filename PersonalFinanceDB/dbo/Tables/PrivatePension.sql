CREATE TABLE [dbo].[PrivatePension]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Player_Id] INT NOT NULL, 
    [Amount] MONEY NULL, 
    CONSTRAINT [FK_PrivatePension_Player] FOREIGN KEY (Player_Id) REFERENCES [Player](Id)
)
