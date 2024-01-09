using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Queries
{
    public interface ITenantQueries
    {
        Task<IEnumerable<Tenant>> GetAllAsync();
        Task<IEnumerable<Branch>> GetAllBranchesAsync(int? tenantId);
        Tenant GetById(int id);
        Branch GetBranchById(int id);
        Tenant GetByUserId(int userid);

    }
}
