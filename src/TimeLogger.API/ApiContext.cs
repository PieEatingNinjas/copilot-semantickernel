using Microsoft.EntityFrameworkCore;
using TimeLogger.API.Entities;

namespace TimeLogger.API;

public class ApiContext : DbContext
{
    public ApiContext(DbContextOptions<ApiContext> options) : base(options)
    { }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<TimeSheetEntry> Entries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<TimeSheetEntry>()
            .Property(p => p.Id)
            .ValueGeneratedOnAdd();
    }
}