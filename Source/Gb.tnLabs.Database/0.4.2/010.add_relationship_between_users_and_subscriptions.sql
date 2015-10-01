-- 29.06.2014 RP: adding a relation between users (participants) and subscriptions
IF NOT EXISTS(SELECT * FROM SYS.COLUMNS 
        WHERE [NAME] = N'SubscriptionId' AND [OBJECT_ID] = OBJECT_ID(N'USERS'))
BEGIN
	ALTER TABLE [Users]
	ADD SubscriptionId int;

	UPDATE [Users]
	SET SubscriptionId = (select min(sub.subscriptionId) from subscriptions sub);

	ALTER TABLE [Users] ALTER COLUMN [SubscriptionId] INT NOT NULL;

	ALTER TABLE [Users] WITH NOCHECK
		ADD CONSTRAINT [FK_Users_Subscriptions] FOREIGN KEY ([SubscriptionId]) REFERENCES [Subscriptions] ([SubscriptionId]);
END
GO
