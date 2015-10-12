using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using System.Text.RegularExpressions;
using GB.tnLabs.Core.Components;

namespace GB.tnLabs.Web.APIControllers
{
    public class ManageController : ApiController
    {
        [System.Web.Http.HttpPost]
        public int SendInvites()
        {
            var emailList = new List<string>();
            var requestContent = HttpUtility.UrlDecode(Request.Content.ReadAsStringAsync().Result);

            //TODO:find a better way
            requestContent = requestContent.Replace("value=", "");

            if (!string.IsNullOrWhiteSpace(requestContent))
            {
                //check to see if the request contains multiple emails
                if (requestContent.Contains(";"))
                {
                    var emails = requestContent.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    //check emails format, if they are valid they are addded to the invitation email list
                    foreach (var email in emails)
                    {
                        var trimmedEmail = email.TrimStart().TrimEnd();
                        if (IsEmailValid(trimmedEmail))
                            emailList.Add(trimmedEmail);
                    }
                }
                else
                {
                    //the request contains only one email which if it is vaild, it is added to the invitation email list
                    var trimmedEmail = requestContent.TrimStart().TrimEnd();
                    if (IsEmailValid(trimmedEmail))
                        emailList.Add(trimmedEmail);
                }

                //build and send the invitations if there is at least one valid email in the emailList
                if (emailList.Count > 0)
                    Email.BuildInviteToTnLabs(emailList).Send();

            }
            return emailList.Count;
        }

        private bool IsEmailValid(string email)
        {
            return Regex.Match(email, @"[\w+-]+(?:\.[\w+-]+)*@[\w+-]+(?:\.[\w+-]+)*(?:\.[a-zA-Z]{2,4})").Success;
        }
    }
}