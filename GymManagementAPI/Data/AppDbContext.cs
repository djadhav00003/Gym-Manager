using GymManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace GymManagementAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Gym> Gyms { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<GymWithTrainerCountDto> GymTrainerCounts { get; set; }

        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<GymImages> GymImages { get; set; }
        public DbSet<PaymentGatewayOrder> PaymentGatewayOrders { get; set; }
        public DbSet<PaymentWebhookLog> PaymentWebhookLogs { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Gym → Trainers (Cascade)
            modelBuilder.Entity<Trainer>()
                .HasOne(t => t.Gym)
                .WithMany(g => g.Trainers)
                .HasForeignKey(t => t.GymId)
                .OnDelete(DeleteBehavior.Cascade);

            // Gym → Plans (Restrict)
            modelBuilder.Entity<Plan>()
                .HasOne(p => p.Gym)
                .WithMany(g => g.Plans)
                .HasForeignKey(p => p.GymId)
                .OnDelete(DeleteBehavior.Restrict);

            // Plan → Trainer (SetNull)
            modelBuilder.Entity<Plan>()
                .HasOne(p => p.Trainer)
                .WithMany(t => t.Plans)
                .HasForeignKey(p => p.TrainerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Member → Gym, Trainer, Plan, User (Restrict)
            modelBuilder.Entity<Member>()
                .HasOne(m => m.Gym)
                .WithMany(g => g.Members)
                .HasForeignKey(m => m.GymId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Member>()
                .HasOne(m => m.Trainer)
                .WithMany(t => t.Members)
                .HasForeignKey(m => m.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Member>()
                .HasOne(m => m.Plan)
                .WithMany(p => p.Members)
                .HasForeignKey(m => m.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Member>()
     .HasOne(m => m.User)
     .WithMany(u => u.Members)
     .HasForeignKey(m => m.UserId)
     .OnDelete(DeleteBehavior.Restrict);

            // Payments
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(m => m.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Plan)
                .WithMany(pl => pl.Payments)
                .HasForeignKey(p => p.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserRefreshTokens
            modelBuilder.Entity<UserRefreshToken>()
    .HasOne(rt => rt.User)           // Each token has one user
    .WithMany(u => u.RefreshTokens)  // One user has many tokens
    .HasForeignKey(rt => rt.UserId)  // FK property in UserRefreshToken
    .OnDelete(DeleteBehavior.Cascade);

            // 🧮 Decimal precision fixes
            modelBuilder.Entity<Plan>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Gym>()
     .HasMany(g => g.GymImages)
     .WithOne(i => i.Gym)
     .HasForeignKey(i => i.GymId)
     .OnDelete(DeleteBehavior.Cascade);

            // Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.Amount).HasPrecision(18, 2);
                entity.Property(p => p.Currency).HasMaxLength(10);
                entity.Property(p => p.PaymentStatus).HasMaxLength(50);
                entity.Property(p => p.OrderId).HasMaxLength(200);
                entity.Property(p => p.TransactionId).HasMaxLength(200);

                entity.HasIndex(p => p.OrderId).HasDatabaseName("IX_Payments_OrderId");
                entity.HasIndex(p => p.TransactionId).HasDatabaseName("IX_Payments_TransactionId");

                // Member navigation (depends on your Member config; keep Restrict or Cascade as desired)
                entity.HasOne(p => p.User)
                      .WithMany(m => m.Payments)   // assume Member has ICollection<Payment> Payments
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Plan nav
                entity.HasOne(p => p.Plan)
                      .WithMany(pl => pl.Payments) // assume Plan has ICollection<Payment> Payments
                      .HasForeignKey(p => p.PlanId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 1:1 with gateway order (optional)
                entity.HasOne(p => p.PaymentGatewayOrder)
                      .WithOne(o => o.Payment)
                      .HasForeignKey<PaymentGatewayOrder>(o => o.PaymentId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // PaymentGatewayOrder
            modelBuilder.Entity<PaymentGatewayOrder>(entity =>
            {
                entity.HasIndex(o => o.OrderId).IsUnique().HasDatabaseName("UX_PaymentGatewayOrder_OrderId");
                entity.Property(o => o.OrderId).HasMaxLength(200);
                entity.Property(o => o.Currency).HasMaxLength(10);
                entity.Property(o => o.Amount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<PaymentWebhookLog>(eb =>
            {
                eb.HasKey(e => e.Id);
                // make WebhookKey nullable but indexed unique (ignore nulls in app logic)
                eb.HasIndex(e => e.WebhookKey).IsUnique();
                eb.Property(e => e.WebhookKey).HasMaxLength(200);
            });
        }


    }

        }

    
