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

    // public IndexModel(ILogger<IndexModel> logger, IConfiguration config, UserManager<IdentityUser> userManager)
    // {
    //     _logger = logger;
    //     _config = config;
    //     _userManager = userManager;
    //     Service = _config["AIService"]!;
    // }

    public async Task OnGetAsync()
    {
        string userId = _userManager.GetUserId(User); // Get current user's ID
        ChatHistories = await _chatService.GetChatHistoryByUserIdAsync(userId); // Fetch chat history for the user
    }

    public async Task<IActionResult> OnPostAsync(string prompt)
    {
        var response = await CallFunction(prompt);
        Reply = response;

        string userId = _userManager.GetUserId(User); // Get current user's ID
        await _chatService.SaveChatMessageAsync(prompt, response, userId); // Save the new chat message

        ChatHistories = await _chatService.GetChatHistoryByUserIdAsync(userId); // Refresh chat history
        return Page();
    }
    //   private readonly ILogger<IndexModel> _logger;
    //   private readonly IConfiguration _config;

    //   [BindProperty]
    //   public string? Reply { get; set; }

    //   [BindProperty]
    //   public string? Service { get; set; }

    //   public List<ChatHistory> ChatHistories { get; set; } = new List<ChatHistory>();

    //   public IndexModel(ILogger<IndexModel> logger, IConfiguration config) {
    //     _logger = logger;
    //     _config = config;
    //     Service = _config["AIService"]!;
    //   }
    //   public void OnGet() { }
    //   // action method that receives prompt from the form
    //   public async Task<IActionResult> OnPostAsync(string prompt) {
    //     // call the Azure Function
    //     var response = await CallFunction(prompt);
    //     Reply = response;
    //     return Page();
    //   }

    private async Task<string> CallFunction(string question)
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
        // Create chat history
        Microsoft.SemanticKernel.ChatCompletion.ChatHistory history = [];
        // Get chat completion service
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        // Get user input
        history.AddUserMessage(question);
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
        return fullMessage;
    }
}