namespace GB.tnLabs.Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class first : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Identities",
                c => new
                    {
                        IdentityId = c.Int(nullable: false, identity: true),
                        NameIdentifier = c.String(),
                        IdentityProvider = c.String(),
                        FirstName = c.String(),
                        LastName = c.String(),
                        DisplayName = c.String(),
                        Email = c.String(),
                    })
                .PrimaryKey(t => t.IdentityId);
            
            CreateTable(
                "dbo.SubscriptionIdentityRoles",
                c => new
                    {
                        SubscriptionIdentityRoleId = c.Int(nullable: false, identity: true),
                        SubscriptionId = c.Int(nullable: false),
                        IdentityId = c.Int(nullable: false),
                        Role = c.String(),
                    })
                .PrimaryKey(t => t.SubscriptionIdentityRoleId)
                .ForeignKey("dbo.Identities", t => t.IdentityId, cascadeDelete: true)
                .ForeignKey("dbo.Subscriptions", t => t.SubscriptionId, cascadeDelete: true)
                .Index(t => t.SubscriptionId)
                .Index(t => t.IdentityId);
            
            CreateTable(
                "dbo.Subscriptions",
                c => new
                    {
                        SubscriptionId = c.Int(nullable: false, identity: true),
                        AzureSubscriptionId = c.String(),
                        CertificateKey = c.String(),
                        BlobStorageName = c.String(),
                        BlobStorageKey = c.String(),
                        Certificate = c.Binary(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.SubscriptionId);
            
            CreateTable(
                "dbo.Labs",
                c => new
                    {
                        LabId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ImageName = c.String(),
                        Description = c.String(),
                        SubscriptionId = c.Int(nullable: false),
                        CreationDate = c.DateTime(nullable: false),
                        Removed = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.LabId)
                .ForeignKey("dbo.Subscriptions", t => t.SubscriptionId, cascadeDelete: true)
                .Index(t => t.SubscriptionId);
            
            CreateTable(
                "dbo.Sessions",
                c => new
                    {
                        SessionId = c.Int(nullable: false, identity: true),
                        SessionName = c.String(),
                        LabId = c.Int(nullable: false),
                        StartDate = c.DateTimeOffset(nullable: false, precision: 7),
                        EndDate = c.DateTimeOffset(nullable: false, precision: 7),
                        StartInterval = c.Time(precision: 7),
                        EndInterval = c.Time(precision: 7),
                        SchedulingType = c.Int(nullable: false),
                        VmSize = c.String(),
                        SubscriptionId = c.Int(nullable: false),
                        Removed = c.Boolean(nullable: false),
                        Version = c.String(),
                    })
                .PrimaryKey(t => t.SessionId)
                .ForeignKey("dbo.Labs", t => t.LabId, cascadeDelete: true)
                .Index(t => t.LabId);
            
            CreateTable(
                "dbo.SessionUsers",
                c => new
                    {
                        SessionUserId = c.Int(nullable: false, identity: true),
                        SessionId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.SessionUserId)
                .ForeignKey("dbo.Sessions", t => t.SessionId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.SessionId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        Email = c.String(),
                        UserName = c.String(),
                        Password = c.String(),
                        Removed = c.Boolean(nullable: false),
                        FirstName = c.String(),
                        LastName = c.String(),
                        SubscriptionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.Subscriptions", t => t.SubscriptionId, cascadeDelete: false)
                .Index(t => t.SubscriptionId);
            
            CreateTable(
                "dbo.VirtualMachines",
                c => new
                    {
                        VirtualMachineId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        SessionId = c.Int(nullable: false),
                        VmName = c.String(),
                        VmRdpPort = c.Int(nullable: false),
                        VmAdminUser = c.String(),
                        VmAdminPass = c.String(),
                        Deleted = c.Boolean(nullable: false),
                        Stopped1 = c.Int(),
                        Stopped = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.VirtualMachineId)
                .ForeignKey("dbo.Sessions", t => t.SessionId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.SessionId);
            
            CreateTable(
                "dbo.TemplateVMs",
                c => new
                    {
                        TemplateVMId = c.Int(nullable: false, identity: true),
                        ServiceName = c.String(),
                        VMName = c.String(),
                        SubscriptionId = c.Int(nullable: false),
                        CreatorId = c.Int(nullable: false),
                        State = c.Int(nullable: false),
                        VMAdminUser = c.String(),
                        VMAdminPass = c.String(),
                        Key = c.Guid(nullable: false),
                        VMLabel = c.String(),
                        Description = c.String(),
                        VmRdpPort = c.Int(nullable: false),
                        ImageName = c.String(),
                        SourceImageName = c.String(),
                        StateChangedTimestamp = c.DateTimeOffset(nullable: false, precision: 7),
                        Identity_IdentityId = c.Int(),
                    })
                .PrimaryKey(t => t.TemplateVMId)
                .ForeignKey("dbo.Identities", t => t.Identity_IdentityId)
                .ForeignKey("dbo.Subscriptions", t => t.SubscriptionId, cascadeDelete: true)
                .Index(t => t.SubscriptionId)
                .Index(t => t.Identity_IdentityId);
            
            CreateTable(
                "dbo.RecommendedVMImages",
                c => new
                    {
                        RecommendedVMImageId = c.Int(nullable: false, identity: true),
                        ImageFamily = c.String(),
                        OSFamily = c.String(),
                    })
                .PrimaryKey(t => t.RecommendedVMImageId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.TemplateVMs", "SubscriptionId", "dbo.Subscriptions");
            DropForeignKey("dbo.TemplateVMs", "Identity_IdentityId", "dbo.Identities");
            DropForeignKey("dbo.SubscriptionIdentityRoles", "SubscriptionId", "dbo.Subscriptions");
            DropForeignKey("dbo.Labs", "SubscriptionId", "dbo.Subscriptions");
            DropForeignKey("dbo.VirtualMachines", "UserId", "dbo.Users");
            DropForeignKey("dbo.VirtualMachines", "SessionId", "dbo.Sessions");
            DropForeignKey("dbo.Users", "SubscriptionId", "dbo.Subscriptions");
            DropForeignKey("dbo.SessionUsers", "UserId", "dbo.Users");
            DropForeignKey("dbo.SessionUsers", "SessionId", "dbo.Sessions");
            DropForeignKey("dbo.Sessions", "LabId", "dbo.Labs");
            DropForeignKey("dbo.SubscriptionIdentityRoles", "IdentityId", "dbo.Identities");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.TemplateVMs", new[] { "Identity_IdentityId" });
            DropIndex("dbo.TemplateVMs", new[] { "SubscriptionId" });
            DropIndex("dbo.VirtualMachines", new[] { "SessionId" });
            DropIndex("dbo.VirtualMachines", new[] { "UserId" });
            DropIndex("dbo.Users", new[] { "SubscriptionId" });
            DropIndex("dbo.SessionUsers", new[] { "UserId" });
            DropIndex("dbo.SessionUsers", new[] { "SessionId" });
            DropIndex("dbo.Sessions", new[] { "LabId" });
            DropIndex("dbo.Labs", new[] { "SubscriptionId" });
            DropIndex("dbo.SubscriptionIdentityRoles", new[] { "IdentityId" });
            DropIndex("dbo.SubscriptionIdentityRoles", new[] { "SubscriptionId" });
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.RecommendedVMImages");
            DropTable("dbo.TemplateVMs");
            DropTable("dbo.VirtualMachines");
            DropTable("dbo.Users");
            DropTable("dbo.SessionUsers");
            DropTable("dbo.Sessions");
            DropTable("dbo.Labs");
            DropTable("dbo.Subscriptions");
            DropTable("dbo.SubscriptionIdentityRoles");
            DropTable("dbo.Identities");
        }
    }
}
