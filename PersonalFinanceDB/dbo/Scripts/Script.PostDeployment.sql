/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
EXEC InsertUserIfnotAlreadyAdded @userName = 'dale', @dob = '19810530', @payeAmount = 80000;
EXEC InsertUserIfnotAlreadyAdded @userName = 'sylwia', @dob = '19810608', @payeAmount = 20000;
