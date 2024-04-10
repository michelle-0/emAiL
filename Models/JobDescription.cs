using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
namespace EfFuncCallSK.Models;

public class JobDescription
{   [Key]
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public string JobTitle { get; set; }
    public string jobResponsibilities{ get; set; }
    public string JobRequirements { get; set; }
}
