using GB.tnLabs.Core.Annotations;
using GB.tnLabs.Core.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.Components
{
	[UsedImplicitly]
	public class SignUp
	{
		class CertificateSet
		{

			public byte[] PfxRawData { get; set; }

			public string Password { get; set; }

			public byte[] CerRawData { get; set; }
		}

		public void AssignCertificate(int subscriptionId, int identityId)
		{
			//create subscription if doesn't exist, otherwise update with new certificate
			CertificateSet certificateSet = CreateCertificate();

			//save the cer and pfx to local folder for debugging (si it me, or does that sound like a bad idea?)
			SaveCertificatesLocally(subscriptionId.ToString(), certificateSet);

			//TODO: finish implementation - send email

			using (tnLabsDBEntities context = new tnLabsDBEntities())
			{
				Subscription subscription = context.Subscriptions.Single(x => x.SubscriptionId == subscriptionId);
				subscription.CertificateKey = certificateSet.Password;
				subscription.Certificate = certificateSet.PfxRawData;
				context.SaveChanges();

				Identity identity = context.Identities.Single(x => x.IdentityId == identityId);

				Email.BuildAssignCertificateEmail(identity, subscription, certificateSet.CerRawData).Send();
			}
		}

		private CertificateSet CreateCertificate()
		{
			//TODO: randomize the password
			string password = "gb_Password1";

			CertificateSet certificateSet = new CertificateSet
			{
				Password = password,
				PfxRawData =
					Certificate.CreateSelfSignCertificatePfx("O=tnLabs1,CN=tnLabs,SN=tnLabs3",
						DateTime.Now.AddDays(-1), DateTime.Now.AddYears(10), password)
			};

			//CN is showed in the Azure Management

			X509Certificate2 cert = new X509Certificate2(certificateSet.PfxRawData, password);
			certificateSet.CerRawData = cert.GetRawCertData();

			return certificateSet;
		}

		private void SaveCertificatesLocally(string name, CertificateSet certificateSet)
		{
			FileStream pfxStream = new FileStream(name + ".pfx", FileMode.Create, FileAccess.Write);
			pfxStream.Write(certificateSet.PfxRawData, 0, certificateSet.PfxRawData.Length);
			pfxStream.Close();

			FileStream cerStream = new FileStream(name + ".cer", FileMode.Create, FileAccess.Write);
			cerStream.Write(certificateSet.CerRawData, 0, certificateSet.CerRawData.Length);
			cerStream.Close();
		}
	}
}
