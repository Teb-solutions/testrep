using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.ViewModels
{
    public class BranchModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CallCenterNumber { get; set; }
        public string PictureUrl { get; set; }
        public float Lat { get; set; }
        public float Lng { get; set; }
    }
}
