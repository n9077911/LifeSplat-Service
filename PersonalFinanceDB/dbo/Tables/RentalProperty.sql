CREATE TABLE [dbo].[RentalProperty]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Player_Id] INT NOT NULL, 
    [Current Value] MONEY NULL, 
    [Outstanding Loan] MONEY NULL, 
    [Rental] MONEY NULL, 
    CONSTRAINT [FK_RentalProperty_Player] FOREIGN KEY ([Player_Id]) REFERENCES [Player](Id)
)
