using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using TravelWeb_API.Models.MemberSystem;

namespace TravelWeb_API.Models.Board.DbSet;

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

    public virtual DbSet<MemberInformation> MemberInformations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //=> optionsBuilder.UseSqlServer("Server=.;Database=Travel;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {      

        modelBuilder.Entity<MemberInformation>(entity =>
        {
            // 1. 指定主鍵
            entity.ToTable("Member_Information", "Member");
            entity.HasKey(e => e.MemberId);  
            entity.Property(e => e.MemberId)
                .HasMaxLength(50)            
                .HasColumnName("MemberID");

            // 2. 修正資料表名稱與 Schema            
            entity.ToTable("Member_Information", "Member");

            // 3. 忽略你不想管的關聯 (避免連鎖報錯)
            entity.Ignore(e => e.MemberComplaints);            
        });

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

            entity.HasOne(d => d.MemberInformation)
        .WithMany()
        .HasForeignKey(d => d.UserId)
        .HasPrincipalKey(m => m.MemberId)
        .OnDelete(DeleteBehavior.ClientSetNull)
        .HasConstraintName("FK_Article_Member_Information");

        });

        modelBuilder.Entity<ArticleFolder>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ArticleId });

            entity.ToTable("ArticleFolder", "Board");

            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("UserID");
            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");

            entity.HasOne(d => d.Article).WithMany(p => p.ArticleFolders)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArticleFolder_Article");
        });

        modelBuilder.Entity<ArticleLike>(entity =>
        {
            entity.HasKey(e => new { e.ArticleId, e.UserId });
            entity.ToTable("ArticleLike", "Board");

            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Article).WithMany(a => a.ArticleLikes)
                .HasForeignKey(d => d.ArticleId)
                .HasConstraintName("FK_ArticleLike_Article");
            entity.HasOne(d => d.MemberInformation).WithMany()
                  .HasForeignKey(d => d.UserId)
                  .HasPrincipalKey(m => m.MemberId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_ArticleLike_Member_Information");
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
            entity.HasKey(e => new { e.ArticleId, e.TagId });
            entity.ToTable("ArticleTags", "Board");

            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.TagId).HasColumnName("TagID");

            entity.HasOne(d => d.Article).WithMany(a => a.ArticleTags)
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
            entity.ToTable("Comment", "Board");

            entity.Property(e => e.CommentId).HasColumnName("CommentID");
            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.Contents).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ParentId).HasColumnName("ParentID");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("UserID");

            entity.HasOne(d => d.Article).WithMany(p => p.Comments)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comment_Article");

            // 自我引用配置
            entity.HasOne(d => d.Parent)                  // 每個子留言有一個「母物件」
                .WithMany(p => p.InverseParent)           // 每個母留言有多個「子物件」
                .HasForeignKey(d => d.ParentId)           // 外鍵是 ParentID
                .OnDelete(DeleteBehavior.ClientSetNull)   // 限制刪除行為
                .HasConstraintName("FK_Comment_Comment"); // 與資料庫一致的約束名稱           

            entity.HasOne(d => d.MemberInformation)          // Comment 有一個 MemberInformation
        .WithMany()                                  // 如果 Member 沒寫 ICollection<Comment>，這裡就留空 WithMany()
        .HasForeignKey(d => d.UserId)                // 外鍵是 Comment 裡的 UserId
        .HasPrincipalKey(m => m.MemberId)            // 指向 MemberInformation 的主鍵 (確認一下 Member 的 PK 叫什麼)
        .OnDelete(DeleteBehavior.ClientSetNull)      // 刪除行為
        .HasConstraintName("FK_Comment_Member_Information");
        });

        modelBuilder.Entity<CommentLike>(entity =>
        {
            entity.HasKey(e => new { e.CommentId, e.UserId });

            entity.ToTable("CommentLike", "Board");

            entity.Property(e => e.CommentId).HasColumnName("CommentID");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("UserID");

            entity.HasOne(d => d.Comment).WithMany(p => p.CommentLikes)
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
            entity.HasKey(e => e.ArticleId);

            entity.ToTable("Journal", "Board");

            entity.Property(e => e.ArticleId)
                .ValueGeneratedNever()
                .HasColumnName("ArticleID");
            entity.Property(e => e.CoverId).HasColumnName("CoverID");
            entity.Property(e => e.TemplateId).HasColumnName("TemplateID");

            entity.HasOne(d => d.Article).WithOne(p => p.Journal)
                .HasForeignKey<Journal>(d => d.ArticleId)
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
            entity.HasKey(e => new { e.ArticleId, e.Date });

            entity.ToTable("JournalPage", "Board");

            entity.Property(e => e.ArticleId).HasColumnName("ArticleID");
            entity.Property(e => e.Date).HasDefaultValue(1);
            entity.Property(e => e.RegionId).HasColumnName("RegionID");

            entity.HasOne(d => d.Article).WithMany(p => p.JournalPages)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JournalPage_Article");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.ArticleId);

            entity.ToTable("Post", "Board");

            entity.Property(e => e.ArticleId)
                .ValueGeneratedNever()
                .HasColumnName("ArticleID");
            entity.Property(e => e.Contents).HasMaxLength(500);
            entity.Property(e => e.RegionId).HasColumnName("RegionID");

            entity.HasOne(d => d.Article).WithOne(p => p.Post)
                .HasForeignKey<Post>(d => d.ArticleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Post_Article");
        });

        modelBuilder.Entity<PostPhoto>(entity =>
        {
            entity.HasKey(e => e.ID);
            entity.ToTable("PostPhotos", "Board");

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
