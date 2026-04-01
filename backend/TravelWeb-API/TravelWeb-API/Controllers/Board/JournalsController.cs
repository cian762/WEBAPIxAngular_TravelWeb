using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TravelWeb_API.Models.Board.DbSet;
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

        public JournalsController(IJournalService journalService)
        {
            _journalService = journalService;
        }

       

        //// GET: api/Journals/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<Journal>> GetJournal(int id)
        //{
        //    var journal = await _context.Journals.FindAsync(id);

        //    if (journal == null)
        //    {
        //        return NotFound();
        //    }

        //    return journal;
        //}

        //// PUT: api/Journals/5
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutJournal(int id, Journal journal)
        //{
        //    if (id != journal.ArticleId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(journal).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!JournalExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/Journals
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<bool> PostJournal()
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null) return false;
            var journal = await _journalService.postJournal(currentUserId);      

            return journal;
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
