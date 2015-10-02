using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace GB.tnLabs.Web.Models
{
    public class AspNetDbContext : IdentityDbContext<ApplicationUser>
    {
        public AspNetDbContext()
            : base("tnLabsDBEntities", throwIfV1Schema: false)
        {
        }

        public static AspNetDbContext Create()
        {
            return new AspNetDbContext();
        }
                                                                
    }
}