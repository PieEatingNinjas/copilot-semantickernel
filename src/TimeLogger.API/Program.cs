using Microsoft.EntityFrameworkCore;
using TimeLogger.API;
using TimeLogger.API.Dto;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApiContext>(options => options.UseInMemoryDatabase("TimesheetDb"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    DataGenerator.Initialize(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/Customers", (string userId, ApiContext ctx) =>
    {
        var customers = ctx.Customers.ToList();
        return customers.Select(c => new CustomerDto(c.Id, c.Name));
    })
    .WithName("GetCustomers")
    .WithOpenApi();

app.MapPost("/TimesheetEntry", (AddTimesheetEntryDto data, ApiContext ctx) =>
    {
        //throw new Exception("It is not allowed to book time on a month that is marked as closed.");
        var customer = ctx.Customers.SingleOrDefault(c => c.Id == data.CustomerId);
        ctx.Entries.Add(new()
        {
            Customer = customer,
            Date = DateOnly.FromDayNumber(data.DayNumber),
            Minutes = data.Minutes
        });
        ctx.SaveChanges();
    })
    .WithName("AddTimesheetEntry")
    .WithOpenApi();

app.MapGet("/TimesheetEntry", (int dayFrom, int dayUntil, ApiContext ctx) =>
    {
        var entries = ctx.Entries
            .Include(e => e.Customer)
            .Where(e =>
                e.Date >= DateOnly.FromDayNumber(dayFrom) && 
                e.Date <= DateOnly.FromDayNumber(dayUntil))
            .ToList();

        return entries.Select(e =>
            new TimeSheetEntryDto(e.Date.DayNumber, e.Minutes,
                new CustomerDto(e.Customer.Id, e.Customer.Name)))
            .ToList();
    })
    .WithName("GetTimesheetEntries")
    .WithOpenApi();

app.Run();