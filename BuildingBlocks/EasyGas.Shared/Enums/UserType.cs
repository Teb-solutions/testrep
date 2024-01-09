using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EasyGas.Shared.Enums
{
    public enum UserType
    {
        CUSTOMER = 1,

        /// <summary>
        /// Delivery Driver
        /// </summary>
        DRIVER,

        /// <summary>
        /// DISTRIBUTOR ADMIN
        /// </summary>
        DISTRIBUTOR,

        [Description("Backend Admin")]
        ADMIN,

        /// <summary>
        /// RELAY_POINT ADMIN
        /// </summary>
        RELAY_POINT,

        CUSTOMER_CARE,

        /// <summary>
        /// DEALER ADMIN
        /// </summary>
        DEALER,

        ALDS_ADMIN,

        CARWASH_ADMIN,

        LUBS_ADMIN,

        PICKUP_DRIVER,

        MARSHAL,

        SECURITY
    }
}
