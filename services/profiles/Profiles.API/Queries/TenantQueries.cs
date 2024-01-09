using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyGas.Services.Profiles.Queries
{
    public class TenantQueries : ITenantQueries
    {

        private readonly ProfilesDbContext _db;
        public TenantQueries(ProfilesDbContext ctx)
        {
            _db = ctx;
        }

        public async Task<IEnumerable<Tenant>> GetAllAsync()
        {
            var data = await _db.Tenants.ToListAsync();

            return data;
        }

        public async Task<IEnumerable<Branch>> GetAllBranchesAsync(int? tenantId)
        {
            if (tenantId != null)
            {
                var data = await _db.Branches.Where(p => p.TenantId == tenantId).ToListAsync();
                return data;
            }
            else
            {
                var data = await _db.Branches.ToListAsync();
                return data;
            }
        }

        public Tenant GetById(int id)
        {
            return _db.Tenants.SingleOrDefault(x => x.Id == id);
        }

        public Branch GetBranchById(int id)
        {
            return _db.Branches.SingleOrDefault(x => x.Id == id);
        }

        public Tenant GetByUserId(int userid)
        {
            return _db.Tenants.SingleOrDefault(t => t.Users.Any(u => u.Id == userid));
        }
    }
}
