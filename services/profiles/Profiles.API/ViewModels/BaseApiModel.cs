using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyGas.Services.Profiles.Controllers
{
   
        public class BaseApiModel

        {

            [Key]

            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public int Id { get; set; }

            public DateTime CreatedAt { get; set; }

        }


    
}
