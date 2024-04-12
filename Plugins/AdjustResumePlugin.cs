using System.ComponentModel;
using System.Text.Json;
using EfFuncCallSK.Models;
using Microsoft.SemanticKernel;

namespace EfFuncCallSK.Plugins
{
    public class AdjustResumePlugin
    {
            [KernelFunction, Description("Get all resume details")]
            public static string? GetResumeDetails(
            [Description("applicant's full name, e.g. Kim Jane Wexler")] string fullName,
            [Description("applicant's email, e.g. kimjwexler@gmail.com")] string email,
            [Description("applicant's previous work experiences")] string experiences,
            [Description("applicant's education, e.g. Diploma in Computer Systems Technology")] string education,
            [Description("applicant's technical and applicable skills, e.g. Java, C#, Python, JavaScript, mySQL, RESTful")] string skills,
            [Description("applicant's projects that demonstrate their skills and abilities")] string projects
        )
        {
            var db = Utils.GetDbContext();
            var resume = db.Resumes
                .Where(r => r.FullName == fullName && r.Email == email && r.Experiences == experiences && r.Education == education && r.Skills == skills && r.Projects == projects)
                .FirstOrDefault();

            var jsonResume = JsonSerializer.Serialize(resume);
            return jsonResume;
        }

        [KernelFunction, Description("Get the job description details")]
        public static string? GetJobDescription(
            [Description("company name, e.g. Fortinet")] string companyName,
            [Description("job title, e.g. Software Developer")] string jobTitle,
            [Description("job description which states the applicant will do at the job")] string jobDescription,
            [Description("job requirements which states what skills and experiences this role will need")] string jobRequirements
        )
        {
            var db = Utils.GetDbContext();
            var jobDescriptionDetails = db.JobDescriptions
                .Where(j => j.CompanyName == companyName && j.JobTitle == jobTitle && j.JobResponsibilities == jobDescription && j.JobRequirements == jobRequirements)
                .FirstOrDefault();

            if (jobDescriptionDetails == null)
                return null;
            return jobDescriptionDetails.ToString();
        }
    }
}
