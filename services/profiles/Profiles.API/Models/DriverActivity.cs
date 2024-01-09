using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EasyGas.Services.Profiles.Models
{
    public enum DLoginState
    {
        LoggedIN,
        PauseJob,
        ResumeJob,
        LoggedOut,
    }


    public enum DActivityState
    {
        NoJob,
        HasJob,
        DisplayedJob,
        OrderVerification,
        PauseJob,
        ResumeJob,
        TripStarted,
        DirectionInMaps,
        TripDelivered,
        TripCancelled,
        FeedbackCaptured,
        YetToReserve
    }

    public class DriverActivity : Trackable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public int? VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public virtual Vehicle Vehicle { get; set; }
        public DateTime DutyStart { get; set; }
        public double DutyStartLat { get; set; }
        public double DutyStartLng { get; set; }
        public DateTime? DutyEnd { get; set; }
        public double? DutyEndLat { get; set; }
        public double? DutyEndLng { get; set; }

        public DLoginState dls { get; set; }
        public DActivityState das { get; set; }

        public DateTime? CurrentTime { get; set; }
        public double? CurrentLat { get; set; }
        public double? CurrentLng { get; set; }


    }


}
