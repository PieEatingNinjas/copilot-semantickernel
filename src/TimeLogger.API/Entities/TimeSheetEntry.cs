namespace TimeLogger.API.Entities;

public class TimeSheetEntry
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public Customer Customer { get; set; }
    public int Minutes { get; set; }
}