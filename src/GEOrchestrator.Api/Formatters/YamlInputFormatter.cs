using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using SharpYaml.Serialization;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GEOrchestrator.Api.Formatters
{
    public class YamlInputFormatter : TextInputFormatter
    {
        private readonly Serializer _deserializer;
 
        public YamlInputFormatter()
        {
            _deserializer = new Serializer();
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationYaml);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.TextYaml);
        }
 
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
 
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }
 
            var request = context.HttpContext.Request;
            using var reader = new StreamReader(request.Body, encoding);
            var type = context.ModelType;
 
            try
            {
                var body = await reader.ReadToEndAsync();
                var model = _deserializer.Deserialize(body, type);
                return await InputFormatterResult.SuccessAsync(model);
            }
            catch (Exception)
            {
                return await InputFormatterResult.FailureAsync();
            }
        }
    }
 
    internal class MediaTypeHeaderValues
    {
        public static readonly MediaTypeHeaderValue ApplicationYaml = MediaTypeHeaderValue.Parse("application/x-yaml").CopyAsReadOnly();
 
        public static readonly MediaTypeHeaderValue TextYaml = MediaTypeHeaderValue.Parse("text/yaml").CopyAsReadOnly();
    }
}
