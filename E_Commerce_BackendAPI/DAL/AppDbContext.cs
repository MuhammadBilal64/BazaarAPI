using E_Commerce_BackendAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_BackendAPI.DAL
{
    public class AppDbContext:DbContext
    {
    public    AppDbContext(DbContextOptions<AppDbContext>options):base(options)
        {
        }
     public   DbSet<User> Users { get; set; }
       public DbSet<Product> Products { get; set; }
 public       DbSet<Category> Categories { get; set; }
      public  DbSet<Order> Orders { get; set; }
     public   DbSet<OrderItem> OrderItems { get; set; }
      public  DbSet<CartItem> CartItems { get; set; }
       public  DbSet<RefreshToken> RefreshTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().Property(u=>u.Role).HasConversion<string>();
            modelBuilder.Entity<CartItem>().Property(c=>c.Status).HasConversion<string>();
            modelBuilder.Entity<Order>().Property(o=>o.Status).HasConversion<string>();

            modelBuilder.Entity<Category>().HasMany(c=>c.Products)
                .WithOne(p=>p.Category).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
        .HasMany(u => u.Orders)
        .WithOne(o => o.User)
        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(c=>c.CartItems).WithOne(u=>u.User).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
         .HasMany(o => o.OrderItems)
         .WithOne(oi => oi.Order)
         .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>().HasMany(u => u.RefreshTokens).WithOne(rt => rt.User).HasForeignKey(rt => rt.UserId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
    .Property(p => p.Price)
    .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)");
        }
       


    }
}
