using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EfFuncCallSK.Models;
public class CoverLetter
{
    [Key]
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string CompanyName { get; set; }
    public string JobTitle { get; set; }
    public string Content { get; set; }

    public JobDescription JobDescription { get; set; }
}
