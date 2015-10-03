namespace GB.tnLabs.Core.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<GB.tnLabs.Core.Repository.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(GB.tnLabs.Core.Repository.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            context.RecommendedVMImages.AddOrUpdate(
                    x => x.RecommendedVMImageId,
                    new Repository.RecommendedVMImage { ImageFamily = "Windows Server 2012 R2 Datacenter",  OSFamily = "Windows", RecommendedVMImageId = 1},
                    new Repository.RecommendedVMImage { ImageFamily = "Windows Server 2008 R2 SP1", OSFamily = "Windows", RecommendedVMImageId = 2 }
                );
        }
    }
}
