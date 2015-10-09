using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using System.Text.RegularExpressions;
using GB.tnLabs.Core.Components;

namespace GB.tnLabs.Web.API_Controllers
{
    public class ManageController : ApiController
    {
        [System.Web.Http.HttpPost]
        public int SendInvites()
        {
            var requestContent = HttpUtility.UrlDecode(Request.Content.ReadAsStringAsync().Result);
            //TODO:find a better way
            requestContent = requestContent.Replace("value=", "");

            var emailList = new List<string>();

            if (!String.IsNullOrWhiteSpace(requestContent))
            {
                if (requestContent.Contains(";"))
                {
                    var emails = requestContent.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var email in emails)
                    {
                        var trimmedEmail = email.TrimStart().TrimEnd();
                        if (IsMailValid(trimmedEmail))
                            emailList.Add(trimmedEmail);
                    }
                }
                //only have an email
                else
                {
                    var trimmedEmail = requestContent.TrimStart().TrimEnd();
                    if (IsMailValid(trimmedEmail))
                        emailList.Add(trimmedEmail);
                }
                if (emailList.Count > 0)
                {
                    //create emails
                    var email = Email.BuildInviteToTnLabs(emailList);

                    //send emails
                    email.Send();
                }
            }
            return emailList.Count;
        }

        public bool IsMailValid(string email)
        {
            return (Regex.Match(email, @"[\w+-]+(?:\.[\w+-]+)*@[\w+-]+(?:\.[\w+-]+)*(?:\.[a-zA-Z]{2,4})").Success);
        }
    }
}