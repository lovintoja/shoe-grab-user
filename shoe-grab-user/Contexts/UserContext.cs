using Microsoft.EntityFrameworkCore;

namespace ShoeGrabCommonModels.Contexts;

public class UserContext : DbContext
{
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserProfile> Profiles { get; set; }

    public UserContext(DbContextOptions<UserContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Profile);
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasMaxLength(50);
        modelBuilder.Entity<UserProfile>()
            .HasOne(u => u.User);
    }
}
