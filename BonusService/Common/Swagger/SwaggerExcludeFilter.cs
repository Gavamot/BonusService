using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace BonusService.Common.Swagger;

[AttributeUsage(AttributeTargets.Property)]
public class SwaggerExcludeAttribute : Attribute
{
}

public class PathPrefixInsertDocumentFilter : IDocumentFilter
{
    private readonly string _pathPrefix;

    public PathPrefixInsertDocumentFilter(string prefix)
    {
        this._pathPrefix = prefix;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var paths = swaggerDoc.Paths.Keys.ToList();
        foreach (var path in paths)
        {
            var pathToChange = swaggerDoc.Paths[path];
            swaggerDoc.Paths.Remove(path);
            swaggerDoc.Paths.Add($"{_pathPrefix}{path}", pathToChange);
        }
    }
}

public class SwaggerExcludeFilter : ISchemaFilter
{

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema?.Properties == null || context.Type == null)
            return;

        var excludedProperties = context.Type.GetProperties()
            .Where(t => t.GetCustomAttribute(typeof(SwaggerExcludeAttribute)) != null);

        foreach (var excludedProperty in excludedProperties)
        {
            if (schema.Properties.ContainsKey(excludedProperty.Name))
                schema.Properties.Remove(excludedProperty.Name);
        }
    }
}
