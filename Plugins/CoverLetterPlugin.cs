using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using EfFuncCallSK.Data;
using EfFuncCallSK.Models;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text.Json;
using EfFuncCallSK.Models;
using Microsoft.SemanticKernel;


namespace EfFuncCallSK.Plugins
{
    public class CoverLetterPlugin
    {
        [KernelFunction, Description("Generate a cover letter based on the resume and job description")]
        public static string? GenerateCoverLetter(
            [Description("applicant's full name, e.g. Kim Jane Wexler")] string fullName,
            [Description("applicant's email, e.g. kimjwexler@gmail.com")] string email,
            [Description("applicant's previous work experiences")] string experiences,
            [Description("applicant's education, e.g. Diploma in Computer Systems Technology")] string education,
            [Description("applicant's technical and applicable skills, e.g. Java, C#, Python, JavaScript, mySQL, RESTful")] string skills,
            [Description("applicant's projects that demonstrate their skills and abilities")] string projects,
            [Description("company name, e.g. Fortinet")] string companyName,
            [Description("job title, e.g. Software Developer")] string jobTitle,
            [Description("job description which states the applicant will do at the job")] string jobDescription,
            [Description("job requirements which states what skills and experiences this role will need")] string jobRequirements
        )
        {
            try
            {
                var resume = new Resume(fullName, email, experiences, education, skills, projects);
                string[] jobKeywords = jobDescription.ToLower().Split(new[] { " ", ",", ".", ";", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                string[] requirementKeywords = jobRequirements.ToLower().Split(new[] { " ", ",", ".", ";", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                string[] commonWords = { "and", "the", "in", "with", "for", "to", "of", "a", "an", "at", "on" };
                jobKeywords = jobKeywords.Except(commonWords).ToArray();
                requirementKeywords = requirementKeywords.Except(commonWords).ToArray();
                string relevantSkills = string.Join(", ", resume.Skills
                    .ToLower()
                    .Split(new[] { " ", ",", ".", ";", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Intersect(jobKeywords)
                    .Distinct());
                string coverLetterContent = $"Dear Hiring Manager,\n\n" +
                                            $"I am writing to express my interest in the {jobTitle} position at {companyName}. " +
                                            $"With a background in {resume.Education} and experience including {resume.Experiences}, " +
                                            $"I am excited about the opportunity to contribute to your team.\n\n" +
                                            $"Based on the job description, I believe my skills in {relevantSkills} align well with the requirements. " +
                                            $"In particular, my experience in {resume.Experiences} has equipped me with the necessary {string.Join(", ", requirementKeywords)} " +
                                            $"to excel in this role.\n\n" +
                                            $"Please find my detailed resume attached for your review. I am looking forward to discussing how " +
                                            $"my background, skills, and enthusiasms can contribute to the success of {companyName}.\n\n" +
                                            $"Thank you for considering my application. I am eager to learn more about the {jobTitle} role " +
                                            $"and discuss how I can be an asset to your team.\n\n" +
                                            $"Sincerely,\n{fullName}";

                return coverLetterContent;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
