using System.Globalization;
using System.Text;
using System.Text.Json;

namespace JsonSizer;

public static class Sizer
{
    private const int MaxDepth = 64;

    public static IReadOnlyDictionary<string, long> Size(ReadOnlySpan<byte> jsonBytes)
    {
        if (jsonBytes.Length == 0)
        {
            return new Dictionary<string, long>();
        }

        var reader = new Utf8JsonReader(
            jsonBytes,
            new JsonReaderOptions { MaxDepth = MaxDepth });

        var pathParts = new string?[MaxDepth];
        var readBytes = 0L;
        var bytesPerPath = new Dictionary<string, long>();

        try
        {
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        pathParts[reader.CurrentDepth - 1] = reader.GetString();
                        break;
                    case JsonTokenType.StartArray:
                        pathParts[reader.CurrentDepth] = "[i]";
                        break;
                }

                var pathBuilder = new StringBuilder("@root");

                for (var i = 0; i < reader.CurrentDepth; i++)
                {
                    if (pathParts[i] != null)
                    {
                        pathBuilder.Append(
                            CultureInfo.InvariantCulture,
                            $";{pathParts[i]}");
                    }
                }

                var path = pathBuilder.ToString();

                if (!bytesPerPath.ContainsKey(path))
                {
                    bytesPerPath[path] = 0;
                }

                var sizeBytes = reader.BytesConsumed - readBytes;

                bytesPerPath[path] += sizeBytes;
                readBytes += sizeBytes;
            }
        }
        catch (JsonException)
        {
            // break
        }

        var brokenBytes = jsonBytes.Length - reader.BytesConsumed;
        var restBytes = reader.BytesConsumed - readBytes;

        if (bytesPerPath.ContainsKey("@root"))
        {
            bytesPerPath["@root"] += restBytes;
        }
        else
        {
            brokenBytes += restBytes;
        }

        if (brokenBytes > 0)
        {
            bytesPerPath["@broken"] = brokenBytes;
        }

        return bytesPerPath;
    }
}