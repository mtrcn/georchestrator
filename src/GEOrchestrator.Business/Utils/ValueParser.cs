using System.Text.RegularExpressions;

namespace GEOrchestrator.Business.Utils
{
    public static class ValueParser
    {
        public static (string stepId, string artifactName) Parse(string value)
        {
            var valuePattern = @"^{{step\.([a-zA-Z0-9]+)\.([a-zA-Z0-9\._]+)}}$";
            var regexResult = Regex.Match(value, valuePattern);
            return (regexResult.Groups[1].Value, regexResult.Groups[2].Value);
        }
    }
}
