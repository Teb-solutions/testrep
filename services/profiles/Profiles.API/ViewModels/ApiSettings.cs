namespace EasyGas.Services.Profiles.Models
{
    public class ApiSettings
    {
        public int TenantIdDefault { get; set; }
        public string TenantUidDefault { get; set; }

        public string LocationsApiUrl { get; set; }
        public string GetVehicleLocation => "api/v1/Locations/";
        public string GetVehicleLocations => "api/v1/Locations";
        public string VehicleLocationsApiAccessToken { get; set; }

        public string RoutePlanner { get; set; }
        public string SmsApiUrl { get; set; }
        public string SmsUserCred { get; set; }
        public string SmsTOrderCreation { get; set; }
        public string SmsTOrderDispatch { get; set; }
        public string SmsTRegisterOtp { get; set; }
        public string SmsTResetPasswordOtp { get; set; }
        public string SmsTLoginOtp { get; set; }
        public string GoogleMapsApiBaseUrl { get; set; }
        public string GoogleMapsApiKey { get; set; }

        public string DriverOrderChangedNoti { get; set; }
        public string DriverOrderAssignedNoti { get; set; }
        public string DriverOrderCancelledNoti { get; set; }
        public string DriverOrderDeliveredNoti { get; set; }

        public string FirebaseUrl { get; set; }
        public string DriverFirebaseServerKey { get; set; }
        public string DriverFirebaseSenderID { get; set; }
        public string CustomerFirebaseServerKey { get; set; }
        public string CustomerFirebaseSenderID { get; set; }

        public string BroadcastEmailUrl { get; set; }
        public string BroadcastSmsDeliveredUrl { get; set; }
        public string CustomerInvoiceUrl { get; set; }
        public string WebJobAuthKey { get; set; }
        public string AssetsServiceUrl { get; set; }
        public string BangloreCustomerCarePhone { get; set; }

        public string RazorPayOrdersUri { get; set; }
        public string RazorPayKeyTest { get; set; }
        public string RazorPaySecretTest { get; set; }
        public string RazorPayKeyLive { get; set; }
        public string RazorPaySecretLive { get; set; }
        public string RazorPayEnv { get; set; }

        public string CustomerPaymentWebViewUrl { get; set; }
        public string DriverPaymentWebViewUrl { get; set; }
        public string PaymentDriverTransferAmt { get; set; }
        public string PaymentVendorPercent { get; set; }

        public string EInvEnv { get; set; }

        public string EInvUsernameTest { get; set; }
        public string EInvPasswordTest { get; set; }
        public string EInvClientIdTest { get; set; }
        public string EInvClientSecretTest { get; set; }
        public string EInvAccessTokenUrlTest { get; set; }
        public string EInvGenerateInvoiceUrlTest { get; set; }


        public string EInvUsernameLive { get; set; }
        public string EInvPasswordLive { get; set; }
        public string EInvClientIdLive { get; set; }
        public string EInvClientSecretLive { get; set; }
        public string EInvAccessTokenUrlLive { get; set; }
        public string EInvGenerateInvoiceUrlLive { get; set; }

        public string BroadcastServiceUrl { get; set; }

        public string WalletServiceUrl { get; set; }
        public string CreateWallet => "api/v1/WalletApi/createwallet/";
        public string GetCouponSummary => "api/v1/WalletApi/GetCouponSummaryUser";
        public string GetActiveReferralCoupon => "api/v1/Referral/GetActiveReferralCoupon";
        public string OrderCreationTransaction => "api/v1/WalletApi/OrderCreationCoupon";
        public string OrderCompletionTransaction => "api/v1/WalletApi/OrderCouponCompletion";
        public string OrderCancelTransaction => "api/v1/WalletApi/OrderCancellationCoupon";


        public string AmbassadorReferralCodeStartsWith { get; set; }

        public string OrderAlertToEmail { get; set; }

        public string JwtTokenPrivateKey { get; set; }
        public string JwtTokenIssuer { get; set; }
        public int JwtTokenExpiryMin { get; set; }
        public int RefreshTokenExpiryMin { get; set; }
        public int RefreshTokenTTL { get; set; }

        public string BlobStorageBaseUrl { get; set; }
        public string BlobProfileImageContainer { get; set; }
        public string BlobCustomerComplaintsImageContainer { get; set; }
        public string BlobCustomerInvoicePdfContainer { get; set; }
        public string BlobCustomerDigitalVoucherPdfContainer { get; set; }
        public string BlobCustomerNotificationsContainer { get; set; }
        public string BlobCustomerAppImageContainer { get; set; }

        public string OrderingApiUrl { get; set; }
        public string UpdateExpressOrderBroadcastDrivers => "api/v1/orders/broadcast/";
        public string GetCustomerIdsOrderedInBranch => "api/v1/admin/customer/ids/branch/";
        public string AttachDistributorToActiveOrders => "api/v1/admin/order/attach/distributor/";
        public string GetCustomerRecentOrders => "api/v1/admin/customer/recentorders/";

        public string CartApiUrl { get; set; }
        public string ConvertTempCart => "api/v1/cart/convert/tempcart/";
        public string UpdateDeliverySlots => "api/v1/deliveryslot/";

        public string CatalogApiUrl { get; set; }
        public string AddRelaypointItemMasters => "api/v1/relaypoint/stocks/master/branch/{branchId}/relaypoint/{relaypointId}";
        public string GetDealersAssetCountsForDistributor => "api/v1/asset/count/dealers";
        public string GetDealerAssetCounts => "api/v1/asset/count/dealer/{dealerId}";

        public string EasyGasPuneProfilesApiBaseUrl { get; set; }
        public string EasyGasPuneGetDriversApiUrl => "api/export/GetAllDrivers";
        public string EasyGasPuneGetCustomersApiUrl => "api/export/GetAllCustomers";
        public string EasyGasPuneGetDistributorsApiUrl => "api/export/GetAllDistributors";
        public string EasyGasPuneGetVehiclesApiUrl => "api/export/GetAllVehicles";
        public string EasyGasPuneGetPincodesApiUrl => "api/export/GetAllPincodes";

        public string CrmApiUrl { get; set; }
        public string CreateCrmStaff => "api/v1/staffs";
        public string UpdateCrmStaffDeviceId => "api/v1/staffs/AddDeviceId";

        public string UpdateWalletUniqueIdApiUrl => "api/v1/WalletApi/UpDateNewUserid";
    }
}
