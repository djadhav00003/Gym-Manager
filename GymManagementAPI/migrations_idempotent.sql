IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251015143853_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251015143853_InitialCreate', N'9.0.3');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251015144308_AddUserRefreshTokenTable'
)
BEGIN
    CREATE TABLE [UserRefreshTokens] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Token] nvarchar(max) NOT NULL,
        [JwtId] nvarchar(max) NULL,
        [IsRevoked] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        CONSTRAINT [PK_UserRefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserRefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251015144308_AddUserRefreshTokenTable'
)
BEGIN
    CREATE INDEX [IX_UserRefreshTokens_UserId] ON [UserRefreshTokens] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251015144308_AddUserRefreshTokenTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251015144308_AddUserRefreshTokenTable', N'9.0.3');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027150457_AddGymImageTable'
)
BEGIN
    CREATE TABLE [GymImage] (
        [Id] int NOT NULL IDENTITY,
        [GymId] int NOT NULL,
        [ImageUrl] nvarchar(max) NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_GymImage] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GymImage_Gyms_GymId] FOREIGN KEY ([GymId]) REFERENCES [Gyms] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027150457_AddGymImageTable'
)
BEGIN
    CREATE INDEX [IX_GymImage_GymId] ON [GymImage] ([GymId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251027150457_AddGymImageTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251027150457_AddGymImageTable', N'9.0.3');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251102080622_RecreateGymImagesTable'
)
BEGIN
    CREATE TABLE [GymImages] (
        [Id] int NOT NULL IDENTITY,
        [GymId] int NOT NULL,
        [ImageUrl] nvarchar(max) NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_GymImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GymImages_Gyms_GymId] FOREIGN KEY ([GymId]) REFERENCES [Gyms] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251102080622_RecreateGymImagesTable'
)
BEGIN
    CREATE INDEX [IX_GymImages_GymId] ON [GymImages] ([GymId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251102080622_RecreateGymImagesTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251102080622_RecreateGymImagesTable', N'9.0.3');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175452_AddPaymentEntities'
)
BEGIN
    DROP INDEX [IX_Payments_PlanId] ON [Payments];
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'PlanId');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @var + '];');
    EXEC(N'UPDATE [Payments] SET [PlanId] = 0 WHERE [PlanId] IS NULL');
    ALTER TABLE [Payments] ALTER COLUMN [PlanId] int NOT NULL;
    ALTER TABLE [Payments] ADD DEFAULT 0 FOR [PlanId];
    CREATE INDEX [IX_Payments_PlanId] ON [Payments] ([PlanId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175452_AddPaymentEntities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251115175452_AddPaymentEntities', N'9.0.3');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    ALTER TABLE [Payments] DROP CONSTRAINT [FK_Payments_Plans_PlanId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'PlanId');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Payments] ALTER COLUMN [PlanId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    ALTER TABLE [Payments] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    ALTER TABLE [Payments] ADD [Currency] nvarchar(10) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    ALTER TABLE [Payments] ADD [OrderId] nvarchar(200) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    ALTER TABLE [Payments] ADD [PaymentGateway] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    ALTER TABLE [Payments] ADD [PaymentStatus] nvarchar(50) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    ALTER TABLE [Payments] ADD [TransactionId] nvarchar(200) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    ALTER TABLE [Payments] ADD [UpdatedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    CREATE TABLE [PaymentGatewayOrders] (
        [Id] int NOT NULL IDENTITY,
        [PaymentId] int NULL,
        [OrderId] nvarchar(200) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Currency] nvarchar(10) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [RequestJson] nvarchar(max) NULL,
        [ResponseJson] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PaymentGatewayOrders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PaymentGatewayOrders_Payments_PaymentId] FOREIGN KEY ([PaymentId]) REFERENCES [Payments] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    CREATE TABLE [PaymentWebhookLogs] (
        [Id] int NOT NULL IDENTITY,
        [PaymentId] int NULL,
        [OrderId] nvarchar(200) NOT NULL,
        [EventType] nvarchar(100) NOT NULL,
        [Payload] nvarchar(max) NOT NULL,
        [Processed] bit NOT NULL,
        [ProcessingResult] nvarchar(max) NULL,
        [ReceivedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        CONSTRAINT [PK_PaymentWebhookLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PaymentWebhookLogs_Payments_PaymentId] FOREIGN KEY ([PaymentId]) REFERENCES [Payments] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    CREATE INDEX [IX_Payments_OrderId] ON [Payments] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    CREATE INDEX [IX_Payments_TransactionId] ON [Payments] ([TransactionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PaymentGatewayOrders_PaymentId] ON [PaymentGatewayOrders] ([PaymentId]) WHERE [PaymentId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    CREATE UNIQUE INDEX [UX_PaymentGatewayOrder_OrderId] ON [PaymentGatewayOrders] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    CREATE INDEX [IX_PaymentWebhookLog_OrderId] ON [PaymentWebhookLogs] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    CREATE INDEX [IX_PaymentWebhookLogs_PaymentId] ON [PaymentWebhookLogs] ([PaymentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    ALTER TABLE [Payments] ADD CONSTRAINT [FK_Payments_Plans_PlanId] FOREIGN KEY ([PlanId]) REFERENCES [Plans] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115175719_AddPaymentEntitiesCreation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251115175719_AddPaymentEntitiesCreation', N'9.0.3');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115183604_ChangedMembertoUser'
)
BEGIN
    ALTER TABLE [Payments] DROP CONSTRAINT [FK_Payments_Members_MemberId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115183604_ChangedMembertoUser'
)
BEGIN
    ALTER TABLE [Payments] DROP CONSTRAINT [FK_Payments_Plans_PlanId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115183604_ChangedMembertoUser'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'MemberId');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Payments] ALTER COLUMN [MemberId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115183604_ChangedMembertoUser'
)
BEGIN
    ALTER TABLE [Payments] ADD [UserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115183604_ChangedMembertoUser'
)
BEGIN
    CREATE INDEX [IX_Payments_UserId] ON [Payments] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115183604_ChangedMembertoUser'
)
BEGIN
    ALTER TABLE [Payments] ADD CONSTRAINT [FK_Payments_Members_MemberId] FOREIGN KEY ([MemberId]) REFERENCES [Members] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115183604_ChangedMembertoUser'
)
BEGIN
    ALTER TABLE [Payments] ADD CONSTRAINT [FK_Payments_Plans_PlanId] FOREIGN KEY ([PlanId]) REFERENCES [Plans] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115183604_ChangedMembertoUser'
)
BEGIN
    ALTER TABLE [Payments] ADD CONSTRAINT [FK_Payments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115183604_ChangedMembertoUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251115183604_ChangedMembertoUser', N'9.0.3');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115190137_RemoveMemberIdFromPayments'
)
BEGIN
    ALTER TABLE [Payments] DROP CONSTRAINT [FK_Payments_Members_MemberId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115190137_RemoveMemberIdFromPayments'
)
BEGIN
    DROP INDEX [IX_Payments_MemberId] ON [Payments];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115190137_RemoveMemberIdFromPayments'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'MemberId');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Payments] DROP COLUMN [MemberId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251115190137_RemoveMemberIdFromPayments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251115190137_RemoveMemberIdFromPayments', N'9.0.3');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251117181050_AddPaymentSessionIdToPaymentGatewayOrder'
)
BEGIN
    ALTER TABLE [PaymentGatewayOrders] ADD [PaymentSessionId] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251117181050_AddPaymentSessionIdToPaymentGatewayOrder'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251117181050_AddPaymentSessionIdToPaymentGatewayOrder', N'9.0.3');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251119155226_addWebhookKeyToPaymentWebhookLogs'
)
BEGIN
    ALTER TABLE [PaymentWebhookLogs] ADD [WebhookKey] nvarchar(200) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251119155226_addWebhookKeyToPaymentWebhookLogs'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_PaymentWebhookLogs_WebhookKey] ON [PaymentWebhookLogs] ([WebhookKey]) WHERE [WebhookKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251119155226_addWebhookKeyToPaymentWebhookLogs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251119155226_addWebhookKeyToPaymentWebhookLogs', N'9.0.3');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251121155358_AddSoftDeleteToGyms'
)
BEGIN
    ALTER TABLE [Gyms] ADD [DeletedAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251121155358_AddSoftDeleteToGyms'
)
BEGIN
    ALTER TABLE [Gyms] ADD [DeletedByUserId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251121155358_AddSoftDeleteToGyms'
)
BEGIN
    ALTER TABLE [Gyms] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251121155358_AddSoftDeleteToGyms'
)
BEGIN
    CREATE INDEX [IX_Gyms_DeletedByUserId] ON [Gyms] ([DeletedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251121155358_AddSoftDeleteToGyms'
)
BEGIN
    ALTER TABLE [Gyms] ADD CONSTRAINT [FK_Gyms_Users_DeletedByUserId] FOREIGN KEY ([DeletedByUserId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251121155358_AddSoftDeleteToGyms'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251121155358_AddSoftDeleteToGyms', N'9.0.3');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251122055819_AddLinkToGymUser'
)
BEGIN
    ALTER TABLE [Gyms] ADD [OwnerUserId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251122055819_AddLinkToGymUser'
)
BEGIN
    CREATE INDEX [IX_Gyms_OwnerUserId] ON [Gyms] ([OwnerUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251122055819_AddLinkToGymUser'
)
BEGIN
    ALTER TABLE [Gyms] ADD CONSTRAINT [FK_Gyms_Users_OwnerUserId] FOREIGN KEY ([OwnerUserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251122055819_AddLinkToGymUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251122055819_AddLinkToGymUser', N'9.0.3');
END;

COMMIT;
GO

