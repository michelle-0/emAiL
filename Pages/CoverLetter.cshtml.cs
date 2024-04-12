using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using EfFuncCallSK.Data;
using EfFuncCallSK.Models;
using EfFuncCallSK.Plugins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Newtonsoft.Json;


namespace EfFuncCallSK.Pages;
public class CoverLetterModel : PageModel
{
    private readonly ILogger<ResumeReviewModel> _logger;
    private readonly IConfiguration _config;

    private readonly ApplicationDbContext _context;
    public Resume Resume { get; set; }

    public JobDescription JobDescription { get; set; }

    [BindProperty]
    public string? Reply { get; set; }

    [BindProperty]
    public string? Service { get; set; }

    public CoverLetterModel(ILogger<ResumeReviewModel> logger, IConfiguration config, ApplicationDbContext context)
    {
        _logger = logger;
        _config = config;
        _context = context;
        Service = _config["AIService"]!;

    }

    //  public void OnGet(string fullName, string email, string experience, string education, string skills, string projects, JobDescription jobDescription)
    //     {
    //         Resume = new Resume(fullName, email, experience, education, skills, projects);
    //         jobDescription = new JobDescription(fullName, email, experience, projects);

    //     }

    public async Task<IActionResult> OnPostAsync(
            string fullName,
            string email,
            string experience,
            string education,
            string skills,
            string projects,
            string CompanyName,
            string JobTitle,
            string jobResponsibilities,
            string JobRequirements)
    {
        var jobDescription = new JobDescription(CompanyName, JobTitle, jobResponsibilities, JobRequirements);
        var resumeData = new Resume(fullName, email, experience, education, skills, projects);

        var jsonResumeData = JsonConvert.SerializeObject(resumeData);
        var jsonJobDescription = JsonConvert.SerializeObject(jobDescription);

        var response = await CallFunction(jsonResumeData, jsonJobDescription);

        var coverLetter = new CoverLetter
        {
            FullName = fullName,
            Email = email,
            CompanyName = CompanyName,
            JobTitle = JobTitle,
            Content = response
        };

        _context.CoverLetters.Add(coverLetter);
        await _context.SaveChangesAsync();

        Reply = response;
        return Page();
    }


    private async Task<string> CallFunction(string jsonResume, string jsonJob)
    {
        string azEndpoint = _config["AzureOpenAiSettings:Endpoint"]!;
        string azApiKey = _config["AzureOpenAiSettings:ApiKey"]!;
        string azModel = _config["AzureOpenAiSettings:Model"]!;
        string oaiModelType = _config["OpenAiSettings:ModelType"]!;
        string oaiApiKey = _config["OpenAiSettings:ApiKey"]!;
        string oaiModel = _config["OpenAiSettings:Model"]!;
        string oaiOrganization = _config["OpenAiSettings:Organization"]!;
        var builder = Kernel.CreateBuilder();
        if (Service!.ToLower() == "openai")
            builder.Services.AddOpenAIChatCompletion(oaiModelType, oaiApiKey);
        else
            builder.Services.AddAzureOpenAIChatCompletion(azModel, azEndpoint, azApiKey);
        builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));
        builder.Plugins.AddFromType<CoverLetterPlugin>();
        var kernel = builder.Build();

        var resumeData = JsonConvert.DeserializeObject<Resume>(jsonResume);
        var jobDescription = JsonConvert.DeserializeObject<JobDescription>(jsonJob);

        Microsoft.SemanticKernel.ChatCompletion.ChatHistory history = [];

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        history.AddUserMessage($"Based on the job description for the {jobDescription.JobTitle} position at {jobDescription.CompanyName}, here are the key requirements:");


        foreach (var requirement in jobDescription.JobRequirements)
        {
            history.AddUserMessage($"- {requirement}");
        }

        history.AddUserMessage("- Experience");
        history.AddUserMessage("- Skills");
        history.AddUserMessage("- Projects");

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);
        string fullMessage = "";
        await foreach (var content in result)
        {
            fullMessage += content.Content;
        }

        history.AddAssistantMessage(fullMessage);
        return fullMessage;
    }
}
