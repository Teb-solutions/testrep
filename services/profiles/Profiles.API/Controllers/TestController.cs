using EasyGas.Shared.Enums;
using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.BizLogic;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Queries;
using EasyGas.Services.Profiles.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Profiles.API.Controllers;
using Profiles.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGas.BuildingBlocks.EventBus.Events;
using EasyGas.Shared.Formatters;
using Profiles.API.Services;

namespace EasyGas.Services.Profiles.Controllers
{
    [Route("profiles/api/[controller]/[action]")]
    //[ApiExplorerSettings(IgnoreApi = true)]
    public class TestController : BaseApiController
    {
        private readonly IProfileQueries _queries;
        private readonly OtpMgr _otpMgr;
        private readonly WalletMgr _walletMgr;
        private readonly VoucherMgr _voucherMgr;
        private readonly InvoiceMgr _invoiceMgr;
        //private readonly RazorPayMgr _razorPayMgr;
        private readonly NotificationMgr _notiMgr;
        private readonly IOrderService _orderSevice;
        private ISmsSender _smsSender;
        private IEmailSender _emailSender;
        private string _token = "ADMINTEST@112233";
        private readonly ILogger _logger;

        public TestController(IProfileQueries queries, ILogger<TestController> logger, IEmailSender emailSender,
            NotificationMgr notiMgr, ISmsSender smsSender, IOrderService orderService,
            IConfiguration cfg, ICommandBus bus, OtpMgr otpMgr, WalletMgr walletMgr,
            VoucherMgr voucherMgr, InvoiceMgr invoiceMgr)
            : base(bus)
        {
            _queries = queries;
            _otpMgr = otpMgr;
            _walletMgr = walletMgr;
            _voucherMgr = voucherMgr;
            _invoiceMgr = invoiceMgr;
            _notiMgr = notiMgr;
            _smsSender = smsSender;
            _emailSender = emailSender;
            _orderSevice = orderService;
            _logger = logger;
        }


        [HttpPost]
        public async Task<IActionResult> TestWalletcreation(int userId, string token)
        {
            if (token != _token)
            {
                return BadRequest();
            }
            var ret = await _walletMgr.CreateCustomerWalletWithReferral(userId);
            return Ok(ret);
        }

        /*
        [HttpPost]
        public async Task<IActionResult> TestWalletcreateOrder(int orderId, string token)
        {
            if (token != _token)
            {
                return BadRequest();
            }
            var ret = await _walletMgr.CreateOrderTransaction(orderId);
            return Ok(ret);
        }

        [HttpPost]
        public async Task<IActionResult> TestWalletDeliverOrder(int orderId, string token)
        {
            if (token != _token)
            {
                return BadRequest();
            }
            var ret = await _walletMgr.DeliverOrderTransaction(orderId);
            return Ok(ret);
        }

        [HttpPost]
        public async Task<IActionResult> TestWalletCancelOrder(int orderId, string token)
        {
            if (token != _token)
            {
                return BadRequest();
            }
            var ret = await _walletMgr.CancelOrderTransaction(orderId);
            return Ok(ret);
        }

        [HttpPost]
        public async Task<IActionResult> TestWalletValidateCoupon(string token, CouponType couponType, string couponName, float couponAmount, int userId, OrderType orderType)
        {
            if (token != _token)
            {
                return BadRequest();
            }
            CouponModel couponModel = new CouponModel()
            {
                CouponType = couponType,
                CouponName = couponName,
                OfferAmount = couponAmount
            };

            var ret = await _walletMgr.ValidateCoupon(couponModel, userId, orderType );
            return Ok(ret);
        }
        */

        [HttpPost]
        public async Task<IActionResult> OrderDeliveredSms(string token)
        {
            /*
            if (token != _token)
            {
                return BadRequest();
            }
            */
            var ret = await _smsSender.SendSmsToCustomerForOrderDelivered(1, "A11223", "12122", "https://totaloilrgdiag.blob.core.windows.net/invoice-in/4c5c5e17-9c6e-4546-881f-08c238b1aa64_A3561.pdf");
            return Ok(ret);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateMissingReferralCodes(string token)
        {
            if (token != _token)
            {
                return BadRequest();
            }
            var ret = await _voucherMgr.UpdateMissingReferralCodes();
            return ret;
        }

        [HttpPost]
        public async Task<IActionResult> GenerateMissingWallets(string token, UserType userType)
        {
            if (token != _token)
            {
                return BadRequest();
            }
            var ret = await _walletMgr.CreateMissingWallets(userType);
            return ret;
        }

        [HttpPost]
        public async Task<IActionResult> SendPushNotification(string token, int userId, string desc, string title = "", string imageUrl = "", PushNotificationType type = PushNotificationType.DATA)
        {
            try
            {
                if (token != _token)
                {
                    return BadRequest();
                }
                bool send = await _notiMgr.AddNotification(userId, NotificationType.INFO, NotificationCategory.CUSTOMER_PROMOTIONS, desc, type, title, imageUrl);
                return Ok(send);
            }
            catch (Exception ex)
            {

            }
            return BadRequest();
        }


        [HttpPost]
        public async Task<IActionResult> GetAmbassadorCoupons(string token)
        {
            try
            {
                if (token != _token)
                {
                    return BadRequest();
                }
                return Ok();
            }
            catch (Exception ex)
            {

            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> GetReferralCoupons(string token)
        {
            try
            {
                if (token != _token)
                {
                    _logger.LogInformation("info ref coupon"); _logger.LogWarning("warning ref coupon");
                    return BadRequest();
                }
                _logger.LogWarning("warning ref coupon");
                return Ok();
            }
            catch (Exception ex)
            {

            }
            return BadRequest();
        }

        /*
        [HttpGet]
        public async Task<IActionResult> CreateRazorpayOrder(int orderId, string token)
        {
            if (token != _token)
            {
                return BadRequest();
            }
            string razorPayOrderId = await _razorPayMgr.CreateOrder(orderId);
            if (!string.IsNullOrEmpty(razorPayOrderId))
            {
                return Ok(razorPayOrderId);
            }
            return BadRequest();
        }

        //for testing
        [HttpGet]
        public async Task<IActionResult> VerifyRazorpaySignature(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature, string token)
        {
            if (token != _token)
            {
                return BadRequest();
            }
            bool razorPayOrderId = await _razorPayMgr.VerifySignature(razorpayOrderId, razorpayPaymentId, razorpaySignature);
            if (razorPayOrderId)
            {
                return Ok(razorPayOrderId);
            }
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> RefundPayment(int orderId, string token)
        {
            if (token != _token)
            {
                return BadRequest();
            }
            var refundId = await _razorPayMgr.RefundPayment(orderId);
            return Ok(refundId);
        }

        [HttpGet]
        public async Task<IActionResult> TransferPayment(int orderId, string token)
        {
            if (token != _token)
            {
                return BadRequest();
            }
            var refundId = await _razorPayMgr.TransferPaymentToLinkedAccounts(orderId);
            return Ok(refundId);
        }

        [HttpGet]
        public async Task<IActionResult> CheckAndUpdatePaymentAtDelivery(int orderId, string token)
        {
            if (token != _token)
            {
                return BadRequest();
            }
            Order order = new Order()
            {
                Id = orderId,
                PaymentCompleted = false
            };
            var orderResponse = await _razorPayMgr.CheckAndUpdatePayment(order);
            return Ok(orderResponse);
        }
        

        [HttpGet]
        public async Task<IActionResult> SendOrderAlertEmail(int orderId, string token)
        {
            if (token != _token)
            {
                return BadRequest();
            }
            var send = await _emailSender.SendEmailToAdminForNewOrder(orderId);
            return Ok(send);
        }
        */

        [Route("{token}")]
        [HttpPost]
        public async Task<IActionResult> TestInvoicePdf(string token, [FromBody] CustomerOrderDeliveredIntegrationEvent @event)
        {
            //if (token != _token)
            //{
            //    return BadRequest();
            //}
            var response = await _invoiceMgr.AddInvoiceAndDigitalVoucher(@event);
            return Ok(response);
        }


        [HttpGet]
        public async Task<IActionResult> TestLogger(string token)
        {
            if (token != _token)
            {
                return BadRequest();
            }
            _logger.LogInformation("test inform " + DateMgr.GetCurrentIndiaTime());

            _logger.LogCritical("test critical " + DateMgr.GetCurrentIndiaTime());
            return Ok();
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.BRANCH_ADMIN)]
        public async Task<IActionResult> BranchAdmin()
        {
            return Ok("success");
        }

        [HttpGet]
        public async Task<IActionResult> RelaypointNotification(int relaypointId, int userId)
        {
            var status = await _notiMgr.AddRelaypointPickupOrderAssignedNotification(relaypointId, userId, NotificationCategory.CUSTOMER_PICKUP_ORDERED, "A1111");
            return Ok(status);
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> TenantAdmin()
        {
            return Ok("success");
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN, test")]
        public async Task<IActionResult> MultiAdmin()
        {
            return Ok("success");
        }

        [HttpGet]
        [Authorize()]
        public async Task<IActionResult> UpdateRecentOrders(string token)
        {
            if (token != _token)
            {
                return BadRequest();
            }

            var orders = await _orderSevice.GetCustomerRecentOrders();
            var response = await _notiMgr.UpdateProfileOrderDetails(orders);
            return Ok(response);
        }
    }
}
