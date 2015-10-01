using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace GB.tnLabs.Core.Components
{
	using System.IO;
	using System.Net.Mail;
	using System.Net.Mime;
	using System.Text;

	using GB.tnLabs.Core.Repository;

	using RazorEngine;

	using Encoding = System.Text.Encoding;
	using MailMessage = System.Net.Mail.MailMessage;

	public class Email
	{
		#region private fields

		private readonly List<EmailMessage> emails;

		#endregion private fields

		#region private .ctor

		private Email(List<EmailMessage> emails)
		{
			this.emails = emails;
		}

		#endregion private .ctor

		#region public methods

		public static Email BuildTemplateVMCaptured(TemplateVM templateVm, bool labCreated = false)
		{
			//TODO: finish!
			string fileName;
			string subject;

			if (labCreated)
			{
				fileName = "LabCreatedWithImage";
				subject = string.Format("Your Lab \"{0}\" has been created", templateVm.VMLabel);
			}
			else
			{
				fileName = "TemplateImageUpdated";
				subject = string.Format("Your Template Image \"{0}\" has been updated", templateVm.VMLabel);
			}

			string template = GetTemplate(fileName);

			EmailMessage emailMessage = new EmailMessage()
			{
				Body = Razor.Parse(
					template,
					new
					{
						User = templateVm.Identity.FirstName,
						LabName = templateVm.VMLabel

					}),
				To = templateVm.Identity.Email,
				Subject = subject,
			};

			var email = new Email(new List<EmailMessage> { emailMessage });

			return email;
		}

		public static Email BuildTemplateVMReady(TemplateVM templateVm)
		{
			string template = GetTemplate("TemplateVMReady");

			EmailMessage emailMessage = new EmailMessage()
				{
					Body = Razor.Parse(
						template,
						new
						{
							User = templateVm.Identity.FirstName,
							UserName = string.Format(@"{0}\{1}", templateVm.VMName, templateVm.VMAdminUser),
							Password = templateVm.VMAdminPass,
							LabName = templateVm.VMLabel,
							RDPHost = templateVm.VMName + ":" + templateVm.VmRdpPort

						}),
					To = templateVm.Identity.Email,
					Subject = string.Format("Template Image VM for \"{0}\" is ready", templateVm.VMLabel),
					Attachement = BuildRdpAttachement(templateVm.VMName, templateVm.VmRdpPort),
					AttachementName = "trainingMachine.rdp"
				};

			var email = new Email(new List<EmailMessage> { emailMessage });

			return email;
		}

		public static Email BuildSessionEmails(Session session, string serviceName)
		{
			string template = GetTemplate("ConnectionDetails");

			var emails = session.VirtualMachines.Select(virtualMachine =>
				new EmailMessage()
					{
						Body = Razor.Parse(template,
						new
						{
							User = virtualMachine.User.FirstName,
							UserName = string.Format(@"{0}\{1}", virtualMachine.VmName, virtualMachine.VmAdminUser),
							Password = virtualMachine.VmAdminPass,
							SessionName = virtualMachine.Session.SessionName,
							RDPHost = serviceName + ".cloudapp.net:" + virtualMachine.VmRdpPort

						}),
						To = virtualMachine.User.Email,
						Subject = string.Format("Connection details for {0} training session", virtualMachine.Session.SessionName),
						Attachement = BuildRdpAttachement(serviceName, virtualMachine.VmRdpPort),
                        AttachementName = serviceName + ".rdp"
					}).ToList();
			var email = new Email(emails);

			return email;
		}

		public static Email BuildSignUpEmail(Identity identity)
		{
			string template = GetTemplate("signup");

			var emailMessage = new EmailMessage()
				{
					Body =
						Razor.Parse(
							template,
							new
								{
									User = identity.DisplayName,
									Email = identity.Email
								}),
					To = "romulus.crisan@endava.com,radu.pascal@endava.com,catalin.stancel@endava.com",
					Subject = string.Format("User {0} just signup", identity.DisplayName),

				};

			var email = new Email(new List<EmailMessage> { emailMessage });

			return email;
		}

		public static Email BuildAssignCertificateEmail(Identity identity, Subscription subscription, byte[] cer)
		{
			string template = GetTemplate("AssignCertificate");

			var emailMessage = new EmailMessage()
			{
				Body =
					Razor.Parse(
						template,
						new
						{
							User = identity.DisplayName
						}),
				To = identity.Email,
				Subject = string.Format("Azure Management Certificate"),
				Attachement = cer,
				AttachementName = "Management.cer"
			};

			var email = new Email(new List<EmailMessage> { emailMessage });

			return email;
		}
                      

		public void Send()
		{
			foreach (var emailMessage in emails)
			{
				var mail = new MailMessage()
							   {
								   Subject = emailMessage.Subject,
								   IsBodyHtml = true,
								   Body = emailMessage.Body

							   };

				var recipients = emailMessage.To.Split(',');

				foreach (var recipient in recipients)
				{
					mail.To.Add(new MailAddress(recipient));
				}

				using (var ms = new MemoryStream())
				{
					if (emailMessage.Attachement != null)
					{
						ms.Write(emailMessage.Attachement, 0, emailMessage.Attachement.Length);

						ms.Seek(0, SeekOrigin.Begin);

						var contentType = new ContentType
							{
								MediaType = MediaTypeNames.Application.Octet,
								Name = emailMessage.AttachementName
							};

						mail.Attachments.Add(new Attachment(ms, contentType));
					}
					var smtpClient = new SmtpClient();
					smtpClient.Send(mail);
				}
			}
		}

		public async Task SendAsync()
		{
			List<Task> waitList = new List<Task>();

			foreach (var emailMessage in emails)
			{
				var mail = new MailMessage()
				{
					Subject = emailMessage.Subject,
					IsBodyHtml = true,
					Body = emailMessage.Body

				};

				var recipients = emailMessage.To.Split(',');

				foreach (var recipient in recipients)
				{
					mail.To.Add(new MailAddress(recipient));
				}

				using (var ms = new MemoryStream())
				{
					if (emailMessage.Attachement != null)
					{
						ms.Write(emailMessage.Attachement, 0, emailMessage.Attachement.Length);

						ms.Seek(0, SeekOrigin.Begin);

						var contentType = new ContentType
						{
							MediaType = MediaTypeNames.Application.Octet,
							Name = emailMessage.AttachementName
						};

						mail.Attachments.Add(new Attachment(ms, contentType));
					}
					var smtpClient = new SmtpClient();
					waitList.Add(smtpClient.SendMailAsync(mail));
				}
			}

			await Task.WhenAll(waitList);
		}

		#endregion public methods

		#region private methods

		private static byte[] BuildRdpAttachement(string vmName, int vmPort)
		{
			var attachmentContet = new StringBuilder();
			attachmentContet.AppendFormat("full address:s:{0}.cloudapp.net:{1}", vmName, vmPort).AppendLine();
			attachmentContet.AppendLine("prompt for credentials:i:1");
			return Encoding.UTF8.GetBytes(attachmentContet.ToString());
		}

		private static string GetTemplate(string templateName)
		{
			string template;
			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
				@"Content\email templates\" + templateName + ".cshtml");

			if (File.Exists(path))
			{
				template = File.ReadAllText(path);
			}
			else
			{
				path = Path.Combine("bin", path);

				template = File.ReadAllText(path);
			}

			return template;
		}

		#endregion private methods
	}

	public class EmailMessage
	{
		public string To { get; set; }

		public string Body { get; set; }

		public string Subject { get; set; }

		public byte[] Attachement { get; set; }

		public string AttachementName { get; set; }
	}
}