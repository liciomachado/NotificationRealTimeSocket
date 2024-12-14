namespace RealTimeDriverTracking;

public static class ExtensionMethods
{
    public static void CustomUseHttpsRedirection(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                await next();
            }
            else
            {
                app.UseHttpsRedirection();
                await next();
            }
        });
    }

    public static IServiceCollection CustomAddCors(this IServiceCollection service)
    {
        service.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins",
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
        return service;
    }
}