using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

public static class JsonSchemaValidator
{
    public static bool IsValid(string json, string schemaJson, out IList<string> errors)
    {
        var schema = JSchema.Parse(schemaJson);
        var obj = JObject.Parse(json);

        bool valid = obj.IsValid(schema, out errors);
        return valid;
    }
}