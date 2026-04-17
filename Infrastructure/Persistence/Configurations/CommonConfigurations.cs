using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ClothConfiguration : IEntityTypeConfiguration<Cloth>
{
    public void Configure(EntityTypeBuilder<Cloth> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Brand).HasMaxLength(100);
        builder.Property(c => c.PhotoUrl).HasMaxLength(2048);
        builder.Property(c => c.StoreLink).HasMaxLength(2048);
        builder.Property(c => c.EstimatedPrice).HasPrecision(18, 2);
    }
}

public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).HasMaxLength(50).IsRequired();
        builder.Property(t => t.Category).HasMaxLength(50).IsRequired();
        builder.HasIndex(t => t.Name).IsUnique();
        builder.HasIndex(t => t.Category);
    }
}

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Text).HasMaxLength(1000).IsRequired();
        
        // Убираем каскад от пользователя, оставляем только от публикации
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(c => c.Replies).Metadata.SetField("_replies");
    }
}

public sealed class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.HasKey(a => a.Id);
        
        builder.HasIndex(a => new { a.UserId, a.PublicationId }).IsUnique();

        // Убираем каскад от пользователя, оставляем только от публикации
        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        builder.HasKey(f => new { f.FollowerId, f.FolloweeId });

        builder.HasOne(f => f.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Followee)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FolloweeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class LikeConfiguration : IEntityTypeConfiguration<Like>
{
    public void Configure(EntityTypeBuilder<Like> builder)
    {
        builder.HasKey(l => l.Id);

        // Для лайков отключаем все каскады, чтобы не было конфликтов
        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Publication)
            .WithMany()
            .HasForeignKey(l => l.PublicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Comment)
            .WithMany()
            .HasForeignKey(l => l.CommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => new { l.UserId, l.PublicationId })
            .HasFilter("[PublicationId] IS NOT NULL")
            .IsUnique();

        builder.HasIndex(l => new { l.UserId, l.CommentId })
            .HasFilter("[CommentId] IS NOT NULL")
            .IsUnique();
    }
}
