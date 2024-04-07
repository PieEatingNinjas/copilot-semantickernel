namespace TimeLogger.API.Entities;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<TimeSheetEntry> Entries { get; set; }
}