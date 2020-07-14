CREATE TABLE [dbo].[Player]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [UserName] NVARCHAR(50) NULL, 
    [DOB] DATE NULL,
    [CreatedDate] DATETIME2 NULL DEFAULT getutcdate(), 
)
