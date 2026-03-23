using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.IService;
using TravelWeb_API.Models.Board.Service;

namespace TravelWeb_API.Controllers.Board
{
    [Route("api/Board/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Board")]
    public class TagsController : Controller
    {
        private readonly BoardDbContext _context;
        private readonly ITagsService _tagsService;

        public TagsController(BoardDbContext context, ITagsService tagsService)
        {
            _context = context;
            _tagsService = tagsService;
        }

        // GET: Tags
        [HttpGet]
        public async Task<IActionResult> getTagsByArticleId([FromQuery]int articleId)
        {
            var result = _tagsService.getTagsByArticleId(articleId);
           
            return Ok(result);
        }

        //// GET: Tags/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var articleTag = await _context.ArticleTags
        //        .Include(a => a.Article)
        //        .Include(a => a.Tag)
        //        .FirstOrDefaultAsync(m => m.ArticleId == id);
        //    if (articleTag == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(articleTag);
        //}

        //// GET: Tags/Create
        //public IActionResult Create()
        //{
        //    ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleId", "ArticleId");
        //    ViewData["TagId"] = new SelectList(_context.TagsLists, "TagId", "TagId");
        //    return View();
        //}

        //// POST: Tags/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("ArticleId,TagId")] ArticleTag articleTag)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(articleTag);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleId", "ArticleId", articleTag.ArticleId);
        //    ViewData["TagId"] = new SelectList(_context.TagsLists, "TagId", "TagId", articleTag.TagId);
        //    return View(articleTag);
        //}

        //// GET: Tags/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var articleTag = await _context.ArticleTags.FindAsync(id);
        //    if (articleTag == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleId", "ArticleId", articleTag.ArticleId);
        //    ViewData["TagId"] = new SelectList(_context.TagsLists, "TagId", "TagId", articleTag.TagId);
        //    return View(articleTag);
        //}

        //// POST: Tags/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("ArticleId,TagId")] ArticleTag articleTag)
        //{
        //    if (id != articleTag.ArticleId)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(articleTag);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!ArticleTagExists(articleTag.ArticleId))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["ArticleId"] = new SelectList(_context.Articles, "ArticleId", "ArticleId", articleTag.ArticleId);
        //    ViewData["TagId"] = new SelectList(_context.TagsLists, "TagId", "TagId", articleTag.TagId);
        //    return View(articleTag);
        //}

        //// GET: Tags/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var articleTag = await _context.ArticleTags
        //        .Include(a => a.Article)
        //        .Include(a => a.Tag)
        //        .FirstOrDefaultAsync(m => m.ArticleId == id);
        //    if (articleTag == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(articleTag);
        //}

        //// POST: Tags/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var articleTag = await _context.ArticleTags.FindAsync(id);
        //    if (articleTag != null)
        //    {
        //        _context.ArticleTags.Remove(articleTag);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool ArticleTagExists(int id)
        //{
        //    return _context.ArticleTags.Any(e => e.ArticleId == id);
        //}
    }
}
