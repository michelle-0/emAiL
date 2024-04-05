using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EfFuncCallSK.Data;
using EfFuncCallSK.Models;
using Microsoft.AspNetCore.Identity;

namespace EfFuncCallSK.Pages
{
    [Authorize]
    public class HistoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public List<ChatHistory> ChatHistories { get; set; }

        public HistoryModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task OnGetAsync(string searchTerm)
        {
            var userId = _userManager.GetUserId(User); // Fetch the current user's ID

            IQueryable<ChatHistory> chatQuery = _context.ChatHistories
                                                         .Where(ch => ch.UserId == userId)
                                                         .OrderByDescending(ch => ch.Timestamp);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                chatQuery = chatQuery.Where(ch => ch.UserMessage.Contains(searchTerm) ||
                                                  ch.AIResponse.Contains(searchTerm));
            }

            ChatHistories = await chatQuery.ToListAsync();
        }


        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var chatHistoryItem = await _context.ChatHistories.FindAsync(id);

            // Check if the item belongs to the current user before attempting to delete
            if (chatHistoryItem != null && chatHistoryItem.UserId == _userManager.GetUserId(User))
            {
                _context.ChatHistories.Remove(chatHistoryItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
