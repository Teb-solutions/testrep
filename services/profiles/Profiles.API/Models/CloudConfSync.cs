using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyGas.Services.Profiles.Models
{
    public class CloudConfSync {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public String tenantName { get; set; }
        public String tenantLocation { get; set; }
        public int tenantID { get; set; }
        public int branchID { get; set; }
        public String loggerUrl { get; set; }

        public String loggerProfileUrl { get; set; }
        public String faqUrl { get; set; }
        public String ratesUrl { get; set; }
        public String helpUrl { get; set; }
        public String safetyUrl { get; set; }
        public String referralUrl { get; set; }
        public String rateusUrl { get; set; }
        public String aboutUrl { get; set; }
        public String yourratingUrl { get; set; }
        public String graphUrl { get; set; }
        public String rewardsUrl { get; set; }

    }



}
