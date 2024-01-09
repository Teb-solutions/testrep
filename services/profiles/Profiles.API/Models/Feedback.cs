using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EasyGas.Services.Profiles.Models
{
    public class Feedback : Trackable
    {   [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? BranchId { get; set; }
        public int TenantId { get; set; }
        public FeedbackType FeedbackType { get; set; }
        public LanguageType Language { get; set; }
        public string Remarks { get; set; }
    }
    public enum FeedbackType
    {
        /// <summary>
        /// order feedback
        /// </summary>
        CUSTOMER_FEEDBACK,
        DRIVER_FEEDBACK,
        DRIVER_CANCEL,
        CUSTOMER_CANCEL,

        /// <summary>
        /// feedback after cylinder is surrendered
        /// </summary>
        CUSTOMER_SURRENDER_FEEDBACK
    }
    public enum LanguageType
    {
        ENGLISH,
        HINDI,
        FRENCH
    }
}
