using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using CsvHelper.TypeConversion;

namespace SeatingOptimizer
{
    // Represents a person who wants to attend
    public class SeatRequest()
    {
        public string Name { get; set; }
        public int Id { get; set; }
        // List of people that this person wants to be adjacent to, as strings containing names
        public List<string> RequestedAdjacentsString { get; set; } = new List<string>();
        // List of people that this person wants to be adjacent to, as ID numbers representing the people
        public List<int> RequestedAdjacentsId { get; set; } = new List<int>();
    }
    public class SeatRequestMap : ClassMap<SeatRequest>
    {
        public SeatRequestMap(string nameColumnName, string requestedColumnName)
        {
            Map(m => m.Name).Name(nameColumnName);
            Map(m => m.RequestedAdjacentsString).Name(requestedColumnName).TypeConverter<CustomListConverter>();
            Map(m => m.Id).Ignore();
            Map(m => m.RequestedAdjacentsId).Ignore();

        }
    }
    public class CustomListConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string>();

            return text.Split(',')
                .Select(x => x.Trim())
                .ToList();
        }
    }
}
