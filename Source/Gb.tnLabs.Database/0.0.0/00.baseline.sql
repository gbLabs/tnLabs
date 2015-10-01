--check for an important table, to avoid errors on trying to recreate the db when it's already in place
IF OBJECT_ID(N'[dbo].[Identities]', 'U') IS NULL
BEGIN
	-- Creating table 'SessionUsers'
	CREATE TABLE [dbo].[SessionUsers] (
		[SessionUserId] int IDENTITY(1,1) NOT NULL,
		[SessionId] int  NOT NULL,
		[UserId] int  NOT NULL
	);

	-- Creating table 'Users'
	CREATE TABLE [dbo].[Users] (
		[UserId] int IDENTITY(1,1) NOT NULL,
		[Email] nvarchar(max)  NOT NULL,
		[UserName] nvarchar(max)  NOT NULL,
		[Password] nvarchar(max)  NOT NULL,
		[Removed] bit  NOT NULL,
		[FirstName] varchar(50)  NULL,
		[LastName] varchar(50)  NULL,
		[SubscriptionId] int  NOT NULL
	);

	-- Creating table 'VirtualMachines'
	CREATE TABLE [dbo].[VirtualMachines] (
		[VirtualMachineId] int IDENTITY(1,1) NOT NULL,
		[UserId] int  NOT NULL,
		[SessionId] int  NOT NULL,
		[VmName] varchar(50)  NOT NULL,
		[VmRdpPort] int  NOT NULL,
		[VmAdminUser] varchar(50)  NOT NULL,
		[VmAdminPass] varchar(50)  NOT NULL,
		[Deleted] bit  NOT NULL,
		[Stopped1] int  NULL,
		[Stopped] bit  NOT NULL
	);

	-- Creating table 'Identities'
	CREATE TABLE [dbo].[Identities] (
		[IdentityId] int IDENTITY(1,1) NOT NULL,
		[NameIdentifier] nvarchar(max)  NOT NULL,
		[IdentityProvider] nvarchar(max)  NOT NULL,
		[FirstName] nvarchar(max)  NOT NULL,
		[LastName] nvarchar(max)  NOT NULL,
		[DisplayName] nvarchar(max)  NOT NULL,
		[Email] nvarchar(max)  NOT NULL
	);

	-- Creating table 'SubscriptionIdentityRoles'
	CREATE TABLE [dbo].[SubscriptionIdentityRoles] (
		[SubscriptionId] int  NOT NULL,
		[IdentityId] int  NOT NULL,
		[Role] nvarchar(50)  NOT NULL
	);

	-- Creating table 'Subscriptions'
	CREATE TABLE [dbo].[Subscriptions] (
		[SubscriptionId] int IDENTITY(1,1) NOT NULL,
		[AzureSubscriptionId] nvarchar(max)  NOT NULL,
		[CertificateKey] nvarchar(max)  NOT NULL,
		[BlobStorageName] nvarchar(max)  NOT NULL,
		[BlobStorageKey] nvarchar(max)  NOT NULL,
		[Certificate] varbinary(max)  NOT NULL,
		[Name] nvarchar(16)  NOT NULL
	);

	-- Creating table 'Labs'
	CREATE TABLE [dbo].[Labs] (
		[LabId] int IDENTITY(1,1) NOT NULL,
		[Name] nvarchar(max)  NOT NULL,
		[ImageName] nvarchar(max)  NOT NULL,
		[Description] nvarchar(max)  NULL,
		[SubscriptionId] int  NOT NULL,
		[CreationDate] datetime  NOT NULL,
		[Removed] bit  NOT NULL
	);

	-- Creating table 'RecommendedVMImages'
	CREATE TABLE [dbo].[RecommendedVMImages] (
		[RecommendedVMImageId] int IDENTITY(1,1) NOT NULL,
		[ImageFamily] nvarchar(max)  NOT NULL,
		[OSFamily] nvarchar(max)  NOT NULL
	);

	-- Creating table 'TemplateVMs'
	CREATE TABLE [dbo].[TemplateVMs] (
		[TemplateVMId] int IDENTITY(1,1) NOT NULL,
		[ServiceName] varchar(max)  NOT NULL,
		[VMName] varchar(max)  NOT NULL,
		[SubscriptionId] int  NOT NULL,
		[CreatorId] int  NOT NULL,
		[State] int  NOT NULL,
		[VMAdminUser] nvarchar(max)  NOT NULL,
		[VMAdminPass] nvarchar(max)  NOT NULL,
		[Key] uniqueidentifier  NOT NULL,
		[VMLabel] varchar(max)  NOT NULL,
		[Description] varchar(max)  NOT NULL,
		[VmRdpPort] int  NOT NULL,
		[ImageName] varchar(max)  NULL,
		[SourceImageName] varchar(max)  NOT NULL,
		[StateChangedTimestamp] datetimeoffset  NOT NULL
	);

	-- Creating table 'Sessions'
	CREATE TABLE [dbo].[Sessions] (
		[SessionId] int IDENTITY(1,1) NOT NULL,
		[SessionName] nvarchar(max)  NOT NULL,
		[LabId] int  NOT NULL,
		[StartDate] datetimeoffset  NOT NULL,
		[EndDate] datetimeoffset  NOT NULL,
		[StartInterval] time  NULL,
		[EndInterval] time  NULL,
		[SchedulingType] int  NOT NULL,
		[VmSize] nvarchar(max)  NOT NULL,
		[SubscriptionId] int  NOT NULL,
		[Removed] bit  NOT NULL,
		[Version] char(36)  NULL
	);

	-- --------------------------------------------------
	-- Creating all PRIMARY KEY constraints
	-- --------------------------------------------------

	-- Creating primary key on [SessionUserId] in table 'SessionUsers'
	ALTER TABLE [dbo].[SessionUsers]
	ADD CONSTRAINT [PK_SessionUsers]
		PRIMARY KEY CLUSTERED ([SessionUserId] ASC);

	-- Creating primary key on [UserId] in table 'Users'
	ALTER TABLE [dbo].[Users]
	ADD CONSTRAINT [PK_Users]
		PRIMARY KEY CLUSTERED ([UserId] ASC);

	-- Creating primary key on [VirtualMachineId] in table 'VirtualMachines'
	ALTER TABLE [dbo].[VirtualMachines]
	ADD CONSTRAINT [PK_VirtualMachines]
		PRIMARY KEY CLUSTERED ([VirtualMachineId] ASC);

	-- Creating primary key on [IdentityId] in table 'Identities'
	ALTER TABLE [dbo].[Identities]
	ADD CONSTRAINT [PK_Identities]
		PRIMARY KEY CLUSTERED ([IdentityId] ASC);

	-- Creating primary key on [SubscriptionId], [IdentityId], [Role] in table 'SubscriptionIdentityRoles'
	ALTER TABLE [dbo].[SubscriptionIdentityRoles]
	ADD CONSTRAINT [PK_SubscriptionIdentityRoles]
		PRIMARY KEY CLUSTERED ([SubscriptionId], [IdentityId], [Role] ASC);

	-- Creating primary key on [SubscriptionId] in table 'Subscriptions'
	ALTER TABLE [dbo].[Subscriptions]
	ADD CONSTRAINT [PK_Subscriptions]
		PRIMARY KEY CLUSTERED ([SubscriptionId] ASC);

	-- Creating primary key on [LabId] in table 'Labs'
	ALTER TABLE [dbo].[Labs]
	ADD CONSTRAINT [PK_Labs]
		PRIMARY KEY CLUSTERED ([LabId] ASC);

	-- Creating primary key on [RecommendedVMImageId] in table 'RecommendedVMImages'
	ALTER TABLE [dbo].[RecommendedVMImages]
	ADD CONSTRAINT [PK_RecommendedVMImages]
		PRIMARY KEY CLUSTERED ([RecommendedVMImageId] ASC);

	-- Creating primary key on [TemplateVMId] in table 'TemplateVMs'
	ALTER TABLE [dbo].[TemplateVMs]
	ADD CONSTRAINT [PK_TemplateVMs]
		PRIMARY KEY CLUSTERED ([TemplateVMId] ASC);

	-- Creating primary key on [SessionId] in table 'Sessions'
	ALTER TABLE [dbo].[Sessions]
	ADD CONSTRAINT [PK_Sessions]
		PRIMARY KEY CLUSTERED ([SessionId] ASC);

	-- --------------------------------------------------
	-- Creating all FOREIGN KEY constraints
	-- --------------------------------------------------

	-- Creating foreign key on [UserId] in table 'SessionUsers'
	ALTER TABLE [dbo].[SessionUsers]
	ADD CONSTRAINT [FK_SessionUser_0]
		FOREIGN KEY ([UserId])
		REFERENCES [dbo].[Users]
			([UserId])
		ON DELETE NO ACTION ON UPDATE NO ACTION;

	-- Creating non-clustered index for FOREIGN KEY 'FK_SessionUser_0'
	CREATE INDEX [IX_FK_SessionUser_0]
	ON [dbo].[SessionUsers]
		([UserId]);

	-- Creating foreign key on [UserId] in table 'VirtualMachines'
	ALTER TABLE [dbo].[VirtualMachines]
	ADD CONSTRAINT [FK_VirtualMachines_0]
		FOREIGN KEY ([UserId])
		REFERENCES [dbo].[Users]
			([UserId])
		ON DELETE NO ACTION ON UPDATE NO ACTION;

	-- Creating non-clustered index for FOREIGN KEY 'FK_VirtualMachines_0'
	CREATE INDEX [IX_FK_VirtualMachines_0]
	ON [dbo].[VirtualMachines]
		([UserId]);

	-- Creating foreign key on [IdentityId] in table 'SubscriptionIdentityRoles'
	ALTER TABLE [dbo].[SubscriptionIdentityRoles]
	ADD CONSTRAINT [FK_SubscriptionIdentityRoles_1]
		FOREIGN KEY ([IdentityId])
		REFERENCES [dbo].[Identities]
			([IdentityId])
		ON DELETE NO ACTION ON UPDATE NO ACTION;

	-- Creating non-clustered index for FOREIGN KEY 'FK_SubscriptionIdentityRoles_1'
	CREATE INDEX [IX_FK_SubscriptionIdentityRoles_1]
	ON [dbo].[SubscriptionIdentityRoles]
		([IdentityId]);

	-- Creating foreign key on [SubscriptionId] in table 'SubscriptionIdentityRoles'
	ALTER TABLE [dbo].[SubscriptionIdentityRoles]
	ADD CONSTRAINT [FK_SubscriptionIdentityRoles_0]
		FOREIGN KEY ([SubscriptionId])
		REFERENCES [dbo].[Subscriptions]
			([SubscriptionId])
		ON DELETE NO ACTION ON UPDATE NO ACTION;

	-- Creating foreign key on [SubscriptionId] in table 'Labs'
	ALTER TABLE [dbo].[Labs]
	ADD CONSTRAINT [FK_Labs_0]
		FOREIGN KEY ([SubscriptionId])
		REFERENCES [dbo].[Subscriptions]
			([SubscriptionId])
		ON DELETE NO ACTION ON UPDATE NO ACTION;

	-- Creating non-clustered index for FOREIGN KEY 'FK_Labs_0'
	CREATE INDEX [IX_FK_Labs_0]
	ON [dbo].[Labs]
		([SubscriptionId]);

	-- Creating foreign key on [CreatorId] in table 'TemplateVMs'
	ALTER TABLE [dbo].[TemplateVMs]
	ADD CONSTRAINT [FK_TemplateVMs_CreatorId]
		FOREIGN KEY ([CreatorId])
		REFERENCES [dbo].[Identities]
			([IdentityId])
		ON DELETE NO ACTION ON UPDATE NO ACTION;

	-- Creating non-clustered index for FOREIGN KEY 'FK_TemplateVMs_CreatorId'
	CREATE INDEX [IX_FK_TemplateVMs_CreatorId]
	ON [dbo].[TemplateVMs]
		([CreatorId]);

	-- Creating foreign key on [SubscriptionId] in table 'TemplateVMs'
	ALTER TABLE [dbo].[TemplateVMs]
	ADD CONSTRAINT [FK_TemplateVMs_SubscriptionId]
		FOREIGN KEY ([SubscriptionId])
		REFERENCES [dbo].[Subscriptions]
			([SubscriptionId])
		ON DELETE NO ACTION ON UPDATE NO ACTION;

	-- Creating non-clustered index for FOREIGN KEY 'FK_TemplateVMs_SubscriptionId'
	CREATE INDEX [IX_FK_TemplateVMs_SubscriptionId]
	ON [dbo].[TemplateVMs]
		([SubscriptionId]);

	-- Creating foreign key on [SubscriptionId] in table 'Users'
	ALTER TABLE [dbo].[Users]
	ADD CONSTRAINT [FK_Users_Subscriptions]
		FOREIGN KEY ([SubscriptionId])
		REFERENCES [dbo].[Subscriptions]
			([SubscriptionId])
		ON DELETE NO ACTION ON UPDATE NO ACTION;

	-- Creating non-clustered index for FOREIGN KEY 'FK_Users_Subscriptions'
	CREATE INDEX [IX_FK_Users_Subscriptions]
	ON [dbo].[Users]
		([SubscriptionId]);

	-- Creating foreign key on [LabId] in table 'Sessions'
	ALTER TABLE [dbo].[Sessions]
	ADD CONSTRAINT [FK_Session_0]
		FOREIGN KEY ([LabId])
		REFERENCES [dbo].[Labs]
			([LabId])
		ON DELETE NO ACTION ON UPDATE NO ACTION;

	-- Creating non-clustered index for FOREIGN KEY 'FK_Session_0'
	CREATE INDEX [IX_FK_Session_0]
	ON [dbo].[Sessions]
		([LabId]);

	-- Creating foreign key on [SessionId] in table 'SessionUsers'
	ALTER TABLE [dbo].[SessionUsers]
	ADD CONSTRAINT [FK_SessionUser_1]
		FOREIGN KEY ([SessionId])
		REFERENCES [dbo].[Sessions]
			([SessionId])
		ON DELETE NO ACTION ON UPDATE NO ACTION;

	-- Creating non-clustered index for FOREIGN KEY 'FK_SessionUser_1'
	CREATE INDEX [IX_FK_SessionUser_1]
	ON [dbo].[SessionUsers]
		([SessionId]);

	-- Creating foreign key on [SessionId] in table 'VirtualMachines'
	ALTER TABLE [dbo].[VirtualMachines]
	ADD CONSTRAINT [FK_VirtualMachines_1]
		FOREIGN KEY ([SessionId])
		REFERENCES [dbo].[Sessions]
			([SessionId])
		ON DELETE NO ACTION ON UPDATE NO ACTION;

	-- Creating non-clustered index for FOREIGN KEY 'FK_VirtualMachines_1'
	CREATE INDEX [IX_FK_VirtualMachines_1]
	ON [dbo].[VirtualMachines]
		([SessionId]);
END
GO