using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using GB.tnLabs.Core.Repository;
using RazorEngine;
using RazorEngine.Templating;
using RestSharp;
using RestSharp.Authenticators;
using Encoding = System.Text.Encoding;

namespace GB.tnLabs.Core.Components
{
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

            var emailMessage = new EmailMessage()
            {
                Body = Engine.Razor.RunCompile(
                    template, "vmCapturedTemplate", null,
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

            var emailMessage = new EmailMessage()
            {
                Body = Engine.Razor.RunCompile(
                    template, "vmReadyTemplate", null,
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

        public static Email BuildInviteToTnLabs(List<string> emailsTo)
        {
            string template = GetTemplate("InviteToTnLabs");
            var builtEmails = new List<EmailMessage>();

            foreach (var emailTo in emailsTo)
            {
                builtEmails.Add(
                new EmailMessage()
                {
                    Body = Engine.Razor.RunCompile(template, "InviteToTnLabs", null,
                            new
                            {
                                AppUrl = ConfigurationManager.AppSettings["AppUrl"]
                            }),
                    To = emailTo,
                    Subject = string.Format("tnLabs Invitation"),

                });
            }

            return new Email(builtEmails);
        }

        public static Email BuildSessionEmails(Session session, string serviceName)
        {
            string template = GetTemplate("ConnectionDetails");
            var emails = session.VirtualMachines.Select(virtualMachine =>
                new EmailMessage()
                {
                    Body = Engine.Razor.RunCompile(template, "sessionTemplate", null,
                        new
                        {
                            User = virtualMachine.Identity.FirstName,
                            UserName = string.Format(@"{0}\{1}", virtualMachine.VmName, virtualMachine.VmAdminUser),
                            Password = virtualMachine.VmAdminPass,
                            virtualMachine.Session.SessionName,
                            RDPHost = serviceName + ".cloudapp.net:" + virtualMachine.VmRdpPort

                        }),
                    To = virtualMachine.Identity.Email,
                    Subject = string.Format("Connection details for {0} training session", virtualMachine.Session.SessionName),
                    Attachement = BuildRdpAttachement(serviceName, virtualMachine.VmRdpPort),
                    AttachementName = serviceName + ".rdp"
                }).ToList();
            var email = new Email(emails);

            return email;
        }

        public static Email BuildSessionStartVmsEmails(string trainerFirstName, string toEmailAddress, Session session, string serviceName, int nbOfUsers)
        {
            string template = GetTemplate("StartingVirtualMachines");
            var builtEmails = new List<EmailMessage>();

            builtEmails.Add(
              new EmailMessage()
              {
                  Body = Engine.Razor.RunCompile(template, "StartingVirtualMachines", null,
                          new
                          {
                              TrainerFirstName = trainerFirstName,
                              NumberOfUsers = nbOfUsers,
                              SessionName = session.SessionName,
                              StartDate = session.StartDate,
                              EndDate = session.EndDate,
                              LabName = session.Lab.Name,
                              ServiceName = serviceName
                          }),
                  To = toEmailAddress,
                  Subject = string.Format("[GbLabs] Starting VM(s) for the {0} training session", session.SessionName),
              });

            return new Email(builtEmails);
        }

        public static Email BuildSessionEndVmsEmails(string trainerFirstName, string toEmailAddress, string sessionName, string serviceName, int nbOfUsers, int nbOfVMs)
        {
            string template = GetTemplate("FinishedStartingTheVirtualMachines");
            var builtEmails = new List<EmailMessage>();

            builtEmails.Add(
             new EmailMessage()
             {
                 Body = Engine.Razor.RunCompile(template, "FinishedStartingTheVirtualMachines", null,
                         new
                         {
                             TrainerFirstName = trainerFirstName,
                             NumberOfUsers = nbOfUsers,
                             NumberOfVMs = nbOfVMs,
                             SessionName = sessionName,
                             ServiceName = serviceName
                         }),
                 To = toEmailAddress,
                 Subject = string.Format("[GbLabs] Finished starting VM(s) for the {0} training session", sessionName),
             });

            return new Email(builtEmails);
        }

        public static Email BuildProblemStartingVmsEmails(string trainerFirstName, string toEmailAddress, string sessionName, string serviceName, string errorMessage)
        {
            string template = GetTemplate("ProblemStartingTheVirtualMachines");
            var builtEmails = new List<EmailMessage>();

            builtEmails.Add(
            new EmailMessage()
            {
                Body = Engine.Razor.RunCompile(template, "ProblemStartingTheVirtualMachines", null,
                        new
                        {
                            TrainerFirstName = trainerFirstName,
                            SessionName = sessionName,
                            ServiceName = serviceName,
                            ErrorMessage = errorMessage
                        }),
                To = toEmailAddress,
                Subject = string.Format("[GbLabs] Error occured when starting VM(s) for the {0} training session", sessionName),
            });

            return new Email(builtEmails);
        }

        public static Email BuildSignUpEmail(Identity identity)
        {
            string template = GetTemplate("signup");

            var emailMessage = new EmailMessage()
            {
                Body =
                    Engine.Razor.RunCompile(
                        template, "signupEmailTemplate", null,
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
                    Engine.Razor.RunCompile(
                        template, "assignCertificateTemplate", null,
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

        public bool Send()
        {
            return (from emailMessage in emails
                    let client = CreateRestClient()
                    let request = CreateEmailRequest(emailMessage)
                    select client.Execute(request)).All(response => response.StatusCode == HttpStatusCode.OK);
        }

        public void SendAsync()
        {
            foreach (var emailMessage in emails)
            {
                var client = CreateRestClient();
                var request = CreateEmailRequest(emailMessage);

                client.ExecuteAsync(request, MailResponse);
            }
        }

        private void MailResponse(IRestResponse response, RestRequestAsyncHandle handle)
        {
            //Do nothing if mail failed
        }

        #endregion public methods

        #region private methods

        private RestRequest CreateEmailRequest(EmailMessage emailMessage)
        {
            var request = new RestRequest();
            request.AddParameter("domain",
                "sandbox3280ff42a045437d9d190757b3b5caf2.mailgun.org", ParameterType.UrlSegment);
            request.Resource = "sandbox3280ff42a045437d9d190757b3b5caf2.mailgun.org/messages";
            request.AddParameter("from", "mailer@tnlabs.sandbox3280ff42a045437d9d190757b3b5caf2.mailgun.org");
            request.AddParameter("to", emailMessage.To);
            request.AddParameter("subject", emailMessage.Subject);
            request.AddParameter("html", emailMessage.Body);

            if (emailMessage.Attachement != null && emailMessage.AttachementName != null)
            {
                request.AddFile("attachment", emailMessage.Attachement, emailMessage.AttachementName,
                      MediaTypeNames.Application.Octet);
            }

            request.Method = Method.POST;
            return request;
        }

        private RestClient CreateRestClient()
        {
            var key = ConfigurationManager.AppSettings["mailgun"];

            var client = new RestClient
            {
                BaseUrl = new Uri("https://api.mailgun.net/v3"),
                Authenticator = new HttpBasicAuthenticator("api",
                    key)
            };
            return client;
        }

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