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

namespace TravelWeb_API.Controllers.Board
{
    [Route("api/Board/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Board")]
    public class ReportLogsController : ControllerBase
    {
        private readonly BoardDbContext _context;

        public ReportLogsController(BoardDbContext context)
        {
            _context = context;
        }

        // GET: api/ReportLogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReportLog>>> GetReportLogs()
        {
            return await _context.ReportLogs.ToListAsync();
        }

        // GET: api/ReportLogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReportLog>> GetReportLog(int id)
        {
            var reportLog = await _context.ReportLogs.FindAsync(id);

            if (reportLog == null)
            {
                return NotFound();
            }

            return reportLog;
        }

        // PUT: api/ReportLogs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReportLog(int id, ReportLog reportLog)
        {
            if (id != reportLog.LogId)
            {
                return BadRequest();
            }

            _context.Entry(reportLog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReportLogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ReportLogs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult> PostReportLog(Log reportLog)
        {
            // 從 Cookie 取出 Token  
            string? token = Request.Cookies["AuthToken"];
            string? currentUserId = GetUser.Id(token);
            if (string.IsNullOrEmpty(currentUserId))
                return BadRequest();
            try
            {
                _context.ReportLogs.Add(
                    new ReportLog
                    {
                        TargetId = reportLog.TargetID,
                        TargetType = 0,
                        UserId = currentUserId, 
                        ViolationType = (byte)reportLog.ViolationType,
                        Reason = reportLog.Reason,
                        Photo = reportLog.Photo,
                        ResultType = 0,
                    }
                    );
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch { 
            return BadRequest();
            }
        }

        // DELETE: api/ReportLogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReportLog(int id)
        {
            var reportLog = await _context.ReportLogs.FindAsync(id);
            if (reportLog == null)
            {
                return NotFound();
            }

            _context.ReportLogs.Remove(reportLog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReportLogExists(int id)
        {
            return _context.ReportLogs.Any(e => e.LogId == id);
        }
    }
}
