using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TravelWeb_API.Models.MemberSystem;

public partial class MemberSystemContext : DbContext
{
    public MemberSystemContext()
    {
    }

    public MemberSystemContext(DbContextOptions<MemberSystemContext> options)
        : base(options)
    {
    }

    
    public virtual DbSet<Administrator> Administrators { get; set; }

    public virtual DbSet<Authorization> Authorizations { get; set; }

    public virtual DbSet<Blocked> Blockeds { get; set; }

    public virtual DbSet<ComplaintRecord> ComplaintRecords { get; set; }

    public virtual DbSet<LogInRecord> LogInRecords { get; set; }

    public virtual DbSet<MemberComplaint> MemberComplaints { get; set; }

    public virtual DbSet<MemberInformation> MemberInformations { get; set; }

    public virtual DbSet<MemberList> MemberLists { get; set; }

    public virtual DbSet<EmailVerification> EmailVerifications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.HasKey(e => e.AdminId);

            entity.ToTable("Administrator", "Member");

            entity.Property(e => e.AdminId).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<Authorization>(entity =>
        {
            entity.ToTable("Authorization", "Member");

            entity.Property(e => e.AuthorizationId).ValueGeneratedNever();
            entity.Property(e => e.AdminId).HasMaxLength(50);
            entity.Property(e => e.ExecutedAt).HasColumnType("datetime");
            entity.Property(e => e.MemberCode).HasMaxLength(50);
            entity.Property(e => e.Permission).HasMaxLength(50);
            entity.Property(e => e.Remark).HasMaxLength(255);

            entity.HasOne(d => d.Admin).WithMany(p => p.Authorizations)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Authorization_Administrator");

            entity.HasOne(d => d.MemberCodeNavigation).WithMany(p => p.Authorizations)
                .HasForeignKey(d => d.MemberCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Authorization_Member_List");
        });

        modelBuilder.Entity<Blocked>(entity =>
        {
            entity.HasKey(e => e.MemberId);

            entity.ToTable("blocked", "Member");

            entity.Property(e => e.MemberId).HasMaxLength(50);
            entity.Property(e => e.BlockedId).HasMaxLength(50);
            entity.Property(e => e.Reason).HasMaxLength(100);
        });

        modelBuilder.Entity<ComplaintRecord>(entity =>
        {
            entity.HasKey(e => e.ComplaintId);

            entity.ToTable("Complaint_Record", "Member");

            entity.Property(e => e.ComplaintId).HasMaxLength(50);
            entity.Property(e => e.AdminId).HasMaxLength(50);
            entity.Property(e => e.Compensation).HasMaxLength(255);
            entity.Property(e => e.MemberCode).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Admin).WithMany(p => p.ComplaintRecords)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Complaint_Record_Administrator");

            entity.HasOne(d => d.MemberCodeNavigation).WithMany(p => p.ComplaintRecords)
                .HasForeignKey(d => d.MemberCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Complaint_Record_Member_List");
        });

        modelBuilder.Entity<LogInRecord>(entity =>
        {
            entity.ToTable("Log_in_record", "Member");

            entity.Property(e => e.LoginRecordId)
                  .UseIdentityColumn() 
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.LoginAt).HasColumnType("datetime");
            entity.Property(e => e.MemberCode).HasMaxLength(50);

            entity.HasOne(d => d.MemberCodeNavigation).WithMany(p => p.LogInRecords)
                .HasForeignKey(d => d.MemberCode)
                .HasConstraintName("FK_Log_in_record_Member_List");
        });

        modelBuilder.Entity<MemberComplaint>(entity =>
        {
            entity.HasKey(e => e.ComplaintId);

            entity.ToTable("Member_Complaint", "Member");

            entity.Property(e => e.ComplaintId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.MemberId).HasMaxLength(50);
            entity.Property(e => e.ReplyEmail).HasMaxLength(100);

            entity.HasOne(d => d.Complaint).WithOne(p => p.MemberComplaint)
                .HasForeignKey<MemberComplaint>(d => d.ComplaintId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Member_Complaint_Complaint_Record");

            entity.HasOne(d => d.Member).WithMany(p => p.MemberComplaints)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Member_Complaint_Member_Information");
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

            // 只設定一側，EF Core 自動推導反向
            entity.HasMany(d => d.Followeds)
                .WithMany(p => p.Followers)
                .UsingEntity<Dictionary<string, object>>(
                    "MemberFollowing",
                    r => r.HasOne<MemberInformation>().WithMany()
                        .HasForeignKey("FollowedId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Member_Following_Member_Information1"),
                    l => l.HasOne<MemberInformation>().WithMany()
                        .HasForeignKey("FollowerId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Member_Following_Member_Information"),
                    j =>
                    {
                        j.HasKey("FollowerId", "FollowedId").HasName("PK_Member_Following_1");
                        j.ToTable("Member_Following", "Member");
                        j.IndexerProperty<string>("FollowerId").HasMaxLength(50);
                        j.IndexerProperty<string>("FollowedId").HasMaxLength(50);
                    });

            modelBuilder.Entity<MemberInformation>()
                         .HasMany(m => m.blockedIds)        // 我封鎖的人
                         .WithMany(m => m.blockedIngs)      // 封鎖我的人
                         .UsingEntity<Blocked>(
                          j => j.HasOne<MemberInformation>()
                                 .WithMany()
                                 .HasForeignKey(b => b.BlockedId),
                                  j => j.HasOne<MemberInformation>()
                                         .WithMany()
                                         .HasForeignKey(b => b.MemberId)
                                                                          );
        });
        

        modelBuilder.Entity<MemberList>(entity =>
        {
            entity.HasKey(e => e.MemberCode);

            entity.ToTable("Member_List", "Member");

            entity.Property(e => e.MemberCode).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<EmailVerification>(entity =>
        {
            entity.HasKey(e => e.Email);

            entity.ToTable("Email_Verification", "Member");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.VerificationCode).HasMaxLength(6);
            entity.Property(e => e.ExpiryTime).HasColumnType("datetime");

        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
