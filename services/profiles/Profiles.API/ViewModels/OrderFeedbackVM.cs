using EasyGas.Shared;
using EasyGas.Shared.Enums;
using System;
namespace EasyGas.Services.Profiles.Models
{
    public class OrderFeedbackVM
    {
        public UserType UserType { get; set; }
        public int OrderId { get; set; }
        public String Feedback { get; set; }
        public float? Rating { get; set; }
        public Source Source { get; set; }
    }
}
