using EfFuncCallSK.Plugins;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using EfFuncCallSK.Models;
public class IndexModel : PageModel
{

    private readonly ILogger<IndexModel> _logger;
    private readonly IConfiguration _config;
    private readonly UserManager<IdentityUser> _userManager; // Inject UserManager for user management
    private readonly ChatService _chatService; // Your service for handling chat data
    [BindProperty]
    public string? Topic { get; set; }
    [BindProperty]
    public string? Formality { get; set; }

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

    public async Task<IActionResult> OnPostAsync(string prompt)
    {
        var response = await CallFunction(prompt, Formality, Topic);
        Reply = response;

        string userId = _userManager.GetUserId(User); // Get current user's ID
        await _chatService.SaveChatMessageAsync(prompt, response, userId); // Save the new chat message

        ChatHistories = await _chatService.GetChatHistoryByUserIdAsync(userId); // Refresh chat history
        return Page();
    }

    private async Task<string> CallFunction(string question, string formality, string topic)
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
        builder.Plugins.AddFromType<EmailPlugin>();
        var kernel = builder.Build();

        string aiPrompt = $"Based on the context '{question}', generate an email with a '{formality}' formality level about the topic of '{topic}'. Please craft the email accordingly.";
        // Create chat history
        Microsoft.SemanticKernel.ChatCompletion.ChatHistory history = [];
        // Get chat completion service
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        // Get user input
        history.AddUserMessage(aiPrompt);
        // Enable auto function calling
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };
        // Get the response from the AI
        var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
          history,
          executionSettings: openAIPromptExecutionSettings,
          kernel: kernel);
        string fullMessage = "";
        await foreach (var content in result)
        {
            fullMessage += content.Content;
        }
        // Add the message to the chat history
        history.AddAssistantMessage(fullMessage);
        return fullMessage ;
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