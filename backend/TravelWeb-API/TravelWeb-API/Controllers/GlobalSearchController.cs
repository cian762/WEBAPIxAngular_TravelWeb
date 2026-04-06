using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelWeb_API.Models;
using TravelWeb_API.Models.Board.DbSet;

namespace TravelWeb_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GlobalSearchController : ControllerBase
    {
        private readonly GlobalSearchContext _context;
        private readonly BoardDbContext _boardContext;
        private readonly IConfiguration _config;
       

        public GlobalSearchController(GlobalSearchContext context, IConfiguration config,
            BoardDbContext boardContext)
        {
            _context = context;
            _config = config;
            _boardContext = boardContext;
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
            if (string.IsNullOrWhiteSpace(q)) return Ok(new List<object>());

            var suggestions = await _context.ViewGlobalSearches
                .Where(v => v.Title!.Contains(q))
                .OrderByDescending(v => v.Title!.StartsWith(q))
                // 這裡修改 Select，同時抓取標題與類型
                .Select(v => new
                {
                    Title = v.Title,
                    // 假設你的 View 裡面有一欄叫 Category 或 Source
                    // 如果沒有，可以根據你的業務邏輯在 SQL 層轉換
                    Type = v.Category ?? "景點"
                })
                .Distinct()
                .Take(8)
                .ToListAsync();

            return Ok(suggestions);
        }



        // 3. 這是文章引用的
        [HttpGet("getProduct")]
        public async Task<IActionResult> GetProduct(int articleId)
        {
            ArticleSource? source = await _boardContext.ArticleSources
                .Where(a => a.ArticleId == articleId)
                .FirstOrDefaultAsync();
            if (source == null) return Ok();

            var results = await _context.ViewGlobalSearches.ToListAsync();
            var result =  results.Where(g => int.Parse(g.Id) == source.Source &&
                CategorytoINT(g.Category) == source.Type).FirstOrDefault();
            if (result == null) return Ok();
            // 從 appsettings.json 讀取路徑
            string mvcDomain = _config["AppSettings:MvcDomain"]?.TrimEnd('/') ?? "";
            string mvchung = _config["AppSettings:Mvchung"]!;


            if (string.IsNullOrEmpty(result.ImageUrl)) return Ok(result);

            // 如果已經是 http 開頭（如 Article），就不處理
            if (result.ImageUrl.StartsWith("http")) return Ok(result);

            // 根據分類補上對應的前綴
            if (result.Category == "Attraction")
            {
                // 景點：補上 Domain (因為路徑裡已經有 uploads/attractions/...)
                result.ImageUrl = mvcDomain + result.ImageUrl;
            }
            else if (result.Category == "Product")
            {
                // 行程：補上 Mvchung (專門指向 PImages 資料夾)
                result.ImageUrl = mvchung + result.ImageUrl;
            }


            return Ok(result);
        }

        int CategorytoINT(string CategoryName)
        {
            if (CategoryName == "Attraction")
            {
                return 0;
            }
            if (CategoryName == "Product")
            {
                return 2;
            }
            if (CategoryName == "Activity")
            {
                return 1;
            }
            return 999;
        }
    }
}
