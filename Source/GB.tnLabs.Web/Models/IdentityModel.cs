using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GB.tnLabs.Web.Models
{
    [Obsolete]
	public class IdentityModel_old_to_be_removed
	{
		public int IdentityId { get; set; }
		
		public string NameIdentifier { get; set; }

		public string IdentityProvider { get; set; }

		[Required]
		[Display(Name = "First Name")]
		public string FirstName { get; set; }

		[Required]
		[Display(Name = "Last Name")]
		public string LastName { get; set; }

		[Required]
        [Obsolete]
		[Display(Name = "Display Name")]
		public string DisplayName { get; set; }

		[Required]
		[Display(Name = "Email")]
		[EmailAddress(ErrorMessage = "Invalid Email Address")]
		public string Email { get; set; }
	}
}