using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace EfFuncCallSK.Plugins
{
    public class EmailPlugin
    {
        [KernelFunction, Description("Make the email better")]
        public static string? MakeEmailBetter(
            [Description("The email to improve")]
            string email
        )
        {
            return email.Replace(" ", ".");
        }

        [KernelFunction, Description("Generate an auto email for the context")]
        public static string? GenerateAutoEmail(
        [Description("The context to generate the email for")]
        string context,
        [Description("The formality level of the email")] string formality,
        [Description("The topic of the email")] string topic
        )
        {
            string emailPurpose;
            string actionStatement;
            string greeting;

            if (formality == "formal")
            {
                greeting = "Dear [Recipient],\n\n";
            }
            else if (formality == "casual") {
                greeting = "How is it going [Recipient],\n\n";
            }
            else if (formality == "business") {
                greeting = "Hello [Recipient],\n\n";
            }
            else {
                greeting = "Dear [Recipient],\n\n";
            }

            if (topic == "apology" || topic == "sorry" || topic == "regret")
            {
                emailPurpose = "an apology for my recent actions";
                actionStatement = "I realize my actions were inappropriate and am deeply sorry for any inconvenience or disappointment I may have caused. I am eager to make amends and ensure that such an incident does not occur again.";
            }
            else if (topic == "request" || topic == "help" || topic == "assistance")
            {
                emailPurpose = "to make a request";
                actionStatement = "I am reaching out to kindly ask for your assistance on a matter of importance to me. Your guidance or support would be greatly appreciated.";
            }
            else if (topic == "meeting" || topic == "schedule" || topic == "appointment")
            {
                emailPurpose = "to schedule a meeting";
                actionStatement = "I would like to discuss a matter of importance and am hoping we can schedule a meeting at your earliest convenience.";
            }
            else
            {
                emailPurpose = "regarding a matter";
                actionStatement = "I am writing to discuss a matter that recently came to my attention. I believe it is in our best interest to address it promptly.";
            }

            return $"{greeting},\n\nI hope this email finds you well. I am writing to you {emailPurpose}. {actionStatement}\n\nPlease let me know your thoughts or when would be a convenient time for us to discuss this further.\n\nBest regards,\n\n[Your Name]";
        }


    }
}