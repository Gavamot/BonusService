using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
namespace BonusService.Test;

public static class Ext
{
    public static T GetRequiredService<T>(this IServiceScope scope) where T : notnull => scope.ServiceProvider.GetRequiredService<T>();
    public static T ToJsonClone<T>(this T obj) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(obj)) ?? throw new InvalidOperationException();
}
