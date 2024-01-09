using EasyGas.Services.Profiles.Data;
using EasyGas.Services.Profiles.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.BizLogic
{
    public class GeoFenceMgr
    {
        ProfilesDbContext _db;

        private float MaxDistFromBranch = 100000; //TODO change hard code

        public GeoFenceMgr(ProfilesDbContext db)
        {
            _db = db;
        }

        public int GetBranchByRadius(int tenantId, double orderLat, double orderLng)
        {
            int branchId = 0;
            List<Branch> branches = _db.Branches.Where(p => p.TenantId == tenantId && p.IsActive == true).ToList();
            foreach (var branch in branches)
            {
                var distFromBranch = GetDistanceBetweenLocationinMtr(orderLat, orderLng, branch.Lat, branch.Lng);
                if (distFromBranch < MaxDistFromBranch)
                {
                    branchId = branch.Id;
                }
            }
            return branchId;
        }

        public bool IsPincodeDeliverable(string pincode, double? lat, double? lng)
        {
            if (!_db.Pincodes.Any(p => p.Code == pincode && p.IsActive == true))
            {
                return false;
            }
            return true;
        }

        public async Task<Branch> GetBranchByLocation(int tenantId, string location, double? lat, double? lng)
        {
            if (!string.IsNullOrEmpty(location))
            {
                var branches = await _db.Branches.Where(p => p.TenantId == tenantId).ToListAsync();
                location = location.ToLower();
                foreach (var branch in branches)
                {
                    string branchName = branch.Name.ToLower();
                    if (location.Contains(branchName))
                    {
                        return branch;
                    }
                }
            }

            return null;
        }

        public Branch GetBranchByPincode(string pincode)
        {
            var pincodeModel = _db.Pincodes
                .Include(x => x.Branch)
                .Where(x => x.Code == pincode && x.IsActive)
                .FirstOrDefault();
            if (pincodeModel != null && pincodeModel.Branch != null)
            {
                return pincodeModel.Branch;
            }
            return null;
        }

        public async Task<Branch> GetBranchByLatLng(double lat, double lng)
        {
            double nearestDistance = 0;
            List<Branch> branches = await _db.Branches.Where(p => p.IsActive == true).ToListAsync();
            Branch nearestBranch = branches.First();
            foreach (var branch in branches)
            {
                var distFromBranch = GetDistanceBetweenLocationinMtr(lat, lng, branch.Lat, branch.Lng);
                if (nearestDistance == 0 || distFromBranch < nearestDistance)
                {
                    nearestDistance = distFromBranch;
                    nearestBranch = branch;
                }
            }

            return nearestBranch;
        }

        public Pincode GetPincodeModel(string pincode, double? lat, double? lng)
        {
            return _db.Pincodes.Include(p => p.Branch).Where(p => p.Code == pincode && p.IsActive).FirstOrDefault();
        }

        public double GetDistanceBetweenLocationinMtr(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371; // km
            double sLat1 = Math.Sin(Radians(lat1));
            double sLat2 = Math.Sin(Radians(lat2));
            double cLat1 = Math.Cos(Radians(lat1));
            double cLat2 = Math.Cos(Radians(lat2));
            double cLon = Math.Cos(Radians(lon1) - Radians(lon2));
            double cosD = sLat1 * sLat2 + cLat1 * cLat2 * cLon;
            double d = Math.Acos(cosD);
            double dist = R * d * 1000;
            //dist = 1;
            dist += dist * 25 / 100; //it is road distance so I am adding 25%

            return dist > 0 ? dist : 0;
        }

        private static double Radians(double x)
        {
            return x * Math.PI / 180;
        }
    }
}
