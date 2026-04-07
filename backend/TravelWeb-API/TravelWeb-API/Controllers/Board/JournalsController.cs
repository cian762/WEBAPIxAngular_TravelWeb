using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TravelWeb_API.Models.Board.DbSet;
using TravelWeb_API.Models.Board.DTO;
using TravelWeb_API.Models.Board.IService;
using TravelWeb_API.Models.Board.Service;

namespace TravelWeb_API.Controllers.Board
{
    [Route("api/Board/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Board")]      
    public class JournalsController : ControllerBase
    {
        private readonly IJournalService _journalService;
        private readonly ITagsService _tagsService;

        public JournalsController(IJournalService journalService, ITagsService tagsService)
        {
            _journalService = journalService;
            _tagsService = tagsService;
        }

        [HttpGet("JournalDetail/{id}")]
        public async Task<ActionResult<JournalDetailDTO>> GetJournalDetail(int id)
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null) return Unauthorized();

            var journal = await _journalService.GetJournalDetail(id,currentUserId);
            if (journal == null) return NotFound();
            return journal;
        }


        // GET: api/Journals/5
        [HttpGet("{id}")]
        public async Task<ActionResult<JournalUpdateDTO>> GetJournal(int id)
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null) return BadRequest();

            var journal = await _journalService.UpdateJournal(id);
            journal.tags = _tagsService.getTagsByArticleId(id).Select(t=>new TagDTO { 
            TagId = t.TagId,
            icon=t.icon,
            TagName = t.TagName,
            }).ToList();

            return journal;
        }

        // PUT: api/Journals/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<bool> PutJournal(int id, JournalUpdateDTO updateDTO)
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null) return false;
            var tagIds = updateDTO.tags?.Select(t => t.TagId).ToList();
            var isTagUpdateSuccessful = await _tagsService.EditTagsByArticleId(id, tagIds);                       
            var isJournalUpdateSuccessful = await _journalService.putJournal(id, updateDTO);
            if (isJournalUpdateSuccessful&& isTagUpdateSuccessful) return true;
            return false;
        }

        // POST: api/Journals
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<int>> PostJournal()
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null) return BadRequest();
            var articleId = await _journalService.postJournal(currentUserId);            
            return articleId;
            
        }

        //// DELETE: api/Journals/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteJournal(int id)
        //{
        //    var journal = await _context.Journals.FindAsync(id);
        //    if (journal == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Journals.Remove(journal);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        //private bool JournalExists(int id)
        //{
        //    return _context.Journals.Any(e => e.ArticleId == id);
        //}
    }
}
