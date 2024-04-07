using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Microsoft.SemanticKernel;

namespace TimeLogger.Copilot.Plugins;

public class TimesheetPlugin
{
    [KernelFunction]
    [Description("Book the given time (in total minutes) for the given date for the given customer. A user confirmation should be asked prior to calling this!")]
    [return: Description("The result of the booking action. Either 'OK' or an error message.")]
    public async Task<string> Book(
        Kernel kernel,
        [Description("The id of the current user")] string userId,
        [Description("The id of the customer")] int customerId,
        [Description("The date in dd/MM/yyyy format")] string date,
        [Description("The amount of hours in minutes")] int minutes
    )
    {
        //Call API
        var days = DateOnly.ParseExact(date, "dd/MM/yyyy").DayNumber;
        
        var dto = new AddTimesheetEntryDto(userId, customerId, days, minutes);

        var client = new HttpClient();
        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

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
    
    [KernelFunction]
    [Description("Get the booked hours for the given period")]
    [return: Description("The summary of booked hours for the given period")]
    public async Task<string> GetBookings(
        Kernel kernel,
        [Description("The date from in dd/MM/yyyy format")] string fromDate,
        [Description("The date until in dd/MM/yyyy format")] string untilDate
    )
    {
        var fromDays = DateOnly.ParseExact(fromDate, "dd/MM/yyyy").DayNumber;
        var untilDays = DateOnly.ParseExact(untilDate, "dd/MM/yyyy").DayNumber;
        
        var client = new HttpClient();
        var result =
            await client.GetAsync($"https://localhost:7001/TimesheetEntry?dayFrom={fromDays}&dayUntil={untilDays}");
        
        var json = await result.Content.ReadAsStringAsync();
        
        var entries = JsonSerializer.Deserialize<TimeSheetEntryDto[]>(json, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return entries?.ToString() ?? string.Empty;
    }
    
    private record AddTimesheetEntryDto(string UserId, int CustomerId, int DayNumber, int Minutes);
    private record TimeSheetEntryDto(int DayNumber, int Minutes, CustomerDto Customer);
    private record CustomerDto(int Id, string Name);
}