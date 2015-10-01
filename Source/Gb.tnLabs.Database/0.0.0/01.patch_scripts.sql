GO
IF EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_FK_Lab_0' AND object_id = OBJECT_ID('Labs'))
BEGIN
	DROP INDEX [IX_FK_Lab_0] ON [dbo].[Labs]
END
GO
IF EXISTS(SELECT *FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
    WHERE CONSTRAINT_NAME ='FK_Lab_0')
BEGIN
	ALTER TABLE [dbo].[Labs] DROP CONSTRAINT [FK_Lab_0]
END
GO

IF EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'OwnerId' AND [object_id] = OBJECT_ID(N'Labs'))
BEGIN
    ALTER TABLE [dbo].Labs DROP COLUMN OwnerId
END

GO

-- 01.05.2014 RP: adding RecommendedVMImages table

IF NOT EXISTS (SELECT 1 
           FROM INFORMATION_SCHEMA.TABLES 
           WHERE TABLE_NAME='RecommendedVMImages')
BEGIN
	CREATE TABLE [dbo].[RecommendedVMImages] (
		[RecommendedVMImageId] int IDENTITY(1,1) NOT NULL,
		[ImageFamily] nvarchar(max)  NOT NULL,
		[OSFamily] nvarchar(max)  NOT NULL
	);

	-- Creating primary key on [RecommendedVMImageId] in table 'RecommendedVMImages'
	ALTER TABLE [dbo].[RecommendedVMImages]
	ADD CONSTRAINT [PK_RecommendedVMImages]
		PRIMARY KEY CLUSTERED ([RecommendedVMImageId] ASC);

END

GO

IF NOT EXISTS (SELECT 1
			FROM [RecommendedVMImages])
BEGIN
	INSERT INTO [dbo].[RecommendedVMImages] ([ImageFamily], [OSFamily]) VALUES ('Windows Server 2012 R2 Datacenter', 'Windows');
	INSERT INTO [dbo].[RecommendedVMImages] ([ImageFamily], [OSFamily]) VALUES ('Windows Server 2008 R2 SP1', 'Windows');
END

-- 04.05.2014 RP: adding the TemplateVM table

IF NOT EXISTS (SELECT 1
           FROM INFORMATION_SCHEMA.TABLES 
           WHERE TABLE_NAME='TemplateVMs')
BEGIN
	CREATE TABLE [dbo].[TemplateVMs] (
		[TemplateVMId] int IDENTITY(1,1) NOT NULL,
		[ServiceName] varchar(max)  NOT NULL,
		[VMName] varchar(max)  NOT NULL,
		[SubscriptionId] int  NOT NULL,
		[CreatorId] int  NOT NULL,
		[State] int  NOT NULL DEFAULT 0,
		[VMAdminUser] nvarchar(max)  NOT NULL,
		[VMAdminPass] nvarchar(max)  NOT NULL,
		[Key] UNIQUEIDENTIFIER NOT NULL
	);

	-- Creating primary key on [TemplateVMId] in table 'TemplateVMs'
	ALTER TABLE [dbo].[TemplateVMs]
	ADD CONSTRAINT [PK_TemplateVMs]
		PRIMARY KEY CLUSTERED ([TemplateVMId] ASC);

	-- Creating foreign key on [SessionUsers] in table 'SessionUsers'
	ALTER TABLE [dbo].[TemplateVMs]
	ADD CONSTRAINT [FK_TemplateVMs_SubscriptionId]
		FOREIGN KEY ([SubscriptionId])
		REFERENCES [dbo].[Subscriptions]
			([SubscriptionId])
		ON DELETE NO ACTION ON UPDATE NO ACTION;
	
	-- Creating foreign key on [SessionUsers] in table 'SessionUsers'
	ALTER TABLE [dbo].[TemplateVMs]
	ADD CONSTRAINT [FK_TemplateVMs_CreatorId]
		FOREIGN KEY ([CreatorId])
		REFERENCES [dbo].[Identities]
			([IdentityId])
		ON DELETE NO ACTION ON UPDATE NO ACTION;
END
GO

-- ?? Radu: adding new fields in TemplateVMs

IF NOT EXISTS (SELECT * FROM sys.columns 
           WHERE [name] = N'VMLabel' AND [object_id] = OBJECT_ID(N'TemplateVMs'))
BEGIN
	DELETE FROM [dbo].[TemplateVMs];

	ALTER TABLE [dbo].[TemplateVMs]
	ADD [VMLabel] varchar(max)  NOT NULL;

	ALTER TABLE [dbo].[TemplateVMs]
	ADD [Description] varchar(max)  NOT NULL;
END
GO

-- 03.06.2014 RC: adding the Remove and CreationDate columns to Labs
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'CreationDate' AND [object_id] = OBJECT_ID(N'Labs'))
BEGIN

ALTER TABLE Labs
ADD CreationDate DateTime NOT NULL 
CONSTRAINT CreationDateDefaultValue DEFAULT GETDATE()

END

GO

IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'Removed' AND [object_id] = OBJECT_ID(N'Labs'))
BEGIN

ALTER TABLE Labs
ADD Removed Bit NOT NULL 
CONSTRAINT RemovedDefaultValue DEFAULT 0

END

GO

-- 7.06.2014 Radu: adding new fields in TemplateVMs

IF NOT EXISTS (SELECT * FROM sys.columns 
           WHERE [name] = N'VmRdpPort' AND [object_id] = OBJECT_ID(N'TemplateVMs'))
BEGIN
	DELETE FROM [dbo].[TemplateVMs];

	ALTER TABLE [dbo].[TemplateVMs]
	ADD [VmRdpPort] int NOT NULL;
END
GO

-- 7.06.2014 Radu: adding new fields in TemplateVMs
IF NOT EXISTS (SELECT * FROM sys.columns 
           WHERE [name] = N'ImageName' AND [object_id] = OBJECT_ID(N'TemplateVMs'))
BEGIN
	DELETE FROM [dbo].[TemplateVMs];

	ALTER TABLE [dbo].[TemplateVMs]
		ADD [ImageName] varchar(max);
	
	ALTER TABLE [dbo].[TemplateVMs]
		ADD [SourceImageName] varchar(max) NOT NULL;

	ALTER TABLE [dbo].[TemplateVMs]
		ADD [StateChangedTimestamp] datetimeoffset NOT NULL;
END
GO

-- 14.06.2014 RC: adding VmSize column, remove OwnerId from Session and add SubscriptionId

IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'VmSize' AND [object_id] = OBJECT_ID(N'Sessions'))
BEGIN

ALTER TABLE [Sessions]
ADD VmSize nvarchar(max) NOT NULL 
CONSTRAINT VmSizeTypeDefaultValue DEFAULT 'Small'

END

GO
IF EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_Session_1')
   AND parent_object_id = OBJECT_ID(N'dbo.Sessions')
)
BEGIN
	ALTER TABLE [dbo].[Sessions] DROP CONSTRAINT [FK_Session_1]
END
GO
IF EXISTS(SELECT * 
FROM sys.indexes 
WHERE name='IX_FK_Session_1' AND object_id = OBJECT_ID('Sessions'))
BEGIN
	DROP INDEX [IX_FK_Session_1] ON [dbo].[Sessions]
END

GO

IF EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'OwnerId' AND [object_id] = OBJECT_ID(N'Sessions'))
BEGIN
    ALTER TABLE [dbo].[Sessions] DROP COLUMN OwnerId
END
GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'SubscriptionId' AND [object_id] = OBJECT_ID(N'Sessions'))
BEGIN

ALTER TABLE [Sessions]
ADD SubscriptionId int NOT NULL 
CONSTRAINT SubscriptionTypeDefaultValue DEFAULT 1

END
GO

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS 
        WHERE [NAME] = N'FIRSTNAME' AND [OBJECT_ID] = OBJECT_ID(N'USERS'))
		BEGIN
ALTER TABLE USERS
ADD FIRSTNAME VARCHAR(50)
END
GO
IF NOT EXISTS(SELECT * FROM SYS.COLUMNS 
        WHERE [NAME] = N'LASTNAME' AND [OBJECT_ID] = OBJECT_ID(N'USERS'))
BEGIN
ALTER TABLE USERS
ADD LASTNAME VARCHAR(50)
END
GO

-- 17.06.2014 RC: adding Removed column on Sessions, and Users
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'Removed' AND [object_id] = OBJECT_ID(N'Sessions'))
BEGIN

ALTER TABLE Sessions
ADD Removed Bit NOT NULL 
CONSTRAINT SessionsRemovedDefaultValue DEFAULT 0

END

GO
IF NOT EXISTS(SELECT * FROM sys.columns 
        WHERE [name] = N'Removed' AND [object_id] = OBJECT_ID(N'Users'))
BEGIN

ALTER TABLE Users
ADD Removed Bit NOT NULL 
CONSTRAINT UsersRemovedDefaultValue DEFAULT 0

END

GO

