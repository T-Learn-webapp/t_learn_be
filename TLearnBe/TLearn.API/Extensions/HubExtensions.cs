using TLearn.Infrastructure.Hubs;

namespace TLearn.API.Extensions;

public static class HubExtensions
{
    public static IServiceCollection AddSignalRHub(
        this IServiceCollection services)
    {
        services.AddSignalR();
        return services;
    }

    public static WebApplication MapSignalRHub(
        this WebApplication app)
    {
        app.MapHub<TodoHub>("/hubs/todo");
        app.MapHub<NotificationHub>("/hubs/notifications");
        return app;
    }

}