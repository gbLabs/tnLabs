using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace GB.tnLabs.Core.Repository
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("tnLabsDBEntities")
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {


            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
        }

        public DbSet<SessionUser> SessionUsers { get; set; }

        public DbSet<Invitation> Invitations { get; set; }

        public DbSet<VirtualMachine> VirtualMachines { get; set; }

        public DbSet<Identity> Identities { get; set; }

        public DbSet<SubscriptionIdentityRole> SubscriptionIdentityRoles { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }

        public DbSet<Lab> Labs { get; set; }

        public DbSet<RecommendedVMImage> RecommendedVMImages { get; set; }

        public DbSet<TemplateVM> TemplateVMs { get; set; }

        public DbSet<Session> Sessions { get; set; }

    }
}