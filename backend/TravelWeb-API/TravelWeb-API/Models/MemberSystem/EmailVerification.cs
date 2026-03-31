using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelWeb_API.Models.MemberSystem
{
    [Table("Email_Verification", Schema = "Member")]
    public partial class EmailVerification
    {
        [Key]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [StringLength(6)]
        public string VerificationCode { get; set; } = null!;

        public DateTime ExpiryTime { get; set; }

        public bool IsVerified { get; set; } = false;
    }
}