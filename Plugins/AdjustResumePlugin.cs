using System.ComponentModel;
using System.Text.Json;
using EfFuncCallSK.Models;
using Microsoft.SemanticKernel;

namespace EfFuncCallSK.Plugins;
public class AdjustResumePlugin
{
    [KernelFunction, Description("Get all resume details")]
    public static string? GetResumeDetails(
        [Description("applicant's full name, e.g. Kim Jane Wexler")] string fullName,
        [Description("applicant's email, e.g. kimjwexler@gmail.com")] string email,
        [Description("applicant's experience e.g. Software Developer at Fortinet May 2020 to December 2020, responsible for transitioning company to new testing software")] string experience,
        [Description("applicant's education, e.g. Diploma in Computer Systems Technology")] string education,
        [Description("applicant's skills, e.g. Java, C#, Python, JavaScript, mySQL, RESTful")] string skills,
        [Description("applicant's projects, e.g. Developed an AI web application using ASP.NET Core where I learned agile development, implemented MVC pattern and CICD")] string projects
    )
    {
        var resume = new Resume
        {
            FullName = fullName,
            Email = email,
            Experience = experience,
            Education = education,
            Skills = skills,
            Projects = projects,
        };
        var jsonResume = JsonSerializer.Serialize(resume);

        return jsonResume;
    }


    [KernelFunction, Description("Get the job description details")]
    public static string? GetJobDescription(
    [Description("company name, e.g. Fortinet")] string companyName,
    [Description("job title, e.g. Software Developer")] string jobTitle,
    [Description("job description, e.g. Develop software solutions by studying information needs, conferring with users, and studying systems flow, data usage, and work processes.")] string jobResponsibilities,
    [Description("job requirements, e.g. A degree in Computer Science or related field, 3+ years of experience in software development, experience with Java, C#, Python, JavaScript, mySQL, RESTful")] string jobRequirements,
    [Description("company values, e.g. Integrity, Innovation, Collaboration, Customer Focus")] string companyValues
)
    {
        var db = Utils.GetDbContext();
        var jobDescriptionDetails = db.JobDescriptions
        .Where(j => j.CompanyName == companyName && j.JobTitle == jobTitle).FirstOrDefault();
        if (jobDescriptionDetails == null)
            return null;
        return jobDescriptionDetails.ToString();
    }
}