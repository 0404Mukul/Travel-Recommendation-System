using System.Globalization;
using System.Text;
using TravelRecommendationEngine.Models;

namespace TravelRecommendationEngine.Services;

public sealed class CsvLoaderService : ICsvLoaderService
{
    private const string DataFolderName = "Data";
    private readonly string _dataFolderPath;
    private readonly Lazy<TravelDataSnapshot> _snapshot;

    public CsvLoaderService(IWebHostEnvironment environment)
        : this(Path.Combine(environment.ContentRootPath, DataFolderName))
    {
    }

    public CsvLoaderService(string dataFolderPath)
    {
        _dataFolderPath = dataFolderPath;
        _snapshot = new Lazy<TravelDataSnapshot>(() => TravelDataSnapshot.Create(LoadFromFiles()));
    }

    public TravelData LoadTravelData() => _snapshot.Value.Data;

    public TravelDataSnapshot GetSnapshot() => _snapshot.Value;

    private TravelData LoadFromFiles()
    {
        return new TravelData
        {
            Destinations = ReadCsv("Expanded_Destinations.csv", MapDestination),
            Users = ReadCsv("Final_Updated_Expanded_Users.csv", MapUser),
            Reviews = ReadCsv("Final_Updated_Expanded_Reviews.csv", MapReview),
            TravelHistories = ReadCsv(ResolveTravelHistoryFileName(), MapTravelHistory),
            Hotels = ReadOptionalCsv("Hotels.csv", MapHotel, GetDefaultHotels()),
            Activities = ReadOptionalCsv("Activities.csv", MapActivity, GetDefaultActivities())
        };
    }

    private string ResolveTravelHistoryFileName()
    {
        var expandedUserHistoryName = Path.Combine(_dataFolderPath, "Final_Updated_Expanded_UserHistory.csv");
        if (File.Exists(expandedUserHistoryName))
        {
            return "Final_Updated_Expanded_UserHistory.csv";
        }

        var underscoredName = Path.Combine(_dataFolderPath, "Travel_History.csv");
        if (File.Exists(underscoredName))
        {
            return "Travel_History.csv";
        }

        return "TravelHistory.csv";
    }

    private IReadOnlyList<T> ReadOptionalCsv<T>(string fileName, Func<IReadOnlyDictionary<string, string>, T> map, IReadOnlyList<T> fallback)
    {
        var path = Path.Combine(_dataFolderPath, fileName);
        return File.Exists(path) ? ReadCsv(fileName, map) : fallback;
    }

    private IReadOnlyList<T> ReadCsv<T>(string fileName, Func<IReadOnlyDictionary<string, string>, T> map)
    {
        var path = Path.Combine(_dataFolderPath, fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"CSV file '{fileName}' was not found in '{_dataFolderPath}'.", path);
        }

        var lines = File.ReadAllLines(path);
        if (lines.Length == 0)
        {
            return [];
        }

        var headers = ParseCsvLine(lines[0]);
        var records = new List<T>();

        foreach (var line in lines.Skip(1).Where(line => !string.IsNullOrWhiteSpace(line)))
        {
            var values = ParseCsvLine(line);
            var row = headers
                .Select((header, index) => new { header, value = index < values.Count ? values[index] : string.Empty })
                .ToDictionary(item => item.header, item => item.value, StringComparer.OrdinalIgnoreCase);

            records.Add(map(row));
        }

        return records;
    }

    private static Destination MapDestination(IReadOnlyDictionary<string, string> row)
    {
        return new Destination
        {
            DestinationId = GetInt(row, "DestinationID"),
            Name = GetString(row, "Name"),
            State = GetString(row, "State"),
            Type = GetString(row, "Type"),
            Popularity = GetDouble(row, "Popularity"),
            BestTimeToVisit = GetString(row, "BestTimeToVisit")
        };
    }

    private static User MapUser(IReadOnlyDictionary<string, string> row)
    {
        return new User
        {
            UserId = GetInt(row, "UserID"),
            Preferences = GetString(row, "Preferences"),
            Gender = GetString(row, "Gender"),
            NumberOfAdults = GetInt(row, "NumberOfAdults"),
            NumberOfChildren = GetInt(row, "NumberOfChildren")
        };
    }

    private static Review MapReview(IReadOnlyDictionary<string, string> row)
    {
        return new Review
        {
            ReviewId = GetInt(row, "ReviewID"),
            DestinationId = GetInt(row, "DestinationID"),
            UserId = GetInt(row, "UserID"),
            Rating = GetDouble(row, "Rating"),
            ReviewText = GetString(row, "ReviewText")
        };
    }

    private static TravelHistory MapTravelHistory(IReadOnlyDictionary<string, string> row)
    {
        return new TravelHistory
        {
            HistoryId = GetInt(row, "HistoryID"),
            UserId = GetInt(row, "UserID"),
            DestinationId = GetInt(row, "DestinationID"),
            VisitDate = DateOnly.Parse(GetString(row, "VisitDate"), CultureInfo.InvariantCulture),
            ExperienceRating = GetDouble(row, "ExperienceRating")
        };
    }

    private static Hotel MapHotel(IReadOnlyDictionary<string, string> row)
    {
        return new Hotel
        {
            HotelId = GetInt(row, "HotelID"),
            DestinationId = GetInt(row, "DestinationID"),
            Name = GetString(row, "Name"),
            StarRating = GetInt(row, "StarRating"),
            PricePerNight = GetDouble(row, "PricePerNight"),
            Amenities = GetString(row, "Amenities")
        };
    }

    private static Activity MapActivity(IReadOnlyDictionary<string, string> row)
    {
        return new Activity
        {
            ActivityId = GetInt(row, "ActivityID"),
            DestinationId = GetInt(row, "DestinationID"),
            Name = GetString(row, "Name"),
            Category = GetString(row, "Category"),
            DurationHours = GetDouble(row, "DurationHours"),
            Difficulty = GetString(row, "Difficulty"),
            Popularity = GetDouble(row, "Popularity")
        };
    }

    private static IReadOnlyList<Hotel> GetDefaultHotels() =>
        new List<Hotel>
        {
            new Hotel { HotelId = 1, DestinationId = 1, Name = "Seaview Resort", StarRating = 5, PricePerNight = 140, Amenities = "Pool, Spa, Beach Access" },
            new Hotel { HotelId = 2, DestinationId = 1, Name = "Beachside Inn", StarRating = 4, PricePerNight = 90, Amenities = "Breakfast, Sea View" },
            new Hotel { HotelId = 3, DestinationId = 2, Name = "Heritage Palace", StarRating = 5, PricePerNight = 160, Amenities = "Pool, Guided Tours" },
            new Hotel { HotelId = 4, DestinationId = 3, Name = "Hilltop Retreat", StarRating = 4, PricePerNight = 110, Amenities = "Garden, Hiking Access" }
        };

    private static IReadOnlyList<Activity> GetDefaultActivities() =>
        new List<Activity>
        {
            new Activity { ActivityId = 1, DestinationId = 1, Name = "Sunset Cruise", Category = "Relaxation", DurationHours = 3, Difficulty = "Easy", Popularity = 9.0 },
            new Activity { ActivityId = 2, DestinationId = 1, Name = "Beach Yoga", Category = "Wellness", DurationHours = 1.5, Difficulty = "Easy", Popularity = 8.4 },
            new Activity { ActivityId = 3, DestinationId = 2, Name = "City Fort Tour", Category = "Cultural", DurationHours = 4, Difficulty = "Moderate", Popularity = 8.7 },
            new Activity { ActivityId = 4, DestinationId = 3, Name = "Tea Plantation Walk", Category = "Nature", DurationHours = 3, Difficulty = "Easy", Popularity = 8.2 }
        };

    private static IReadOnlyList<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"')
            {
                if (inQuotes && index + 1 < line.Length && line[index + 1] == '"')
                {
                    current.Append('"');
                    index++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (character == ',' && !inQuotes)
            {
                values.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(character);
            }
        }

        values.Add(current.ToString().Trim());
        return values;
    }

    private static string GetString(IReadOnlyDictionary<string, string> row, string columnName) =>
        row.TryGetValue(columnName, out var value) ? value : string.Empty;

    private static int GetInt(IReadOnlyDictionary<string, string> row, string columnName) =>
        int.Parse(GetString(row, columnName), CultureInfo.InvariantCulture);

    private static double GetDouble(IReadOnlyDictionary<string, string> row, string columnName) =>
        double.Parse(GetString(row, columnName), CultureInfo.InvariantCulture);
}
