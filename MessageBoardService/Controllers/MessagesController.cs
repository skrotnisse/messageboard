using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessageBoardService.Models;

namespace MessageBoardService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly MessageDBContext _context;

        private bool MessageExists(long id)
        {
            return _context.Messages.Any(e => e.Id == id);
        }

        private void SanitizeMessage(Message message)
        {
            // Dummy sanitation to provide basic protection against XSS-attacks.
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
        public async Task<IActionResult> PutMessage(long id, Message message)
        {
            if (id != message.Id || !ModelState.IsValid)
            {
                return BadRequest();
            }

            SanitizeMessage(message);

            _context.Entry(message).State = EntityState.Modified;

            try
            {
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

            return NoContent();
        }

        // POST: api/Messages
        [HttpPost]
        public async Task<IActionResult> PostMessage(Message message)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (MessageExists(message.Id))
            {
                return Conflict();
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

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return Ok(message);
        }
    }
}