// using EfFuncCallSK.Data;
// using EfFuncCallSK.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;

// public class ChatService
// {
//     private readonly ApplicationDbContext _context;

//     public ChatService(ApplicationDbContext context)
//     {
//         _context = context;
//     }

//     // Asynchronously saves a chat message to the database
//     public async Task SaveChatMessage(ChatHistory chatHistory)
//     {
//         _context.ChatHistories.Add(chatHistory);
//         await _context.SaveChangesAsync();
//     }

//     // Asynchronously retrieves chat history for a specific user
//     public async Task<List<ChatHistory>> GetChatHistoryByUserId(string userId)
//     {
//         return await _context.ChatHistories
//                              .Where(chat => chat.UserId == userId)
//                              .OrderBy(chat => chat.Timestamp) // Assuming you want them in chronological order
//                              .ToListAsync();
//     }
// }
using EfFuncCallSK.Data;
using EfFuncCallSK.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ChatService
{
    private readonly ApplicationDbContext _context;

    public ChatService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Asynchronously saves a chat message to the database
    public async Task SaveChatMessageAsync(string userMessage, string aiResponse, string userId)
    {
        var chatHistory = new ChatHistory
        {
            UserId = userId,
            UserMessage = userMessage,
            AIResponse = aiResponse,
            Timestamp = DateTime.UtcNow // Ensure you're using UTC for consistency
        };

        _context.ChatHistories.Add(chatHistory);
        await _context.SaveChangesAsync();
    }

    // Asynchronously retrieves chat history for a specific user
    public async Task<List<ChatHistory>> GetChatHistoryByUserIdAsync(string userId)
    {
        return await _context.ChatHistories
                             .Where(chat => chat.UserId == userId)
                             .OrderByDescending(chat => chat.Timestamp) // Assuming chronological order is desired
                             .ToListAsync();
    }
}

