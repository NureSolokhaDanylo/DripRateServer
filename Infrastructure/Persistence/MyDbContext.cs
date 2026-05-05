using Application.Interfaces;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class MyDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public DbSet<Publication> Publications => Set<Publication>();
    public DbSet<Cloth> Clothes => Set<Cloth>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<CommentLike> CommentLikes => Set<CommentLike>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<Advertisement> Advertisements => Set<Advertisement>();
    public DbSet<AdvertisementView> AdvertisementViews => Set<AdvertisementView>();

    // Регистрация IApplicationDbContext.Users для соответствия интерфейсу, 
    // хотя он уже есть в базовом IdentityDbContext.
    public override DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(MyDbContext).Assembly);
    }
}
