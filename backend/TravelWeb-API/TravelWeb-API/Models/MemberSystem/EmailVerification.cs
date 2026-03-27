using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelWeb_API.Models.MemberSystem
{
    // 🔥 指定對應的資料表名稱與 Schema (請確保您 SQL Server 中也有一樣的表！)
    [Table("Email_Verification", Schema = "Member")]
    public partial class EmailVerification
    {
        // 1. Email (作為主鍵，因為一個信箱同一時間只需要一組最新的驗證碼)
        [Key]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        // 2. 6 位數驗證碼
        [StringLength(6)]
        public string VerificationCode { get; set; } = null!;

        // 3. 驗證碼過期時間 (建議設為發送後的 10 ~ 15 分鐘)
        public DateTime ExpiryTime { get; set; }

        // 4. 判斷是否已經成功輸入過正確的驗證碼
        // (前端打勾後，後端最後註冊時會檢查這個欄位)
        public bool IsVerified { get; set; } = false;
    }
}