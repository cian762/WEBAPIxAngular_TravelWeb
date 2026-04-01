using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelWeb_API.Models;
using Microsoft.EntityFrameworkCore;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GlobalSearchController : ControllerBase
    {
        private readonly GlobalSearchContext _context;
        private readonly IConfiguration _config;
       

        public GlobalSearchController(GlobalSearchContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        //全站導航引擎搜尋框使用
        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return Ok(new List<ViewGlobalSearch>());

            var results = await _context.ViewGlobalSearches
                .Where(v => v.Title!.Contains(q))
                .Take(30)
                .ToListAsync();

            // 從 appsettings.json 讀取路徑
            string mvcDomain = _config["AppSettings:MvcDomain"]?.TrimEnd('/') ?? "";
            string mvchung = _config["AppSettings:Mvchung"]!;     

            foreach (var item in results)
            {
                if (string.IsNullOrEmpty(item.ImageUrl)) continue;

                // 如果已經是 http 開頭（如 Article），就不處理
                if (item.ImageUrl.StartsWith("http")) continue;

                // 根據分類補上對應的前綴
                if (item.Category == "Attraction")
                {
                    // 景點：補上 Domain (因為路徑裡已經有 uploads/attractions/...)
                    item.ImageUrl = mvcDomain + item.ImageUrl;
                }
                else if (item.Category == "Product")
                {
                    // 行程：補上 Mvchung (專門指向 PImages 資料夾)
                    item.ImageUrl = mvchung + item.ImageUrl;
                }
            }

            return Ok(results);
        }
        // 2. 這是給「搜尋框下拉提示」用的 (只回傳字串陣列)
        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return Ok(new List<string>());

            var suggestions = await _context.ViewGlobalSearches
                .Where(v => v.Title!.Contains(q))
                .OrderByDescending(v => v.Title!.StartsWith(q))
                .Select(v => v.Title)
                .Distinct()
                .Take(8)
                .ToListAsync();

            return Ok(suggestions);
        }
    }
}
