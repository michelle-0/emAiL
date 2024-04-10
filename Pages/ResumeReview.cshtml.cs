using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfFuncCallSK.Plugins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;


namespace EfFuncCallSK.Pages;
public class ResumeReviewModel : PageModel
{
    private readonly ILogger<ResumeReviewModel> _logger;
    private readonly IConfiguration _config;

    [BindProperty]
    public string? Reply { get; set; }
    
    [BindProperty]
    public string? Service { get; set; }

    public ResumeReviewModel(ILogger<ResumeReviewModel> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
        Service = _config["AIService"]!;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(
    string? experience, 
    string? education, 
    string? skills, 
    string? projects, 
    string? CompanyName, 
    string? JobTitle, 
    string? jobResponsibilities, 
    string? JobRequirements) {
    // call the Azure Function
    var response = await CallFunction(
    experience, 
    education, 
    skills, 
    projects, 
    CompanyName, 
    JobTitle, 
    jobResponsibilities, 
    JobRequirements);

    Reply = response;
    return Page();
  }

    private async Task<string> CallFunction(
        string? experience, 
        string? education, 
        string? skills, 
        string? projects, 
        string? CompanyName, 
        string? JobTitle, 
        string? jobResponsibilities, 
        string? JobRequirements)
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
        builder.Plugins.AddFromType<AdjustResumePlugin>();
        var kernel = builder.Build();
        ChatHistory history = [];
        // Get chat completion service
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Get user input
        history.AddUserMessage($"I need you to help me land a job as a {JobTitle} at {CompanyName} by tidying up my resume.");
        string message = $"The following are details of my current resume: Experience: {experience}, Education: {education}, Skills: {skills}, Projects: {projects}. Taking all of the above into account, tailor them according to the job description's requirements and details which are listed here: Job Responsibilities: {jobResponsibilities}, Job Requirements: {JobRequirements}";
        history.AddUserMessage(message);
        // Enable auto function calling
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new() {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };
        // Get the response from the AI
        var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);
        string fullMessage = "";
        await foreach (var content in result) {
        fullMessage += content.Content;
        }
        // Add the message to the chat history
        history.AddAssistantMessage(fullMessage);
        return fullMessage;

    }

}
