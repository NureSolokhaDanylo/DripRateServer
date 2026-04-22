using Domain;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Publication> Publications { get; }
    DbSet<Cloth> Clothes { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Assessment> Assessments { get; }
    DbSet<Follow> Follows { get; }
    DbSet<Like> Likes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
