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
        
        app.MapHub<NotificationHub>("/hubs/notifications");
        app.MapHub<AdminHub>("/hubs/admin");
        return app;
    }

}