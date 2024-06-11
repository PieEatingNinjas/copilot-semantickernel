using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace TimeLogger.Demo.Plugins;

public class CustomersPlugin
{
    [KernelFunction]
    [Description(
        "Retrieves a list of all the companies the current user can book hours on in a dictionary (key = id, value = name)")]
    [return: Description("The dictionary containing all the customers with their id (key).")]
    public async Task<Dictionary<int, string>> GetAllCustomers(
        [Description("The user id of the currently logged-in user")]
        int userId,
        Kernel kernel
    )
    {
        var httpClient = new HttpClient();

        var result = await httpClient.GetAsync($"https://localhost:7001/Customers?userId={userId}");

        var json = await result.Content.ReadAsStringAsync();

        var companies = JsonSerializer.Deserialize<CustomerDto[]>(json,
            new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        return companies.ToDictionary(m => m.Id, m => m.Name);
    }

    private record CustomerDto(int Id, string Name);
}