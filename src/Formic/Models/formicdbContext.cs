using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;

namespace Formic
{
    public partial class FormicdbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(@"Data Source=(localdb)\mssqllocaldb;Initial Catalog=formicdb;Integrated Security=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.HasKey(e => e.PK);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("varchar");
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Content).HasColumnType("varchar");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("varchar");

                entity.HasOne(d => d.Blog).WithMany(p => p.Post).HasForeignKey(d => d.BlogPK).OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Author).WithMany(p => p.Posts).HasForeignKey(p => p.AuthorFK);
            });

            modelBuilder.Entity<Author>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Name)
                    .IsRequired()
                    .HasColumnType("varchar")
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.TagName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("varchar");
            });

            modelBuilder.Entity<PostTag>(entity =>
            {
                entity.HasKey(t => new { t.PostId, t.TagId });

                entity.HasOne(pt => pt.Post)
                    .WithMany(p => p.PostTags)
                    .HasForeignKey(pt => pt.PostId);

                entity.HasOne(pt => pt.Tag)
                    .WithMany(t => t.PostTags)
                    .HasForeignKey(pt => pt.TagId);
            });

        }

        public virtual DbSet<Blog> Blog { get; set; }
        public virtual DbSet<Post> Post { get; set; }
        public virtual DbSet<Tag> Tag { get; set; }

        // Unable to generate entity type for table 'dbo.PostTags'. Please see the warning messages.
    }
}