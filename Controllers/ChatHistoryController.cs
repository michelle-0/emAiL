using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EfFuncCallSK.Data;
using EfFuncCallSK.Models;
using Microsoft.AspNetCore.Identity;

namespace EfFuncCallSK.Controllers
{
    public class ChatHistoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager; // UserManager for ApplicationUser

        public ChatHistoryController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Method to get the current user's ID
        private string GetCurrentUserId() => _userManager.GetUserId(User);

        // Method to save a chat message to the database
        [HttpPost]
        public async Task<IActionResult> SaveChatMessage(string userMessage, string aiResponse)
        {
            var chatHistory = new ChatHistory
            {
                UserId = GetCurrentUserId(),
                UserMessage = userMessage,
                AIResponse = aiResponse,
                Timestamp = DateTime.UtcNow // Use UTC for consistency
            };

            _context.Add(chatHistory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ShowChatHistory)); // Redirect to ShowChatHistory to display the updated chat history
        }

        // Method to show chat history for the current user
        public async Task<IActionResult> ShowChatHistory()
        {
            var userId = GetCurrentUserId();
            var chatHistories = await _context.ChatHistories
                                              .Where(ch => ch.UserId == userId)
                                              .OrderByDescending(ch => ch.Timestamp)
                                              .ToListAsync();
            return View(chatHistories); // Make sure you have a corresponding view to display this list
        }
        // GET: ChatHistory
        public async Task<IActionResult> Index()
        {
            return View(await _context.ChatHistories.ToListAsync());
        }

        // GET: ChatHistory/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chatHistory = await _context.ChatHistories
                .FirstOrDefaultAsync(m => m.ChatId == id);
            if (chatHistory == null)
            {
                return NotFound();
            }

            return View(chatHistory);
        }

        // GET: ChatHistory/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ChatHistory/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ChatId,UserId,UserMessage,AIResponse,Timestamp")] ChatHistory chatHistory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chatHistory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(chatHistory);
        }

        // GET: ChatHistory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chatHistory = await _context.ChatHistories.FindAsync(id);
            if (chatHistory == null)
            {
                return NotFound();
            }
            return View(chatHistory);
        }

        // POST: ChatHistory/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ChatId,UserId,UserMessage,AIResponse,Timestamp")] ChatHistory chatHistory)
        {
            if (id != chatHistory.ChatId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chatHistory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChatHistoryExists(chatHistory.ChatId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(chatHistory);
        }

        // GET: ChatHistory/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chatHistory = await _context.ChatHistories
                .FirstOrDefaultAsync(m => m.ChatId == id);
            if (chatHistory == null)
            {
                return NotFound();
            }

            return View(chatHistory);
        }

        // POST: ChatHistory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chatHistory = await _context.ChatHistories.FindAsync(id);
            if (chatHistory != null)
            {
                _context.ChatHistories.Remove(chatHistory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChatHistoryExists(int id)
        {
            return _context.ChatHistories.Any(e => e.ChatId == id);
        }


    }
}
