using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using System.Text.RegularExpressions;

namespace GB.tnLabs.Web.API_Controllers
{
    public class ManageController : ApiController
    {
        [System.Web.Http.HttpPost]
        public string SendInvites()
        {
            var emails = Request.Content.ReadAsStringAsync().Result.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries );
            return "result";
        }
    }
}