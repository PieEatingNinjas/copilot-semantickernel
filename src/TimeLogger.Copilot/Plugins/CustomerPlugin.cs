using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace TimeLogger.Copilot.Plugins;

public class CustomerPlugin
{
    [KernelFunction]
    [Description(
        "Retrieves a list of all the companies the current user can book hours on in a dictionary (key = id, value = name)")]
    [return: Description("The dictionary containing all the customers.")]
    public async Task<Dictionary<int, string>> GetAllCustomers(
        [Description("The user id of the currently logged-in user")]
        int userId,
        Kernel kernel
    )
    {
        //Example: retrieving the token from an OAuthTokenProvider
        //which could be used to perform an authenticated request
        var tokenProvider = kernel.Services.GetRequiredService<IOAuthTokenProvider>();
        string token = await tokenProvider.AcquireToken();

        var httpClient = new HttpClient();

        var result = await httpClient.GetAsync($"https://localhost:7001/Customers?userId={userId}");
        var json = await result.Content.ReadAsStringAsync();

        var companies = JsonSerializer.Deserialize<CustomerDto[]>(json, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return companies?.ToDictionary(m => m.Id, m => m.Name) 
               ?? new Dictionary<int, string>();
    }

    private record CustomerDto(int Id, string Name);
}