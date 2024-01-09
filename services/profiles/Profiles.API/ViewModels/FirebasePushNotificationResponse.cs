namespace Profiles.API.ViewModels
{
    public class FirebasePushNotificationResponse
    {
        public string multicast_id { get; set; }
        public int success { get; set; }
        public int failure { get; set; }
        public string canonical_ids { get; set; }
        public FirebasePushNotificationResult[] results { get; set; }
    }

    public class FirebasePushNotificationResult
    {
        public string error { get; set; }
        public string message_id { get; set; }
    }
}
