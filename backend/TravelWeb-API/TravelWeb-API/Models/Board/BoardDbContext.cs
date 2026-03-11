using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models.Board.DbSet;

namespace TravelWeb_API.Models.Board;

public partial class BoardDbContext : DbContext
{
    public BoardDbContext()
    {
    }

    public BoardDbContext(DbContextOptions<BoardDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Article> Articles { get; set; }

    public virtual DbSet<ArticleFolder> ArticleFolders { get; set; }

    public virtual DbSet<ArticleLike> ArticleLikes { get; set; }

    public virtual DbSet<ArticleSource> ArticleSources { get; set; }

    public virtual DbSet<ArticleTag> ArticleTags { get; set; }

    public virtual DbSet<AuditNote> AuditNotes { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<CommentLike> CommentLikes { get; set; }

    public virtual DbSet<CommentPhoto> CommentPhotos { get; set; }

    public virtual DbSet<Journal> Journals { get; set; }

    public virtual DbSet<JournalElement> JournalElements { get; set; }

    public virtual DbSet<JournalPage> JournalPages { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PostPhoto> PostPhotos { get; set; }

    public virtual DbSet<ReportLog> ReportLogs { get; set; }

    public virtual DbSet<TagsList> TagsLists { get; set; }

    public virtual DbSet<UserActivityLog> UserActivityLogs { get; set; }

    public virtual DbSet<UserSearchHistory> UserSearchHistories { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=.;Database=Travel;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>(entity =>
        {
            entity.ToTable("Article", "Board");

            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(30);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("UserID");
        });

        modelBuilder.Entity<ArticleFolder>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ArticleFolder", "Board");

            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("UserID");

            entity.HasOne(d => d.Article).WithMany()
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArticleFolder_Article");
        });

        modelBuilder.Entity<ArticleLike>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ArticleLike", "Board");

            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Article).WithMany()
                .HasForeignKey(d => d.ArticleId)
                .HasConstraintName("FK_ArticleLike_Article");
        });

        modelBuilder.Entity<ArticleSource>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ArticleSources", "Board");

            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");

            entity.HasOne(d => d.Article).WithMany()
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArticleSources_Article");
        });

        modelBuilder.Entity<ArticleTag>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ArticleTags", "Board");

            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.TagId).HasColumnName("TagID");

            entity.HasOne(d => d.Article).WithMany()
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArticleTags_Article");

            entity.HasOne(d => d.Tag).WithMany()
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArticleTags_TagsList");
        });

        modelBuilder.Entity<AuditNote>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("AuditNote", "Board");

            entity.Property(e => e.Note).HasMaxLength(100);
            entity.Property(e => e.TargetId).HasColumnName("TargetID");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId);

            entity.ToTable("Comment", "Board");

            entity.Property(e => e.CommentId)
                .HasColumnName("CommentID");
            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.Contents).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ParentId).HasColumnName("ParentID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Article).WithMany(p => p.Comments)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comment_Article");
        });

        modelBuilder.Entity<CommentLike>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("CommentLike", "Board");

            entity.Property(e => e.CommentId).HasColumnName("CommentID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Comment).WithMany()
                .HasForeignKey(d => d.CommentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommentLike_Comment");
        });

        modelBuilder.Entity<CommentPhoto>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("CommentPhotos", "Board");

            entity.Property(e => e.CommentId).HasColumnName("CommentID");

            entity.HasOne(d => d.Comment).WithMany()
                .HasForeignKey(d => d.CommentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CommentPhotos_Comment");
        });

        modelBuilder.Entity<Journal>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Journal", "Board");

            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.CoverId).HasColumnName("CoverID");
            entity.Property(e => e.TemplateId).HasColumnName("TemplateID");

            entity.HasOne(d => d.Article).WithMany()
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Journal_Article");
        });

        modelBuilder.Entity<JournalElement>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("JournalElements", "Board");

            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.ElementId).HasColumnName("ElementID");
            entity.Property(e => e.Zindex).HasColumnName("ZIndex");

            entity.HasOne(d => d.Element).WithMany()
                .HasForeignKey(d => d.ElementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JournalElements_Article");
        });

        modelBuilder.Entity<JournalPage>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("JournalPage", "Board");

            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.Date).HasDefaultValue(1);
            entity.Property(e => e.RegionId).HasColumnName("RegionID");

            entity.HasOne(d => d.Article).WithMany()
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JournalPage_Article");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity
                //.HasNoKey()
                .ToTable("Post", "Board");

            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.Contents).HasMaxLength(500);
            entity.Property(e => e.RegionId).HasColumnName("RegionID");

            entity.HasOne(d => d.Article).WithMany()
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Post_Article");
        });

        modelBuilder.Entity<PostPhoto>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("PostPhotos", "Board");

            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");

            entity.HasOne(d => d.Article).WithMany()
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PostPhotos_Article");
        });

        modelBuilder.Entity<ReportLog>(entity =>
        {
            entity.HasKey(e => e.LogId);

            entity.ToTable("ReportLog", "Board");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Reason).HasMaxLength(100);
            entity.Property(e => e.TargetId).HasColumnName("TargetID");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("UserID");
        });

        modelBuilder.Entity<TagsList>(entity =>
        {
            entity.HasKey(e => e.TagId);

            entity.ToTable("TagsList", "Board");

            entity.HasIndex(e => e.TagName, "IX_TagsList").IsUnique();

            entity.Property(e => e.TagId)
                .ValueGeneratedNever()
                .HasColumnName("TagID");
            entity.Property(e => e.TagName).HasMaxLength(10);
        });

        modelBuilder.Entity<UserActivityLog>(entity =>
        {
            entity.HasKey(e => e.LogId);

            entity.ToTable("UserActivityLog", "Board");

            entity.Property(e => e.LogId)
                .ValueGeneratedNever()
                .HasColumnName("LogID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TargetId).HasColumnName("TargetID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<UserSearchHistory>(entity =>
        {
            entity.HasKey(e => e.SearchId);

            entity.ToTable("UserSearchHistory", "Board");

            entity.Property(e => e.SearchId)
                .ValueGeneratedNever()
                .HasColumnName("SearchID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Keywords).HasMaxLength(50);
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
