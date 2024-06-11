using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Microsoft.SemanticKernel;

namespace TimeLogger.Demo.Plugins;

public class TimesheetPlugin
{
    [KernelFunction]
    [Description("Book the given time (in total minutes) for the given date for the given customer. " +
                 "A user confirmation should be asked prior to calling this!")]
    [return: Description("The result of the booking action. Either 'OK' or an error message.")]
    public async Task<string> Book(
        [Description("The id of the current user")]
        string userId,
        [Description("The id of the customer")]
        int customerId,
        [Description("The date in dd/MM/yyyy format")]
        string date,
        [Description("The amount of hours in minutes")]
        int minutes
    )
    {
        var days = DateOnly.ParseExact(date, "dd/MM/yyyy").DayNumber;

        var dto = new AddTimesheetEntryDto(userId, customerId, days, minutes);

        var client = new HttpClient();
        var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

        try
        {
            var result = await client.PostAsync($"https://localhost:7001/TimesheetEntry", content);

            if (result.IsSuccessStatusCode)
                return "OK";
            else
                return await result.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }

    private record AddTimesheetEntryDto(string UserId, int CustomerId, int DayNumber, int Minutes);
}