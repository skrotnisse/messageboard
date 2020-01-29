using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MessageBoardService.Models;

namespace MessageBoardService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly MessageDBContext _context;

        private bool MessageExists(long id)
        {
            return _context.Messages.Any(e => e.Id == id);
        }

        private long GetUserIdByJWTClaim()
        {
            var userIdString = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userId = Convert.ToInt64(userIdString);
            return userId;
        }

        // Dummy sanitation to provide some kind of basic protection against XSS-attacks..
        // This should be more properly implemented for final production code.
        private void SanitizeMessage(MessageModel message)
        {
            message.Text = message.Text.Replace('<', '.').Replace('>', '.');
            message.Title = message.Title.Replace('<', '.').Replace('>', '.');
        }

        public MessagesController(MessageDBContext context)
        {
            _context = context;
        }

        // GET: api/Messages
        [HttpGet]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await _context.Messages.ToListAsync();

            if (messages == null) {
                return NotFound();
            }

            return Ok(messages);
        }

        // GET: api/Messages/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessage(long id)
        {
            var message = await _context.Messages.FindAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            return Ok(message);
        }

        // PUT: api/Messages/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMessage(long id, MessageModel message)
        {
            if (id != message.Id || !ModelState.IsValid)
            {
                return BadRequest();
            }

            var originalMessage = _context.Messages.SingleOrDefault(e => e.Id == id);
            if (originalMessage == null) {
                return NotFound();
            }

            // Don't allow users to update each others messages.
            try
            {
                long userId = GetUserIdByJWTClaim();
                if (userId != originalMessage.UserId)
                {
                    return Unauthorized();
                }
            }
            catch
            {
                return BadRequest();
            }

            try
            {
                message.CreationDateTime = originalMessage.CreationDateTime;
                message.UserId = originalMessage.UserId;

                SanitizeMessage(message);

                _context.Entry(originalMessage).CurrentValues.SetValues(message);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MessageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }

        // POST: api/Messages
        [HttpPost]
        public async Task<IActionResult> PostMessage(MessageModel message)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (MessageExists(message.Id))
            {
                return Conflict();
            }

            // Assign UserID found in JWT to message.
            try
            {
                long userId = GetUserIdByJWTClaim();
                message.UserId = userId;
            }
            catch
            {
                return BadRequest();
            }

            message.CreationDateTime = DateTime.Now;

            SanitizeMessage(message);

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, message);
        }

        // DELETE: api/Messages/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(long id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            // Don't allow users to delete each others messages.
            try
            {
                long userId = GetUserIdByJWTClaim();
                if (userId != message.UserId)
                {
                    return Unauthorized();
                }
            }
            catch
            {
                return BadRequest();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return Ok(message);
        }
    }
}