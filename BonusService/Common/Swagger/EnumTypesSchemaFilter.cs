using System.Xml.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BonusService.Common.Swagger;

public class EnumTypesSchemaFilter : ISchemaFilter
{
    private readonly XDocument? _xmlComments;

    public EnumTypesSchemaFilter(string xmlPath)
    {
        if (File.Exists(xmlPath))
        {
            _xmlComments = XDocument.Load(xmlPath);
        }
    }

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (_xmlComments == null) return;

        if (schema.Enum != null && schema.Enum.Count > 0 &&
            context.Type != null && context.Type.IsEnum)
        {
            schema.Description += "<p>Содержит значения:</p><ul>";

            var fullTypeName = context.Type.FullName;

            foreach (var enumMemberName in Enum.GetValues(context.Type))
            {
                var enumMemberValue = Convert.ToInt64(enumMemberName);

                var fullEnumMemberName = $"F:{fullTypeName}.{enumMemberName}";

                var enumMemberComments = _xmlComments.Descendants("member")
                    .FirstOrDefault(m => m.Attribute("name")?.Value.Equals
                        (fullEnumMemberName, StringComparison.OrdinalIgnoreCase) ?? false);

                if (enumMemberComments == null) continue;

                var summary = enumMemberComments.Descendants("summary").FirstOrDefault();

                if (summary == null) continue;

                schema.Description += $"<li><i>{enumMemberValue} ({enumMemberName})</i> - { summary.Value.Trim()}</li>";
            }

            schema.Description += "</ul>";
        }
    }
}
