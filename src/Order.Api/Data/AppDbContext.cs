using Microsoft.EntityFrameworkCore;
using Order.Api.Data.Entities;

namespace Order.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<OrderEntity> Orders { get; set; }
        public DbSet<OutboxMessageEntity> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderEntity>(entity =>
            {
                entity.ToTable("Orders");
                
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.CustomerId)
                    .IsRequired();

                entity.Property(e => e.TotalValue)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.CreationDate)
                    .IsRequired();
            });

            modelBuilder.Entity<OutboxMessageEntity>(entity =>
            {
                entity.ToTable("OutboxMessages");
                
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Type)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.Payload)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.ProcessedAt);

                entity.Property(e => e.Error);

                entity.Property(e => e.RetryCount)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.HasIndex(e => new { e.ProcessedAt, e.CreatedAt });
            });
        }
    }
}
