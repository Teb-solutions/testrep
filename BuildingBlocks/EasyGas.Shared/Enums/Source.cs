using System;
using System.ComponentModel.DataAnnotations;

namespace EasyGas.Shared
{
    public enum Source
    {
        [Display(Name = "Driver App")]
        DRIVER_APP = 1,
        [Display(Name = "Backend Dashboard")]
        ADMIN_WEB,
        [Display(Name = "Customer Web")]
        CUSTOMER_WEB,
        [Display(Name = "Customer App")]
        CUSTOMER_APP,
        [Display(Name = "Relaypoint App")]
        RELAYPOINT_APP,
        [Display(Name = "Distributor Web App")]
        DISTRIBUTOR_WEB_APP,
        [Display(Name = "CRM Web App")]
        CRM,
        [Display(Name = "PulzPartner Web App")]
        PULZPARTNER_WEB_APP,
    }
}
