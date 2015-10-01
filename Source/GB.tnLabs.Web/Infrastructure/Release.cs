using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GB.tnLabs.Web.Infrastructure
{
    public class Release : IDisposable
    {
        private readonly Action release;

        public Release(Action release)
        {
            this.release = release;
        }

        public void Dispose()
        {
            this.release();
        }
    }
}