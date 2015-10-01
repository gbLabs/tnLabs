-- 12.07.2014 RP: changing session StartDate and EndDate column type to DateTimeOffset
IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'Sessions' AND COLUMN_NAME = 'EndDate' AND DATA_TYPE = 'datetime')
BEGIN

	ALTER TABLE [dbo].[Sessions] ALTER COLUMN [EndDate] DATETIMEOFFSET (7) NOT NULL;

	ALTER TABLE [dbo].[Sessions] ALTER COLUMN [StartDate] DATETIMEOFFSET (7) NOT NULL;

	ALTER TABLE [dbo].[Sessions]
		ADD [Version] CHAR (36) NULL;
END
GO

IF NOT EXISTS (SELECT 1 
           FROM INFORMATION_SCHEMA.TABLES 
           WHERE TABLE_NAME='HealthChecks')
BEGIN
	CREATE TABLE [dbo].[HealthChecks] (
		[ResourceId]    UNIQUEIDENTIFIER   NOT NULL,
		[Description]   nvarchar(50)       NOT NULL,
		[LastCheck]     DATETIMEOFFSET (7) NULL,
		[CheckInterval] TIME (7)           NULL,
		[NotificationSent]   BIT           NULL,
		PRIMARY KEY ([ResourceId])
	);
END
GO