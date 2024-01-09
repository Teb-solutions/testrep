using EasyGas.Services.Profiles.Data;
using EasyGas.Shared.Enums;
using EasyGas.Shared.Extensions;
using EasyGas.Shared.Formatters;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.BizLogic
{
    public interface IJobMgr
    {
        Task<bool> RunNotificationCronJob();
    }

    public class JobMgr : IJobMgr
    {
        private readonly NotificationSettingsJobMgr _notificationSettingsJobMgr;
        private readonly ILogger<JobMgr> _logger;
        private readonly ProfilesDbContext _db;


        public JobMgr(NotificationSettingsJobMgr notificationSettingsJobMgr, ILogger<JobMgr> logger,
            ProfilesDbContext db)
        {
            _notificationSettingsJobMgr = notificationSettingsJobMgr;
            _db = db;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> RunNotificationCronJob()
        {
            #if DEBUG
                return false;
            #endif

            _logger.LogInformation("Hangfire RunNotificationCronJob started");
            var city = await _db.Branches.FirstAsync();
            var result = await _notificationSettingsJobMgr.RunCronJob(city.Id); //TODO for all cities
            if (result.IsOk)
            {
                _logger.LogInformation("Hangfire RunNotificationCronJob succesfully completed");
            }
            else
            {
                _logger.LogInformation("Hangfire RunNotificationCronJob failure");
            }

            return true;
        }
    }
}
