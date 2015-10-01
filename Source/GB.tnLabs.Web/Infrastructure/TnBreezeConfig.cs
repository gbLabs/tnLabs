using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Breeze.ContextProvider;
using Breeze.WebApi2;
using Newtonsoft.Json;

namespace GB.tnLabs.Web.Infrastructure
{
  public class TnBreezeConfig : BreezeConfig
  {
      protected override JsonSerializerSettings CreateJsonSerializerSettings()
      {
          JsonSerializerSettings jsonSettings = base.CreateJsonSerializerSettings();

        //  jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;

          return jsonSettings;
      }
  }
}