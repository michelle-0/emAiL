using EfFuncCallSK.Plugins;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;


using EfFuncCallSK.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IConfiguration _config;
    private readonly UserManager<IdentityUser> _userManager; // Inject UserManager for user management
    private readonly ChatService _chatService; // Your service for handling chat data
    [BindProperty]
    public string JobDescription { get; set; }  // Ensure this is bound if not already

    [BindProperty]
    public string ResumeJson { get; set; }

    [BindProperty]
    public string? Reply { get; set; }

    [BindProperty]
    public string? Service { get; set; }

    public List<EfFuncCallSK.Models.ChatHistory> ChatHistories { get; set; } = new List<EfFuncCallSK.Models.ChatHistory>();

    public IndexModel(ILogger<IndexModel> logger, IConfiguration config, UserManager<IdentityUser> userManager, ChatService chatService)
    {
        _logger = logger;
        _config = config;
        _userManager = userManager;
        _chatService = chatService;
        Service = _config["AIService"]!;
    }

    public async Task OnGetAsync()
    {
        string userId = _userManager.GetUserId(User); // Get current user's ID
        ChatHistories = await _chatService.GetChatHistoryByUserIdAsync(userId); // Fetch chat history for the user
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var response = await CallFunction(JobDescription, ResumeJson);
        Reply = response;

        string userId = _userManager.GetUserId(User);
        if (userId != null)
        {
            await _chatService.SaveChatMessageAsync(JobDescription, Reply, userId);
        }

        return Page();
    }


    private async Task<string> CallFunction(string jobDescription, string resumeJson)
    {
        var builder = Kernel.CreateBuilder();
        if (Service!.ToLower() == "openai")
            builder.Services.AddOpenAIChatCompletion(_config["OpenAiSettings:ModelType"], _config["OpenAiSettings:ApiKey"]);
        else
            builder.Services.AddAzureOpenAIChatCompletion(_config["AzureOpenAiSettings:Model"], _config["AzureOpenAiSettings:Endpoint"], _config["AzureOpenAiSettings:ApiKey"]);

        builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));
        builder.Plugins.AddFromType<EmailPlugin>();
        var kernel = builder.Build();

        // Set up the chat history to include an invocation of the custom kernel function
        var history = new Microsoft.SemanticKernel.ChatCompletion.ChatHistory();
        history.AddUserMessage($"Generate a job application email using the job description: {jobDescription} and resume: {resumeJson}");

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Set execution settings to invoke kernel functions automatically
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        string fullMessage = "";
        // Get the response from the AI
        var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
            history,
            executionSettings: openAIPromptExecutionSettings,
            kernel: kernel
        );

        // Process the streamed responses
        await foreach (var content in result)
        {
            fullMessage += content.Content;
        }

        // Add the message to the chat history
        history.AddAssistantMessage(fullMessage);

        return fullMessage;
    }


    public async Task<IActionResult> OnGetSearchAsync(string searchTerm)
    {
        string userId = _userManager.GetUserId(User); // Ensure the user is logged in and get their ID
        if (!string.IsNullOrEmpty(searchTerm))
        {
            // Perform the search and order by timestamp descending
            ChatHistories = await _chatService.SearchChatHistoryAsync(userId, searchTerm);
        }
        else
        {
            // If no search term is provided, get the regular chat history
            ChatHistories = await _chatService.GetChatHistoryByUserIdAsync(userId);
        }

        return Page(); // Stay on the same page, updating the ChatHistories model property
    }

}