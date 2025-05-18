using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GEOrchestrator.Business.Extensions
{
    public static class StringExtension
    {
        public static string GetReferenceType(this string value)
        {
            var valuePattern = @"^{{(step|input)\.([a-zA-Z0-9\._]+)}}$";
            var regexResult = Regex.Match(value, valuePattern);
            if (regexResult.Success && regexResult.Groups.Count > 1)
                return regexResult.Groups[1].Value;
            return value == "{{item}}" ? "item" : string.Empty;
        }

        public static string ParseInputReferenceValue(this string value)
        {
            var valuePattern = @"^{{input\.([a-zA-Z0-9\._]+)}}$";
            var regexResult = Regex.Match(value, valuePattern);
            return regexResult.Groups[1].Value;
        }

        public static (string stepId, string parameterName) ParseStepReferenceValue(this string value)
        {
            var valuePattern = @"^{{step\.([a-zA-Z0-9]+)\.([a-zA-Z0-9\._]+)}}$";
            var regexResult = Regex.Match(value, valuePattern);
            return (regexResult.Groups[1].Value, regexResult.Groups[2].Value);
        }

        public static List<string> ReadValues(this string value)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(value);
                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    var result = new List<string>();
                    foreach (var element in doc.RootElement.EnumerateArray())
                    {
                        if (element.ValueKind == System.Text.Json.JsonValueKind.String)
                            result.Add(element.GetString());
                        else
                            result.Add(element.ToString());
                    }
                    return result;
                }
                return new List<string> { value };
            }
            catch
            {
                return new List<string> { value };
            }
        }
    }
}
