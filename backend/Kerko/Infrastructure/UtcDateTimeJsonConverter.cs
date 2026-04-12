using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kerko.Infrastructure;

/// <summary>
/// Ensures DateTime values are always serialized with a Z suffix (UTC).
/// EF Core + SQLite reads DateTime as DateTimeKind.Unspecified, which
/// System.Text.Json serializes without Z — breaking frontend UTC parsing.
/// </summary>
public class UtcDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetDateTime();

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utc = value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();

        writer.WriteStringValue(utc.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}
