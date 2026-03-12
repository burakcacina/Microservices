using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

namespace Infrastructure.Util;

public static class Helper
{
    public static string? HeaderAsString(IBasicProperties? props, string key)
    {
        if (props?.Headers is null || !props.Headers.TryGetValue(key, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            ReadOnlyMemory<byte> rom => Encoding.UTF8.GetString(rom.Span),
            string s => s,
            _ => value.ToString()
        };
    }
}
