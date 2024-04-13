using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfFuncCallSK.Data;
using EfFuncCallSK.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace EfFuncCallSK.Pages;

[Authorize]
public class ResumeListModel : PageModel
{

    private readonly ApplicationDbContext _context;
    private readonly ILogger<ResumeListModel> _logger;
    private readonly UserManager<IdentityUser> _userManager;

    public ResumeListModel(ILogger<ResumeListModel> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
    }

    public IList<Resume> ResumeList { get; set; }

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        ResumeList = await _context.Resumes
            .Include(r => r.JobDescription) // Include JobDescription when querying for Resumes
            .Where(r => r.Email == user.Email) // Filter Resumes by user's email
            .ToListAsync();
    }
}