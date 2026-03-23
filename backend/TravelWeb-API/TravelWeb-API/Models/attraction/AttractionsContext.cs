using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TravelWeb_API.Models.attraction;

public partial class AttractionsContext : DbContext
{
    public AttractionsContext()
    {
    }

    public AttractionsContext(DbContextOptions<AttractionsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attraction> Attractions { get; set; }

    public virtual DbSet<AttractionProduct> AttractionProducts { get; set; }

    public virtual DbSet<AttractionProductDetail> AttractionProductDetails { get; set; }

    public virtual DbSet<AttractionProductFavorite> AttractionProductFavorites { get; set; }

    public virtual DbSet<AttractionProductImage> AttractionProductImages { get; set; }  // ← 新增

    public virtual DbSet<AttractionTypeCategory> AttractionTypeCategories { get; set; }

    public virtual DbSet<AttractionTypeMapping> AttractionTypeMappings { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<ProductInventoryStatus> ProductInventoryStatuses { get; set; }

    public virtual DbSet<StockInRecord> StockInRecords { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<TagsRegion> TagsRegions { get; set; }

    public virtual DbSet<TicketType> TicketTypes { get; set; }

    public virtual DbSet<AttractionLike> AttractionLikes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attraction>(entity =>
        {
            entity.ToTable("Attractions", "Attractions");

            entity.Property(e => e.AttractionId).HasColumnName("attraction_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.ApprovalStatus)
                .HasDefaultValue(0)
                .HasColumnName("approval_status");
            entity.Property(e => e.AreaId)
                .HasMaxLength(50)
                .HasColumnName("area_id");
            entity.Property(e => e.BusinessHours)
                .HasMaxLength(500)
                .HasColumnName("business_hours");
            entity.Property(e => e.ClosedDaysNote)
                .HasMaxLength(200)
                .HasColumnName("closed_days_note");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.GooglePlaceId)
                .HasMaxLength(255)
                .HasColumnName("google_place_id");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.ViewCount)
               .HasDefaultValue(0)
               .HasColumnName("view_count");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(10, 7)")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(10, 7)")
                .HasColumnName("longitude");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.OpendataId)
                .HasMaxLength(100)
                .HasColumnName("opendata_id");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
            entity.Property(e => e.RegionId).HasColumnName("RegionID");
            entity.Property(e => e.TransportInfo).HasColumnName("transport_info");
            entity.Property(e => e.Website)
                .HasMaxLength(500)
                .HasColumnName("website");

            entity.HasOne(d => d.Region).WithMany(p => p.Attractions)
                .HasForeignKey(d => d.RegionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attractions_Tags_Regions");
        });

        modelBuilder.Entity<AttractionProduct>(entity =>
        {
            entity.HasKey(e => e.ProductId);

            entity.ToTable("AttractionProducts", "Attractions");

            entity.HasIndex(e => e.ProductCode, "UQ_AttractionProducts_ProductCode").IsUnique();

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.AttractionId).HasColumnName("attraction_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.MaxPurchaseQuantity).HasColumnName("max_purchase_quantity");
            entity.Property(e => e.PolicyId).HasColumnName("policy_id");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.OriginalPrice)          // ← 新增
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("original_price");
            entity.Property(e => e.ValidityDays)           // ← 新增
                .HasColumnName("validity_days");
            entity.Property(e => e.ProductCode)
                .HasMaxLength(50)
                .HasColumnName("product_code");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("DRAFT")
                .HasColumnName("status");
            entity.Property(e => e.TicketTypeCode).HasColumnName("ticket_type_code");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Attraction).WithMany(p => p.AttractionProducts)
                .HasForeignKey(d => d.AttractionId)
                .HasConstraintName("FK_AttractionProducts_Attractions");

            entity.HasOne(d => d.TicketTypeCodeNavigation).WithMany(p => p.AttractionProducts)
                .HasForeignKey(d => d.TicketTypeCode)
                .HasConstraintName("FK_AttractionProducts_TicketTypes");

            entity.HasMany(d => d.Tags).WithMany(p => p.Products)
                .UsingEntity<Dictionary<string, object>>(
                    "AttractionProductTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_AttractionProductTags_Tags"),
                    l => l.HasOne<AttractionProduct>().WithMany()
                        .HasForeignKey("ProductId")
                        .HasConstraintName("FK_AttractionProductTags_AttractionProducts"),
                    j =>
                    {
                        j.HasKey("ProductId", "TagId").HasName("PK__Attracti__332B17DE9A42B909");
                        j.ToTable("AttractionProductTags", "Attractions");
                        j.IndexerProperty<int>("ProductId").HasColumnName("product_id");
                        j.IndexerProperty<int>("TagId").HasColumnName("tag_id");
                    });
        });

        modelBuilder.Entity<AttractionProductDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("AttractionProductDetails", "Attractions");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ContentDetails).HasColumnName("content_details");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.UsageInstructions).HasColumnName("usage_instructions");
            entity.Property(e => e.Includes).HasColumnName("includes");           // ← 新增
            entity.Property(e => e.Excludes).HasColumnName("excludes");           // ← 新增
            entity.Property(e => e.Eligibility).HasColumnName("eligibility");     // ← 新增
            entity.Property(e => e.CancelPolicy)                                  // ← 新增
                .HasMaxLength(500)
                .HasColumnName("cancel_policy");
            entity.Property(e => e.LastUpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("last_updated_at");

            entity.HasOne(d => d.Product).WithMany()
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_AttractionProductDetails_AttractionProducts");
        });

        modelBuilder.Entity<AttractionProductFavorite>(entity =>
        {
            entity.HasKey(e => e.FavoriteId).HasName("PK__Attracti__46ACF4CBDF390037");

            entity.ToTable("AttractionProductFavorites", "Attractions");

            entity.HasIndex(e => new { e.UserId, e.ProductId }, "UQ_Favorites_UserProduct").IsUnique();

            entity.Property(e => e.FavoriteId).HasColumnName("favorite_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("user_id");

            entity.HasOne(d => d.Product).WithMany(p => p.AttractionProductFavorites)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_AttractionProductFavorites_AttractionProducts");
        });

        // ← 新增 AttractionProductImage mapping
        modelBuilder.Entity<AttractionProductImage>(entity =>
        {
            entity.HasKey(e => e.ImageId);

            entity.ToTable("AttractionProductImages", "Attractions");

            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(500)
                .HasColumnName("image_path");
            entity.Property(e => e.Caption)
                .HasMaxLength(200)
                .HasColumnName("caption");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");

            entity.HasOne(d => d.Product).WithMany(p => p.AttractionProductImages)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_AttractionProductImages_AttractionProducts");
        });

        modelBuilder.Entity<AttractionTypeCategory>(entity =>
        {
            entity.HasKey(e => e.AttractionTypeId).HasName("PK__Attracti__61CA9FF14ABB1A87");

            entity.ToTable("AttractionTypeCategories", "Attractions");

            entity.Property(e => e.AttractionTypeId).HasColumnName("attraction_type_id");
            entity.Property(e => e.AttractionTypeName)
                .HasMaxLength(50)
                .HasColumnName("attraction_type_name");
        });

        modelBuilder.Entity<AttractionTypeMapping>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("AttractionTypeMappings", "Attractions");

            entity.Property(e => e.AttractionId).HasColumnName("attraction_id");
            entity.Property(e => e.AttractionTypeId).HasColumnName("attraction_type_id");

            entity.HasOne(d => d.Attraction).WithMany()
                .HasForeignKey(d => d.AttractionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttractionTypeMappings_Attractions");

            entity.HasOne(d => d.AttractionType).WithMany()
                .HasForeignKey(d => d.AttractionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttractionTypeMappings_AttractionTypeCategories");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK_Attractions_Images");

            entity.ToTable("Images", "Attractions");

            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.AttractionId).HasColumnName("attraction_id");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .HasColumnName("image_path");

            entity.HasOne(d => d.Attraction).WithMany(p => p.Images)
                .HasForeignKey(d => d.AttractionId)
                .HasConstraintName("FK_Images_Attractions");
        });

        modelBuilder.Entity<ProductInventoryStatus>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ProductInventoryStatus", "Attractions");

            entity.Property(e => e.DailyLimit).HasColumnName("daily_limit");
            entity.Property(e => e.InventoryMode)
                .HasMaxLength(20)
                .HasDefaultValue("UNLIMITED")
                .HasColumnName("inventory_mode");
            entity.Property(e => e.LastUpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("last_updated_at");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SoldQuantity)
                .HasDefaultValue(0)
                .HasColumnName("sold_quantity");

            entity.HasOne(d => d.Product).WithMany()
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductInventoryStatus_AttractionProducts");
        });

        modelBuilder.Entity<StockInRecord>(entity =>
        {
            entity.HasKey(e => e.StockInId).HasName("PK__StockInR__F657737DB034425D");

            entity.ToTable("StockInRecords", "Attractions");

            entity.Property(e => e.StockInId).HasColumnName("stock_in_id");
            entity.Property(e => e.InventoryType)
                .HasMaxLength(20)
                .HasDefaultValue("VIRTUAL")
                .HasColumnName("inventory_type");
            entity.Property(e => e.ProductCode)
                .HasMaxLength(50)
                .HasColumnName("product_code");
            entity.Property(e => e.ProductType)
                .HasMaxLength(20)
                .HasColumnName("product_type");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.RemainingStock).HasColumnName("remaining_stock");
            entity.Property(e => e.Remarks)
                .HasMaxLength(500)
                .HasColumnName("remarks");
            entity.Property(e => e.StockInDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("stock_in_date");
            entity.Property(e => e.SupplierName)
                .HasMaxLength(100)
                .HasColumnName("supplier_name");
            entity.Property(e => e.UnitCost)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("unit_cost");

            entity.HasOne(d => d.ProductCodeNavigation).WithMany(p => p.StockInRecords)
                .HasPrincipalKey(p => p.ProductCode)
                .HasForeignKey(d => d.ProductCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockInRecords_AttractionProducts");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK__Tags__4296A2B675F200BC");

            entity.ToTable("Tags", "Attractions");

            entity.HasIndex(e => e.TagName, "UQ__Tags__E298655C3A98ABFF").IsUnique();

            entity.Property(e => e.TagId).HasColumnName("tag_id");
            entity.Property(e => e.TagName)
                .HasMaxLength(50)
                .HasColumnName("tag_name");
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

        modelBuilder.Entity<TicketType>(entity =>
        {
            entity.HasKey(e => e.TicketTypeCode).HasName("PK__TicketTy__427E4A98C2DD4C65");

            entity.ToTable("TicketTypes", "Attractions");

            entity.Property(e => e.TicketTypeCode).HasColumnName("ticket_type_code");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");
            entity.Property(e => e.TicketTypeName)
                .HasMaxLength(50)
                .HasColumnName("ticket_type_name");
        });

        modelBuilder.Entity<AttractionLike>(entity =>
        {
            entity.HasKey(e => e.LikeId);

            entity.ToTable("AttractionLikes", "Attractions");

            entity.Property(e => e.LikeId).HasColumnName("like_id");
            entity.Property(e => e.AttractionId).HasColumnName("attraction_id");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .HasColumnName("ip_address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Attraction)
                .WithMany(p => p.AttractionLikes)
                .HasForeignKey(d => d.AttractionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_AttractionLikes_Attractions");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
