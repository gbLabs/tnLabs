using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GB.tnLabs.Core.Repository
{
    public class RecommendedVMImage
    {
        public int RecommendedVMImageId { get; set; }

        public string ImageFamily { get; set; }

        public string OSFamily { get; set; }
    }
}
