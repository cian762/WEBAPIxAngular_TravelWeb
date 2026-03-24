using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TravelWeb_API.Models.ActivityModel;

public partial class ActivityDbContext : DbContext
{
    public ActivityDbContext()
    {
    }

    public ActivityDbContext(DbContextOptions<ActivityDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AcitivityTicket> AcitivityTickets { get; set; }

    public virtual DbSet<Activity> Activities { get; set; }

    public virtual DbSet<ActivityAnalytic> ActivityAnalytics { get; set; }

    public virtual DbSet<ActivityEditLog> ActivityEditLogs { get; set; }

    public virtual DbSet<ActivityImage> ActivityImages { get; set; }

    public virtual DbSet<ActivityNotification> ActivityNotifications { get; set; }

    public virtual DbSet<ActivityPublishStatus> ActivityPublishStatuses { get; set; }

    public virtual DbSet<ActivityTicketDetail> ActivityTicketDetails { get; set; }

    public virtual DbSet<ActivityTicketDiscount> ActivityTicketDiscounts { get; set; }

    public virtual DbSet<PersonalizedRecommendation> PersonalizedRecommendations { get; set; }

    public virtual DbSet<ProductReview> ProductReviews { get; set; }

    public virtual DbSet<ReviewImage> ReviewImages { get; set; }

    public virtual DbSet<TagsActivityType> TagsActivityTypes { get; set; }

    public virtual DbSet<TagsRegion> TagsRegions { get; set; }

    public virtual DbSet<TicketCategory> TicketCategories { get; set; }

    public virtual DbSet<UserFavorite> UserFavorites { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcitivityTicket>(entity =>
        {
            entity.HasKey(e => e.ProductCode).HasName("PK_商品代碼總表");

            entity.ToTable("Acitivity_Tickets", "Activity");

            entity.Property(e => e.ProductCode).HasMaxLength(50);
            entity.Property(e => e.ProductName).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(10);
            entity.Property(e => e.TicketCategoryId).HasColumnName("TicketCategoryID");

            entity.HasOne(d => d.TicketCategory).WithMany(p => p.AcitivityTickets)
                .HasForeignKey(d => d.TicketCategoryId)
                .HasConstraintName("FK_Acitivity_Tickets_TicketCategories");

            entity.HasMany(d => d.Discounts).WithMany(p => p.ProductCodes)
                .UsingEntity<Dictionary<string, object>>(
                    "ActivityTicketDiscount1",
                    r => r.HasOne<ActivityTicketDiscount>().WithMany()
                        .HasForeignKey("DiscountId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_商品_折扣對照表_商品折扣表"),
                    l => l.HasOne<AcitivityTicket>().WithMany()
                        .HasForeignKey("ProductCode")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_商品_折扣對照表_商品代碼總表"),
                    j =>
                    {
                        j.HasKey("ProductCode", "DiscountId").HasName("PK_商品_折扣對照表");
                        j.ToTable("ActivityTicket_Discount", "Activity");
                        j.IndexerProperty<string>("ProductCode").HasMaxLength(50);
                        j.IndexerProperty<int>("DiscountId").HasColumnName("DiscountID");
                    });
        });

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.ActivityId).HasName("PK_活動表_1");

            entity.ToTable("Activities", "Activity");

            entity.Property(e => e.ActivityId).HasColumnName("ActivityID");
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Title).HasMaxLength(50);
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");

            entity.HasMany(d => d.Regions).WithMany(p => p.Activities)
                .UsingEntity<Dictionary<string, object>>(
                    "ActivityRegion",
                    r => r.HasOne<TagsRegion>().WithMany()
                        .HasForeignKey("RegionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_活動標籤化-區域_標籤_區域表"),
                    l => l.HasOne<Activity>().WithMany()
                        .HasForeignKey("ActivityId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_活動標籤化-區域_活動表"),
                    j =>
                    {
                        j.HasKey("ActivityId", "RegionId").HasName("PK_活動標籤化_區域");
                        j.ToTable("Activity_Region", "Activity");
                        j.IndexerProperty<int>("ActivityId").HasColumnName("ActivityID");
                        j.IndexerProperty<int>("RegionId").HasColumnName("RegionID");
                    });

            entity.HasMany(d => d.Reviews).WithMany(p => p.Activities)
                .UsingEntity<Dictionary<string, object>>(
                    "ReviewActivityRelation",
                    r => r.HasOne<ProductReview>().WithMany()
                        .HasForeignKey("ReviewId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ReviewID_ReviewActivity_Activities"),
                    l => l.HasOne<Activity>().WithMany()
                        .HasForeignKey("ActivityId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ActivityID_ReviewActivity_Activities"),
                    j =>
                    {
                        j.HasKey("ActivityId", "ReviewId").HasName("PK_ReviewActivity");
                        j.ToTable("ReviewActivityRelation", "Activity");
                        j.IndexerProperty<int>("ActivityId").HasColumnName("ActivityID");
                        j.IndexerProperty<int>("ReviewId").HasColumnName("ReviewID");
                    });

            entity.HasMany(d => d.Types).WithMany(p => p.Activities)
                .UsingEntity<Dictionary<string, object>>(
                    "ActivityActivityType",
                    r => r.HasOne<TagsActivityType>().WithMany()
                        .HasForeignKey("TypeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_活動標籤化-類型_標籤_活動類型表"),
                    l => l.HasOne<Activity>().WithMany()
                        .HasForeignKey("ActivityId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_活動標籤化-類型_活動表"),
                    j =>
                    {
                        j.HasKey("ActivityId", "TypeId").HasName("PK_活動標籤化_類型");
                        j.ToTable("Activity_ActivityTypes", "Activity");
                        j.IndexerProperty<int>("ActivityId").HasColumnName("ActivityID");
                        j.IndexerProperty<int>("TypeId").HasColumnName("TypeID");
                    });
        });

        modelBuilder.Entity<ActivityAnalytic>(entity =>
        {
            entity.HasKey(e => e.ActivityId).HasName("PK_活動熱度分析_1");

            entity.ToTable("ActivityAnalytics", "Activity");

            entity.Property(e => e.ActivityId)
                .ValueGeneratedNever()
                .HasColumnName("ActivityID");
            entity.Property(e => e.Rating).HasColumnType("decimal(5, 4)");

            entity.HasOne(d => d.Activity).WithOne(p => p.ActivityAnalytic)
                .HasForeignKey<ActivityAnalytic>(d => d.ActivityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ActivityAnalytics_Activities");
        });

        modelBuilder.Entity<ActivityEditLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK_活動編輯紀錄_1");

            entity.ToTable("ActivityEditLogs", "Activity");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.Action).HasMaxLength(10);
            entity.Property(e => e.ActivityId).HasColumnName("ActivityID");
            entity.Property(e => e.AdminId).HasColumnName("AdminID");
            entity.Property(e => e.EditDate).HasColumnType("datetime");

            entity.HasOne(d => d.Activity).WithMany(p => p.ActivityEditLogs)
                .HasForeignKey(d => d.ActivityId)
                .HasConstraintName("FK_活動編輯紀錄_活動表1");
        });

        modelBuilder.Entity<ActivityImage>(entity =>
        {
            entity.HasKey(e => e.ImageSetId).HasName("PK_活動圖片表");

            entity.ToTable("ActivityImages", "Activity");

            entity.Property(e => e.ImageSetId).HasColumnName("ImageSetID");
            entity.Property(e => e.ActivityId).HasColumnName("ActivityID");

            entity.HasOne(d => d.Activity).WithMany(p => p.ActivityImages)
                .HasForeignKey(d => d.ActivityId)
                .HasConstraintName("FK_活動圖片表_活動表");
        });

        modelBuilder.Entity<ActivityNotification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK_活動通知_1");

            entity.ToTable("ActivityNotification", "Activity");

            entity.Property(e => e.NotificationId).HasColumnName("NotificationID");
            entity.Property(e => e.ActivityId).HasColumnName("ActivityID");
            entity.Property(e => e.NotificaitonType).HasMaxLength(10);
            entity.Property(e => e.SendStatus).HasMaxLength(10);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Activity).WithMany(p => p.ActivityNotifications)
                .HasForeignKey(d => d.ActivityId)
                .HasConstraintName("FK_活動通知_活動表1");
        });

        modelBuilder.Entity<ActivityPublishStatus>(entity =>
        {
            entity.HasKey(e => e.ActivityId).HasName("PK_自動化刊登/下架_1");

            entity.ToTable("ActivityPublishStatus", "Activity");

            entity.Property(e => e.ActivityId)
                .ValueGeneratedNever()
                .HasColumnName("ActivityID");
            entity.Property(e => e.PublishTime).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(10);
            entity.Property(e => e.UnPublishTime).HasColumnType("datetime");

            entity.HasOne(d => d.Activity).WithOne(p => p.ActivityPublishStatus)
                .HasForeignKey<ActivityPublishStatus>(d => d.ActivityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ActivityPublishStatus_Activities");
        });

        modelBuilder.Entity<ActivityTicketDetail>(entity =>
        {
            entity.HasKey(e => e.ProductCode).HasName("PK_活動商品表細節");

            entity.ToTable("Activity_TicketDetails", "Activity");

            entity.Property(e => e.ProductCode).HasMaxLength(50);
            entity.Property(e => e.ActivityId).HasColumnName("ActivityID");

            entity.HasOne(d => d.Activity).WithMany(p => p.ActivityTicketDetails)
                .HasForeignKey(d => d.ActivityId)
                .HasConstraintName("FK_Activity_TicketDetails_Activities");

            entity.HasOne(d => d.ProductCodeNavigation).WithOne(p => p.ActivityTicketDetail)
                .HasForeignKey<ActivityTicketDetail>(d => d.ProductCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Activity_TicketDetails_Acitivity_Tickets");
        });

        modelBuilder.Entity<ActivityTicketDiscount>(entity =>
        {
            entity.HasKey(e => e.DiscountId).HasName("PK_商品折扣表");

            entity.ToTable("Activity_TicketDiscounts", "Activity");

            entity.Property(e => e.DiscountId).HasColumnName("DiscountID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<PersonalizedRecommendation>(entity =>
        {
            entity.HasKey(e => e.RecommendId);

            entity.ToTable("PersonalizedRecommendations", "Activity");

            entity.Property(e => e.RecommendId).HasColumnName("RecommendID");
            entity.Property(e => e.ActivityId).HasColumnName("ActivityID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.PreferenceBehavior).HasMaxLength(10);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Activity).WithMany(p => p.PersonalizedRecommendations)
                .HasForeignKey(d => d.ActivityId)
                .HasConstraintName("FK_個人化推薦_活動表");
        });

        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__ProductR__74BC79AE4C5EFFD1");

            entity.ToTable("ProductReview", "Activity");

            entity.Property(e => e.ReviewId).HasColumnName("ReviewID");
            entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.MemberId)
                .HasMaxLength(50)
                .HasColumnName("MemberID");
            entity.Property(e => e.Title).HasMaxLength(50);
        });

        modelBuilder.Entity<ReviewImage>(entity =>
        {
            entity.HasKey(e => e.ImageSetId).HasName("PK__ReviewIm__34D3A22BCC50099A");

            entity.ToTable("ReviewImage", "Activity");

            entity.Property(e => e.ImageSetId).HasColumnName("ImageSetID");
            entity.Property(e => e.PublicId).HasColumnName("PublicID");
            entity.Property(e => e.ReviewId).HasColumnName("ReviewID");

            entity.HasOne(d => d.Review).WithMany(p => p.ReviewImages)
                .HasForeignKey(d => d.ReviewId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReviewID_ProductReview_ReviewImage");
        });

        modelBuilder.Entity<TagsActivityType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PK_標籤_活動類型表_1");

            entity.ToTable("Tags_ActivityTypes", "Activity");

            entity.Property(e => e.TypeId).HasColumnName("TypeID");
            entity.Property(e => e.ActivityType).HasMaxLength(50);
        });

        modelBuilder.Entity<TagsRegion>(entity =>
        {
            entity.HasKey(e => e.RegionId).HasName("PK_標籤_區域表_1");

            entity.ToTable("Tags_Regions", "Activity");

            entity.Property(e => e.RegionId)
                .ValueGeneratedNever()
                .HasColumnName("RegionID");
            entity.Property(e => e.RegionName).HasMaxLength(10);
            entity.Property(e => e.Uid).HasColumnName("UID");

            entity.HasOne(d => d.UidNavigation).WithMany(p => p.InverseUidNavigation)
                .HasForeignKey(d => d.Uid)
                .HasConstraintName("FK_Tags_Regions_Tags_Regions");
        });

        modelBuilder.Entity<TicketCategory>(entity =>
        {
            entity.HasKey(e => e.TicketCategoryId).HasName("PK__TicketCa__C84589E67C04231C");

            entity.ToTable("TicketCategories", "product");

            entity.Property(e => e.CategoryName).HasMaxLength(50);
        });

        modelBuilder.Entity<UserFavorite>(entity =>
        {
            entity.HasKey(e => e.FavoriteId).HasName("PK_喜好收藏");

            entity.ToTable("UserFavorites", "Activity");

            entity.Property(e => e.FavoriteId).HasColumnName("FavoriteID");
            entity.Property(e => e.ActivityId).HasColumnName("ActivityID");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("UserID");

            entity.HasOne(d => d.Activity).WithMany(p => p.UserFavorites)
                .HasForeignKey(d => d.ActivityId)
                .HasConstraintName("FK_喜好收藏_活動表1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
