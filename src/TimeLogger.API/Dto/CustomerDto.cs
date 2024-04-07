using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TimeLogger.API.Dto;

public record CustomerDto(int Id, string Name);

public record AddTimesheetEntryDto(string UserId, int CustomerId, int DayNumber, int Minutes);

public record TimeSheetEntryDto(int DayNumber, int Minutes, CustomerDto Customer);