using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EfFuncCallSK.Models;
public class Resume
{
    [Key]
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Experience { get; set; }
    public string Education { get; set; }
    public string Skills { get; set; }
    public string Projects { get; set; }
}