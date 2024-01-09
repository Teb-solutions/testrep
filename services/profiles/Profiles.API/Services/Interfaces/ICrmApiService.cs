using Profiles.API.ViewModels;
using Profiles.API.ViewModels.Broadcast;
using Profiles.API.ViewModels.Crm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.Services
{
    public interface ICrmApiService
    {
        Task<bool> CreateStaff(CreateCrmStaffRequest req, int userId);
        Task<bool> UpdateStaffDeviceId(int userId, string deviceId, string accessToken);
    }
}
