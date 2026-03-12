using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TravelWeb_API.Models.TripProduct;

public partial class TripDbContext : DbContext
{
    public TripDbContext()
    {
    }

    public TripDbContext(DbContextOptions<TripDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AcitivityTicket> AcitivityTickets { get; set; }

    public virtual DbSet<Activity> Activities { get; set; }

    public virtual DbSet<ActivityImage> ActivityImages { get; set; }

    public virtual DbSet<Attraction> Attractions { get; set; }

    public virtual DbSet<AttractionProduct> AttractionProducts { get; set; }

    public virtual DbSet<CancellationPolicy> CancellationPolicies { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<ItineraryProductCollection> ItineraryProductCollections { get; set; }

    public virtual DbSet<MemberInformation> MemberInformations { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderItemTicket> OrderItemTickets { get; set; }

    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    public virtual DbSet<Resource> Resources { get; set; }

    public virtual DbSet<ResourcesImage> ResourcesImages { get; set; }

    public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }

    public virtual DbSet<TicketCategory> TicketCategories { get; set; }

    public virtual DbSet<TravelTag> TravelTags { get; set; }

    public virtual DbSet<TripItineraryItem> TripItineraryItems { get; set; }

    public virtual DbSet<TripProduct> TripProducts { get; set; }

    public virtual DbSet<TripRegion> TripRegions { get; set; }

    public virtual DbSet<TripSchedule> TripSchedules { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

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
        });

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.ActivityId).HasName("PK_活動表_1");

            entity.ToTable("Activities", "Activity");

            entity.Property(e => e.ActivityId).HasColumnName("ActivityID");
            entity.Property(e => e.Title).HasMaxLength(50);
            entity.Property(e => e.UpdateAt).HasColumnType("datetime");
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
        });

        modelBuilder.Entity<CancellationPolicy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).HasName("PK__Cancella__2E1339A4CC83D9F9");

            entity.ToTable("CancellationPolicies", "product");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.PolicyName).HasMaxLength(50);
            entity.Property(e => e.RefundRate).HasColumnType("decimal(3, 2)");
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

        modelBuilder.Entity<ItineraryProductCollection>(entity =>
        {
            entity.HasKey(e => e.FavoriteProductId);

            entity.ToTable("ItineraryProductCollection");

            entity.Property(e => e.MemberId).HasMaxLength(50);

            entity.HasOne(d => d.Member).WithMany(p => p.ItineraryProductCollections)
                .HasForeignKey(d => d.MemberId)
                .HasConstraintName("FK_ItineraryProductCollection_Member_Information");

            entity.HasOne(d => d.TripProduct).WithMany(p => p.ItineraryProductCollections)
                .HasForeignKey(d => d.TripProductId)
                .HasConstraintName("FK_ItineraryProductCollection_TripProducts");
        });

        modelBuilder.Entity<MemberInformation>(entity =>
        {
            entity.HasKey(e => e.MemberId);

            entity.ToTable("Member_Information", "Member");

            entity.Property(e => e.MemberId).HasMaxLength(50);
            entity.Property(e => e.AvatarUrl).HasMaxLength(255);
            entity.Property(e => e.MemberCode).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCFF0307F3C");

            entity.ToTable("Orders", "product");

            entity.Property(e => e.ContactEmail).HasMaxLength(255);
            entity.Property(e => e.ContactName).HasMaxLength(100);
            entity.Property(e => e.ContactPhone).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerNote).HasMaxLength(500);
            entity.Property(e => e.MemberId).HasMaxLength(50);
            entity.Property(e => e.OrderStatus).HasMaxLength(20);
            entity.Property(e => e.PaymentStatus).HasMaxLength(20);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Member).WithMany(p => p.Orders)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Member_Information");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__OrderIte__57ED0681138B173F");

            entity.ToTable("OrderItems", "product");

            entity.Property(e => e.ProductCode).HasMaxLength(50);
            entity.Property(e => e.ProductNameSnapshot).HasMaxLength(255);

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItems_Orders");
        });

        modelBuilder.Entity<OrderItemTicket>(entity =>
        {
            entity.HasKey(e => e.OrderItemTicketId).HasName("PK__OrderIte__9F6B10C635839B8B");

            entity.ToTable("OrderItemTickets", "product");

            entity.Property(e => e.TicketNameSnapshot).HasMaxLength(50);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.OrderItem).WithMany(p => p.OrderItemTickets)
                .HasForeignKey(d => d.OrderItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OIT_OrderItems");

            entity.HasOne(d => d.TicketCategory).WithMany(p => p.OrderItemTickets)
                .HasForeignKey(d => d.TicketCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OIT_Tickets");
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__PaymentT__55433A6BA97FD419");

            entity.ToTable("PaymentTransactions", "product");

            entity.HasIndex(e => e.ProviderTransactionNo, "UQ__PaymentT__10C5588BD82A9F25").IsUnique();

            entity.Property(e => e.CompletedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaymentMethod).HasMaxLength(20);
            entity.Property(e => e.PaymentProvider).HasMaxLength(50);
            entity.Property(e => e.ProviderTransactionNo).HasMaxLength(100);
            entity.Property(e => e.ResponseCode).HasMaxLength(20);
            entity.Property(e => e.TransactionStatus).HasMaxLength(20);

            entity.HasOne(d => d.Order).WithMany(p => p.PaymentTransactions)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Orders");
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.ResourceId).HasName("PK__Resource__4ED1816F8DB494E7");

            entity.ToTable("Resources", "product");

            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.ResourceName).HasMaxLength(255);
            entity.Property(e => e.ShortDescription).HasMaxLength(255);
        });

        modelBuilder.Entity<ResourcesImage>(entity =>
        {
            entity.HasKey(e => e.MainImageid);

            entity.ToTable("ResourcesImage");

            entity.Property(e => e.MainImage).HasMaxLength(255);

            entity.HasOne(d => d.Resource).WithMany(p => p.ResourcesImages)
                .HasForeignKey(d => d.ResourceId)
                .HasConstraintName("FK_ResourcesImage_Resources");
        });

        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Shopping__51BCD7B7F931242E");

            entity.ToTable("ShoppingCart", "product");

            entity.HasIndex(e => new { e.MemberId, e.ProductCode }, "UX_ShoppingCart_User_Schedule").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MemberId).HasMaxLength(50);
            entity.Property(e => e.ProductCode).HasMaxLength(50);

            //entity.HasOne(d => d.Member).WithMany(p => p.ShoppingCarts)
            //    .HasForeignKey(d => d.MemberId)
            //    .OnDelete(DeleteBehavior.ClientSetNull)
            //    .HasConstraintName("FK_ShoppingCart_Member_Information");

            entity.HasOne(d => d.TicketCategory).WithMany(p => p.ShoppingCarts)
                .HasForeignKey(d => d.TicketCategoryId)
                .HasConstraintName("FK_ShoppingCart_TicketCategories");
        });

        modelBuilder.Entity<TicketCategory>(entity =>
        {
            entity.HasKey(e => e.TicketCategoryId).HasName("PK__TicketCa__C84589E67C04231C");

            entity.ToTable("TicketCategories", "product");

            entity.Property(e => e.CategoryName).HasMaxLength(50);
        });

        modelBuilder.Entity<TravelTag>(entity =>
        {
            entity.Property(e => e.TravelTagName).HasMaxLength(50);
        });

        modelBuilder.Entity<TripItineraryItem>(entity =>
        {
            entity.HasKey(e => e.ItineraryItemId).HasName("PK__TripItin__1BF8587EE59168B4");

            entity.ToTable("TripItineraryItems", "product");

            entity.Property(e => e.ActivityId).HasColumnName("ActivityID");
            entity.Property(e => e.AttractionId).HasColumnName("attraction_id");

            entity.HasOne(d => d.Activity).WithMany(p => p.TripItineraryItems)
                .HasForeignKey(d => d.ActivityId)
                .HasConstraintName("FK_TripItineraryItems_Activities");

            entity.HasOne(d => d.Attraction).WithMany(p => p.TripItineraryItems)
                .HasForeignKey(d => d.AttractionId)
                .HasConstraintName("FK_TripItineraryItems_Attractions");

            entity.HasOne(d => d.Resource).WithMany(p => p.TripItineraryItems)
                .HasForeignKey(d => d.ResourceId)
                .HasConstraintName("FK_Itinerary_Resources");

            entity.HasOne(d => d.TripProduct).WithMany(p => p.TripItineraryItems)
                .HasForeignKey(d => d.TripProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TripItineraryItems_TripProducts");
        });

        modelBuilder.Entity<TripProduct>(entity =>
        {
            entity.HasKey(e => e.TripProductId).HasName("PK__TripProd__36D9B92A7A23B8AB");

            entity.ToTable("TripProducts", "product");

            entity.Property(e => e.CoverImage)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.DisplayPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(255);
            entity.Property(e => e.RegionId).HasColumnName("RegionID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Policy).WithMany(p => p.TripProducts)
                .HasForeignKey(d => d.PolicyId)
                .HasConstraintName("FK_TripProducts_CancellationPolicies");

            entity.HasOne(d => d.Region).WithMany(p => p.TripProducts)
                .HasForeignKey(d => d.RegionId)
                .HasConstraintName("FK_TripProducts_TripRegions");

            entity.HasMany(d => d.TravelTags).WithMany(p => p.TripProducts)
                .UsingEntity<Dictionary<string, object>>(
                    "TravelandProductRelation",
                    r => r.HasOne<TravelTag>().WithMany()
                        .HasForeignKey("TravelTagid")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TravelandProductRelation_TravelTags"),
                    l => l.HasOne<TripProduct>().WithMany()
                        .HasForeignKey("TripProductId")
                        .HasConstraintName("FK_TravelandProductRelation_TripProducts"),
                    j =>
                    {
                        j.HasKey("TripProductId", "TravelTagid");
                        j.ToTable("TravelandProductRelation","dbo");
                    });
        });

        modelBuilder.Entity<TripRegion>(entity =>
        {
            entity.HasKey(e => e.RegionId).HasName("PK__TripRegi__ACD8444348146F8D");

            entity.Property(e => e.RegionId).HasColumnName("RegionID");
            entity.Property(e => e.RegionName).HasMaxLength(50);
        });

        modelBuilder.Entity<TripSchedule>(entity =>
        {
            entity.HasKey(e => e.ProductCode).HasName("PK__TripSche__D559BD2170733C82");

            entity.ToTable("TripSchedules", "product");

            entity.Property(e => e.ProductCode).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.SoldQuantity).HasDefaultValue(0);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.TripProduct).WithMany(p => p.TripSchedules)
                .HasForeignKey(d => d.TripProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TripSchedules_TripProducts");

            entity.HasMany(d => d.TicketCategories).WithMany(p => p.ProductCodes)
                .UsingEntity<Dictionary<string, object>>(
                    "TripAndTicketRelation",
                    r => r.HasOne<TicketCategory>().WithMany()
                        .HasForeignKey("TicketCategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TripAndTicketRelation_TicketCategories"),
                    l => l.HasOne<TripSchedule>().WithMany()
                        .HasForeignKey("ProductCode")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TripAndTicketRelation_TripSchedules"),
                    j =>
                    {
                        j.HasKey("ProductCode", "TicketCategoryId");
                        j.ToTable("TripAndTicketRelation","dbo");
                        j.IndexerProperty<string>("ProductCode").HasMaxLength(50);
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
