using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Interfaces;

public interface IApplicationDbContext
{
    DatabaseFacade Database { get; }
    DbSet<User> Users { get; }
    DbSet<Publication> Publications { get; }
    DbSet<Cloth> Clothes { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Assessment> Assessments { get; }
    DbSet<Follow> Follows { get; }
    DbSet<CommentLike> CommentLikes { get; }
    DbSet<Collection> Collections { get; }
    DbSet<Report> Reports { get; }
    DbSet<Advertisement> Advertisements { get; }
    DbSet<AdvertisementView> AdvertisementViews { get; }
    DbSet<PublicationGameStats> GameStats { get; }
    DbSet<UserGameCursor> UserGameCursors { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
