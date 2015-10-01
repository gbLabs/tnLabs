using System.Data.Entity;
using GB.tnLabs.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GB.tnLabs.Core.Annotations;

namespace GB.tnLabs.Core.Components
{
	[UsedImplicitly]
	public class HealthCheck
	{
		public void CheckIn(Guid resourceId)
		{
			UpdateResourceStatusAsync(resourceId).Wait();

			CheckForSystemHealthProblemsAsync().Wait();
		}

		public async Task CheckInAsync(Guid resourceId)
		{
			await UpdateResourceStatusAsync(resourceId);

			await CheckForSystemHealthProblemsAsync();
		}

		private async static Task CheckForSystemHealthProblemsAsync()
		{
			using (tnLabsDBEntities repository = new tnLabsDBEntities())
			{
				//check for any resources that haven't checked back for a while
				//and if notification is not sent, send it
				var allChecks = await repository.HealthChecks.ToListAsync();
				var timedOutResources = allChecks.Where(x =>
					(x.LastCheck.HasValue && x.CheckInterval.HasValue) &&
					(x.LastCheck.Value + TimeSpan.FromSeconds(x.CheckInterval.Value.TotalSeconds * 3) > DateTimeOffset.Now) &&
					(!x.NotificationSent.HasValue || !x.NotificationSent.Value)).ToList();

				await Email.BuildHealthProblemsEmail(timedOutResources).SendAsync();

				timedOutResources.ForEach(x => x.NotificationSent = true);
				await repository.SaveChangesAsync();
			}
		}

		private async static Task UpdateResourceStatusAsync(Guid resourceId)
		{
			using (tnLabsDBEntities repository = new tnLabsDBEntities())
			{
				var healthCheck = await repository.HealthChecks.SingleOrDefaultAsync(x => x.ResourceId == resourceId);

				if (healthCheck == null)
				{
					healthCheck = new Repository.HealthCheck
					{
						ResourceId = Guid.NewGuid(),
						Description = "Unknown"
					};

					repository.HealthChecks.Add(healthCheck);
				}

				if (healthCheck.LastCheck.HasValue && !healthCheck.CheckInterval.HasValue)
				{
					healthCheck.CheckInterval = (DateTimeOffset.Now - healthCheck.LastCheck.Value);
				}

				//reset the notification sent so it can send notifications again when the resource is down
				healthCheck.NotificationSent = null;
				healthCheck.LastCheck = DateTimeOffset.Now;

				await repository.SaveChangesAsync();
			}
		}


	}
}
