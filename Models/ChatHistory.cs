using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace EfFuncCallSK.Models;
public class ChatHistory
{
    [Key]
    public int ChatId { get; set; }
    
    [ForeignKey("User")]
    public string UserId { get; set; } 
    public string UserMessage { get; set; }
    public string AIResponse { get; set; }
    public DateTime Timestamp { get; set; }

    public IdentityUser User { get; set; }
}
