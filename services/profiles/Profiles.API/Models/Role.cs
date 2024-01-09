using EasyGas.Services.Profiles.Models;
using EasyGas.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Profiles.API.Models
{
    public class Role : Trackable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(25)]
        public string Name { get; set; }
        [MaxLength(50)]
        public string DisplayName { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }

        public Role(string name)
        {
            Name = name;
        }

    }

    public class RoleNames
    {
        public const string CUSTOMER = "CUSTOMER";
        public const string DRIVER = "DRIVER";
        public const string RELAYPOINT_ADMIN = "RELAYPOINT_ADMIN";
        public const string BRANCH_ADMIN = "BRANCH_ADMIN";
        public const string TENANT_ADMIN = "TENANT_ADMIN";
        public const string VITE_ADMIN = "VITE_ADMIN";
        public const string CUSTOMER_CARE = "CUSTOMER_CARE";
        public const string CUSTOMER_CARE_ADMIN = "CUSTOMER_CARE_ADMIN";
        public const string DISTRIBUTOR_ADMIN = "DISTRIBUTOR_ADMIN";
        public const string DEALER_ADMIN = "DEALER_ADMIN";
        public const string ALDS_ADMIN = "ALDS_ADMIN";
        public const string CARWASH_ADMIN = "CARWASH_ADMIN";
        public const string LUBS_ADMIN = "LUBS_ADMIN";
        public const string MARSHAL = "MARSHAL";
        public const string SECURITY = "SECURITY";
        public const string PICKUP_DRIVER = "PICKUP_DRIVER";

        public string GetRoleOfUserType(UserType type)
        {
            switch (type) 
            {
                case UserType.DRIVER:
                    return DRIVER;
                case UserType.CUSTOMER:
                    return CUSTOMER;
                case UserType.RELAY_POINT:
                    return RELAYPOINT_ADMIN;
                case UserType.CUSTOMER_CARE:
                    return CUSTOMER_CARE;
                case UserType.CARWASH_ADMIN:
                    return CARWASH_ADMIN;
                case UserType.DEALER:
                    return DEALER_ADMIN;
                case UserType.ALDS_ADMIN:
                    return ALDS_ADMIN;
                case UserType.DISTRIBUTOR:
                    return DISTRIBUTOR_ADMIN;
                case UserType.LUBS_ADMIN:
                    return LUBS_ADMIN;
                case UserType.MARSHAL:
                    return MARSHAL;
                case UserType.SECURITY:
                    return SECURITY;
                case UserType.PICKUP_DRIVER:
                    return PICKUP_DRIVER;
                default: return null;
            }
        }
    }
}
