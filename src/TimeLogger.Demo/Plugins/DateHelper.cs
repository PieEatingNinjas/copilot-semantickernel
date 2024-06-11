using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace TimeLogger.Demo.Plugins;

public class DateHelper
{
    [KernelFunction]
    [Description(
        "Get the exact date in dd/MM/yyyy format given the user's description of the date. " +
        "It is assumed the date is in the past, unless explicitly stated otherwise.")]
    [return: Description("The exact date in dd/MM/yyyy for the given user intent")]
    public async Task<string> GetDate(
        [Description("The date indication like 'yesterday', 'last Thursday', 'this month' ...")]
        string description,
        Kernel kernel
    )
    {
        var prompt = """
                     Given that the current date is: {{$currentDate}}

                     You should reason what the date is meant with: '''{{$description}}'''

                     It's important that the intended date is always in the past,
                     never in the future unless explicitly stated.
                     That means for example that "Tuesday" refers to "last week Tuesday".

                     The date (in dd/MM/yyyy format) corresponding with '''{{$description}}''' is:
                     """;

        KernelArguments arguments = new KernelArguments()
        {
            { "currentDate", DateTime.Now.ToLongDateString() },
            { "description", description },
        };

        var response = await kernel.InvokePromptAsync(prompt, arguments);

        return response.GetValue<string>();
    }
}