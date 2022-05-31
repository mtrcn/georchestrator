using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

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
                var jsonContent = JToken.Parse(value);
                if (jsonContent is JArray)
                {
                    return jsonContent.ToObject<List<string>>();
                }

                return new List<string> {value};
            }
            catch
            {
                return new List<string> { value };
            }
        }
    }
}
