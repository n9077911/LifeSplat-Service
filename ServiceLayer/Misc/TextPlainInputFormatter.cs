using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace ServiceLayer.Misc
{
    //Needed to avoid a localhost problem whereby cors prevents the sending of application/json content-type. 
    //This feels like a hack. According to the internet its widely used.
    //https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/custom-formatters?view=aspnetcore-3.1
    //copied from https://github.com/aspnet/Entropy/blob/master/samples/Mvc.Formatters/TextPlainInputFormatter.cs
    public class TextPlainInputFormatter : TextInputFormatter
    {
        public TextPlainInputFormatter()
        {
            SupportedMediaTypes.Add("text/plain");
            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
            SupportedEncodings.Add(UTF16EncodingLittleEndian);
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(string);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            string data;

            using (var reader = new StreamReader(context.HttpContext.Request.Body, encoding, leaveOpen: true))
                data = await reader.ReadToEndAsync();

            return InputFormatterResult.Success(data);
        }
    }
}