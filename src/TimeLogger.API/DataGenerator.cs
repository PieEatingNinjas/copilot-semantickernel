using Microsoft.EntityFrameworkCore;
using TimeLogger.API.Entities;

namespace TimeLogger.API;

public class DataGenerator
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new ApiContext(
                   serviceProvider.GetRequiredService<DbContextOptions<ApiContext>>()))
        {
            if (context.Customers.Any())
                return;

            context.Customers.AddRange(
                new Customer() { Id = 1, Name = "Xebia" },
            new Customer(){ Id  = 2, Name = "Microsoft"},
            new Customer(){ Id  = 3, Name = "Apple"},
            new Customer(){ Id  = 4, Name = "Google"},
            new Customer(){ Id  = 5, Name = "Amazon"});

            context.SaveChanges();
        }
    }
}