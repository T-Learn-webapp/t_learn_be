using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TLearn.Domain.Entities;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // === Database & Identity ===
        services.AddDbContext<TLearnDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false; // Có thể đổi sau
            })
            .AddEntityFrameworkStores<TLearnDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
    
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Đăng ký MediatR (nếu dùng)
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        // Đăng ký AutoMapper (nếu dùng)
        // services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        // Đăng ký Services sau này
        // services.AddScoped<IAIService, AIService>();
        // services.AddScoped<IQuizService, QuizService>();

        return services;
    }
}