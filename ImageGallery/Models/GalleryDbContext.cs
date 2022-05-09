using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GalleryDatabase.Models
{
    public class GalleryDbContext : IdentityDbContext<GalleryOwner>
    {
        public GalleryDbContext(DbContextOptions<GalleryDbContext> options) : base(options)
        {
        }

        public DbSet<Image> Images { get; set; }

        public DbSet<Thumbnail> Thumbnails { get; set; }

        public DbSet<Album> Albums { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<CommentUser> CommentUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Comment>(
                o =>
                {
                    o.HasOne(ot => ot.CommentedComment)
                    .WithMany(m => m.CommentsReactingtoThisComment)
                    .HasForeignKey(dt => dt.CommentedCommentId)
                    .OnDelete(DeleteBehavior.Restrict);

                    o.HasOne(ot => ot.Image)
                    .WithMany(pl => pl.Comments)
                    .HasForeignKey(ot => ot.ImageId)
                    .OnDelete(DeleteBehavior.Restrict);
                }
            );

            modelBuilder.Entity<CommentUser>().HasKey(sc => new { sc.UserId, sc.CommentId });

            modelBuilder.Entity<CommentUser>(
               o =>
               {
                   o.HasOne(ot => ot.Comment)
                   .WithMany(m => m.CommentUsers)
                   .HasForeignKey(dt => dt.CommentId)
                   .OnDelete(DeleteBehavior.NoAction);

                   o.HasOne(ot => ot.User)
                   .WithMany(pl => pl.CommentUsers)
                   .HasForeignKey(ot => ot.UserId)
                   .OnDelete(DeleteBehavior.NoAction);
               }
           );

            modelBuilder.Entity<Image>(
                o =>
                {
                    o.HasMany(x => x.Albums)
                    .WithOne(x => x.Cover)
                    .HasForeignKey(x => x.CoverImageId)
                    .OnDelete(DeleteBehavior.SetNull);

                    o.HasOne(x => x.Album)
                    .WithMany(x => x.Images)
                    .HasForeignKey(x => x.AlbumId)
                    .OnDelete(DeleteBehavior.NoAction);
                }

           );

            modelBuilder.Entity<Comment>().Property(x => x.ReactionScore).HasPrecision(38, 18);
            modelBuilder.Entity<Comment>().Property(x => x.ControversyScore).HasPrecision(38, 18);

        }
    }
}
