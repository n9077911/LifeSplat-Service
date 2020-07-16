CREATE PROCEDURE [dbo].[InsertUserIfNotAlreadyAdded]
	@userName nvarchar(50),
	@dob Date,
	@payeAmount money
AS
	DECLARE @dale_count INT
    SELECT @dale_count = count(*) 
    FROM Player
    WHERE Player.UserName = @userName

IF @dale_count < 1
    BEGIN

    INSERT INTO Player (UserName, DOB)
    VALUES (@userName, @dob)

    DECLARE @dale_id INT
    SELECT @dale_id = Id
    FROM Player
    WHERE Player.UserName = @userName

    INSERT INTO PAYE (Player_Id, Amount)
    VALUES (@dale_id, @payeAmount)
    END

RETURN 0
