namespace BSE.Tunes.WinUI.Client.Helpers;

public static class Json
{
    public static T ToObject<T>(string value)
    {
        var result = System.Text.Json.JsonSerializer.Deserialize<T>(value);

        if (result is null)
        {
            throw new InvalidOperationException("Deserialization returned null.");
        }

        return result;
    }

    public static string Stringify(object value)
    {
        return System.Text.Json.JsonSerializer.Serialize(value);
    }
}
