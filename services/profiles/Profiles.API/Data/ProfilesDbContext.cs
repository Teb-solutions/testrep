using Microsoft.EntityFrameworkCore;
using EasyGas.Services.Profiles.Models;
using EasyGas.Services.Profiles.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyGas.Shared.Formatters;
using System.Threading;
using Microsoft.EntityFrameworkCore.Internal;
using EasyGas.Services.Profiles.Controllers;
using RelayPointLogistics.Models;
using Profiles.API.Models;
using EasyGas.Shared.Enums;
using Microsoft.EntityFrameworkCore.Storage;

namespace EasyGas.Services.Profiles.Data
{
    public class ProfilesDbContext : DbContext
    {
        public string UserId { get; set; }
        public ProfilesDbContext(DbContextOptions<ProfilesDbContext> options)
            : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        private IDbContextTransaction _currentTransaction;
        public IDbContextTransaction GetCurrentTransaction() => _currentTransaction;
        public bool HasActiveTransaction => _currentTransaction != null;

        public override int SaveChanges()
        {
            this.ChangeTracker.DetectChanges();
            var added = this.ChangeTracker.Entries()
                        .Where(t => t.State == EntityState.Added)
                        .Select(t => t.Entity)
                        .ToArray();

            foreach (var entity in added)
            {
                if (entity is ITrack)
                {
                    var track = entity as ITrack;
                    track.CreatedAt = DateMgr.GetCurrentIndiaTime();
                    track.UpdatedAt = DateMgr.GetCurrentIndiaTime();
                    if (!string.IsNullOrEmpty(UserId))
                    {
                        track.CreatedBy = UserId;
                        track.UpdatedBy = UserId;
                    }
                }
            }

            var modified = this.ChangeTracker.Entries()
                        .Where(t => t.State == EntityState.Modified)
                        .Select(t => t.Entity)
                        .ToArray();

            foreach (var entity in modified)
            {
                if (entity is ITrack)
                {
                    var track = entity as ITrack;
                    track.UpdatedAt = DateMgr.GetCurrentIndiaTime();
                    if (!string.IsNullOrEmpty(UserId))
                    {
                        track.UpdatedBy = UserId;
                    }
                }
            }

            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            this.ChangeTracker.DetectChanges();
            var added = this.ChangeTracker.Entries()
                        .Where(t => t.State == EntityState.Added)
                        .Select(t => t.Entity)
                        .ToArray();

            foreach (var entity in added)
            {
                if (entity is ITrack)
                {
                    var track = entity as ITrack;
                    track.CreatedAt = DateMgr.GetCurrentIndiaTime();
                    track.UpdatedAt = DateMgr.GetCurrentIndiaTime();
                    if (!string.IsNullOrEmpty(UserId))
                    {
                        track.CreatedBy = UserId;
                        track.UpdatedBy = UserId;
                    }
                }
            }

            var modified = this.ChangeTracker.Entries()
                        .Where(t => t.State == EntityState.Modified)
                        .Select(t => t.Entity)
                        .ToArray();

            foreach (var entity in modified)
            {
                if (entity is ITrack)
                {
                    var track = entity as ITrack;
                    track.UpdatedAt = DateMgr.GetCurrentIndiaTime();
                    if (!string.IsNullOrEmpty(UserId))
                    {
                        track.UpdatedBy = UserId;
                    }
                }
            }

            return await base.SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SetStringMaxLengthConvention(255);

            // One to one between users and profiles where FK is on
            // profiles (UserId)
            
            modelBuilder.Entity<UserProfile>()
                .HasOne<User>(c => c.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(p => p.UserId);

            modelBuilder
            .Entity<DeliverySlot>()
            .Property(e => e.Type)
            .HasConversion(
                v => v.ToString(),
                v => (SlotType)Enum.Parse(typeof(SlotType), v));

            modelBuilder
            .Entity<DeliverySlot>()
            .Property(e => e.DeliveryMode)
            .HasConversion(
                v => v.ToString(),
                v => (DeliveryMode)Enum.Parse(typeof(DeliveryMode), v));

            modelBuilder
            .Entity<BusinessEntity>()
            .Property(e => e.Type)
            .HasConversion(
                v => v.ToString(),
                v => (BusinessEntityType)Enum.Parse(typeof(BusinessEntityType), v));

            modelBuilder
            .Entity<BusinessEntity>()
            .Property(e => e.WorkingStartDay)
            .HasConversion(
                v => v.ToString(),
                v => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), v));

            modelBuilder
            .Entity<BusinessEntityTiming>()
            .Property(e => e.Day)
            .HasConversion(
                v => v.ToString(),
                v => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), v));

            modelBuilder
            .Entity<BusinessEntity>()
            .Property(e => e.WorkingEndDay)
            .HasConversion(
                v => v.ToString(),
                v => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), v));

            modelBuilder
            .Entity<Complaint>()
            .Property(e => e.Category)
            .HasConversion(
                v => v.ToString(),
                v => (ComplaintCategory)Enum.Parse(typeof(ComplaintCategory), v));

            modelBuilder
            .Entity<Complaint>()
            .Property(e => e.Status)
            .HasConversion(
                v => v.ToString(),
                v => (ComplaintStatus)Enum.Parse(typeof(ComplaintStatus), v));

            modelBuilder
            .Entity<Feedback>()
            .Property(e => e.FeedbackType)
            .HasConversion(
                v => v.ToString(),
                v => (FeedbackType)Enum.Parse(typeof(FeedbackType), v));

            modelBuilder
            .Entity<Feedback>()
            .Property(e => e.Language)
            .HasConversion(
                v => v.ToString(),
                v => (LanguageType)Enum.Parse(typeof(LanguageType), v));

            modelBuilder
            .Entity<NotificationSettings>()
            .Property(e => e.NotiCategory)
            .HasConversion(
                v => v.ToString(),
                v => (NotificationCategory)Enum.Parse(typeof(NotificationCategory), v));

            modelBuilder
            .Entity<NotificationSettings>()
            .Property(e => e.NotiTriggerType)
            .HasConversion(
                v => v.ToString(),
                v => (NotificationTriggerType)Enum.Parse(typeof(NotificationTriggerType), v));

            modelBuilder
            .Entity<NotificationSettings>()
            .Property(e => e.NotiType)
            .HasConversion(
                v => v.ToString(),
                v => (NotificationType)Enum.Parse(typeof(NotificationType), v));

            modelBuilder
            .Entity<NotificationSettings>()
            .Property(e => e.NotiUserCategory)
            .HasConversion(
                v => v.ToString(),
                v => (NotificationUserCategory)Enum.Parse(typeof(NotificationUserCategory), v));

            modelBuilder
            .Entity<NotificationSettings>()
            .Property(e => e.NotiTriggerTime)
            .HasConversion(
                v => v.ToString(),
                v => (NotificationTriggerTime)Enum.Parse(typeof(NotificationTriggerTime), v));

            modelBuilder
            .Entity<Device>()
            .Property(e => e.Type)
            .HasConversion(
                v => v.ToString(),
                v => (DeviceType)Enum.Parse(typeof(DeviceType), v));

            modelBuilder
            .Entity<Device>()
            .Property(e => e.FuelType)
            .HasConversion(
                v => v.ToString(),
                v => (FuelType)Enum.Parse(typeof(FuelType), v));

        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Pincode> Pincodes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserAddress> Addresses { get; set; }
        public DbSet<UserProfile> Profiles { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }
        public DbSet<UserLogin> UserLogin { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Otp> Otps { get; set; }


        //public DbSet<ItemCapacity> ItemCapacities { get; set; }
        public DbSet<DeliverySlot> DeliverySlots { get; set; }
        public DbSet<BusinessEntity> BusinessEntities { get; set; }
        public DbSet<BusinessEntityTiming> BusinessEntityTimings { get; set; }

        //public DbSet<Order> Orders { get; set; }
        //public DbSet<OrderAddress> OrderAddresses { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }

        //public DbSet<PulzeRoutePlan> PulzeRoutePlans { get; set; }
        //public DbSet<PulzeVehicleRoutePlan> PulzeVehicleRoutePlans { get; set; }
        //public DbSet<PulzeOrderPlan> PulzeOrderPlans { get; set; }
        //public DbSet<PulzePlanDroppedOrders> PulzePlanDroppedOrders { get; set; }
        //public DbSet<PulzePlanDroppedVehicles> PulzePlanDroppedVehicles { get; set; }
        //public DbSet<PVRPSetup> PVRPSetups { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        //public DbSet<Item> Items { get; set; }
        //public DbSet<ItemOrderSlotTypes> ItemOrderSlotTypes { get; set; }
        // public DbSet<ItemPricing> ItemPricings { get; set; }

        //public DbSet<UserItem> UserItems { get; set; }
        //public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<DriverActivity> DriverActivities { get; set; }
        //public DbSet<CloudConfSync> CloudConfSync { get; set; }
        //public DbSet<PaymentMode> PaymentModes { get; set; }
        //public DbSet<SmsLog> SmsLogs { get; set; }
        //public DbSet<Settings> Settings { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        //public DbSet<DigitalSV> DigitalSVs { get; set; }
        //public DbSet<SenselGetVehicleDetails> SenselGetVehicleDetails { get; set; }
        //public DbSet<Invoice> Invoices { get; set; }
        //public DbSet<Pincode> Pincodes { get; set; }
        //public DbSet<BroadcastDriver> BroadcastDrivers { get; set; }
        //public DbSet<OrderRejection> OrderRejections { get; set; }
        public DbSet<AddressNotInService> AddressNotInService { get; set; }
        //public DbSet<OrderPayment> OrderPayments { get; set; }
        //public DbSet<OrderPaymentTransfer> OrderPaymentTransfers { get; set; }
        //
        public DbSet<NotificationSettings> NotificationSettings { get; set; }

        public DbSet<NotificationTemplate> CustomerNotificationTemplates { get; set; }

        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<AppImage> AppImages { get; set; }

        public DbSet<Device> Devices { get; set; }
    }
}
