using Microsoft.EntityFrameworkCore;
using TrialWorld.Persistence.Entities;

namespace TrialWorld.Persistence.Data
{
    /// <summary>
    /// Entity Framework DbContext for the application.
    /// Uses dedicated Entity classes from TrialWorld.Persistence.Entities.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Gets or sets the media metadata entities.
        /// </summary>
        public DbSet<MediaMetadataEntity> MediaMetadataEntities { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the transcript segment entities.
        /// </summary>
        public DbSet<TranscriptSegmentEntity> TranscriptSegmentEntities { get; set; } = null!;
        
        /// <summary>
        /// Gets or sets the search feedback entities.
        /// </summary>
        public DbSet<SearchFeedbackEntity> SearchFeedbackEntities { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configures the entity relationships and constraints when the model is created.
        /// </summary>
        /// <param name="modelBuilder">The model builder instance.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity mappings

            modelBuilder.Entity<MediaMetadataEntity>(entity =>
            {
                entity.ToTable("MediaMetadata");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FilePath).IsRequired();
                entity.HasIndex(e => e.FilePath).IsUnique();
                entity.Property(e => e.Status).HasConversion<string>(); 
                entity.HasMany<TranscriptSegmentEntity>()
                      .WithOne()
                      .HasForeignKey(t => t.MediaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TranscriptSegmentEntity>(entity =>
            {
                entity.ToTable("TranscriptSegments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).IsRequired();
                entity.HasIndex(e => e.MediaId);
            });

            // Note: Face and emotion analysis features have been removed from the application

            modelBuilder.Entity<SearchFeedbackEntity>(entity =>
            {
                entity.ToTable("SearchFeedbacks");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Query).IsRequired();
                entity.Property(e => e.Feedback).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Timestamp);
            });

        }
    }
}