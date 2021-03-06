using Goblin.Core.Models;

namespace Goblin.Identity.Share.Models.UserModels
{
    public class GoblinIdentityUpdateIdentityModel : GoblinApiSubmitRequestModel
    {
        public string CurrentPassword { get; set; }
        
        // Identity

        public string NewEmail { get; set; }

        public string NewUserName { get; set; }

        public string NewPassword { get; set; }
    }
}