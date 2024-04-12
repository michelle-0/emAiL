using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfFuncCallSK.Data;
using EfFuncCallSK.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
namespace EfFuncCallSK.Pages;
    public class ResumeList : PageModel
    {
        
private readonly ApplicationDbContext _context;
        private readonly ILogger<ResumeList> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        public List<Resume> resumes { get; set; }
        public ResumeList(ILogger<ResumeList> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public void OnGet()
        {
            resumes = _context.Resumes
            .Where(r => r.Email == _userManager.GetUserName(User))
            .ToList();  
        }
    }