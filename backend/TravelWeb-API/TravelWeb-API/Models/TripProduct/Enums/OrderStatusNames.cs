using System.ComponentModel.DataAnnotations;

namespace TravelWeb_API.Models.TripProduct.Enums
{
    // 1. 狀態名稱常數 (給資料庫字串用的，不需要 DisplayName，因為它是值本身)
    public static class OrderStatusNames
    {
        public const string Pending = "待處理";     // 待處理
        public const string Paid = "已付款";           // 已付款
        public const string Active = "已成立";       // 已成立
        public const string Cancelled = "已取消"; // 已取消
        public const string Completed = "已完成"; // 已完成
    }

    // 2. 動作觸發 Enum (加上 Display 屬性，方便 UI 顯示中文)
    public enum OrderTrigger
    {
        [Display(Name = "支付成功")]
        PaySuccess,

        [Display(Name = "用戶取消")]
        UserCancel,

        [Display(Name = "申請退款")]
        ApplyRefund,

        [Display(Name = "行程結束/核銷")]
        AdminFinish
    }
}
