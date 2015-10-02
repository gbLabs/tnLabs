using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GB.tnLabs.AcceptanceTests.Utils
{
    public sealed class RunnableInDebugOnlyFactAttribute: FactAttribute
    {
        public RunnableInDebugOnlyFactAttribute()
        {
            if (!Debugger.IsAttached)
            {
                Skip = "Only running in interactive mode.";
            }
        }
    }
}
