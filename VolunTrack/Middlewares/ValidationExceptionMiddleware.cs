namespace VolunTrack.Middlewares
{
    public class ValidationExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            if (context.Response.StatusCode == 400 &&
                !string.IsNullOrEmpty(context.Response.ContentType) &&
                context.Response.ContentType.Contains("json"))
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                var body = await new StreamReader(responseBody).ReadToEndAsync();

                if (body.Contains("Request body too large"))
                {
                    context.Response.Body = originalBodyStream;
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = 413;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "Файл слишком большой. Максимальный размер: 30 MB"
                    });
                    return;
                }
            }

            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
    }
}
