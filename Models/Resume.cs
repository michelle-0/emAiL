using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace EfFuncCallSK.Models;
public class Resume(string fullName, string email, string experiences, string education, string skills, string projects)
{
    [Key]
    public int ResumeId { get; set; }
    public IdentityUser User { get; set; }
    public string FullName { get; set; } = fullName;
    public string Email { get; set; } = email;

    public string Experiences { get; set; } = experiences;
    public string Education { get; set; } = education;
    public string Skills { get; set; } = skills;

    public string Projects { get; set; } = projects;
    public JobDescription JobDescription { get; set; }
}
