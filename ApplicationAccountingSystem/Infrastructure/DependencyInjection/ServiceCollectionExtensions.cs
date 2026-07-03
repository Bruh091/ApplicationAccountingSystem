using Microsoft.Extensions.DependencyInjection;
using ApplicationAccountingSystem.Application.Interface;
using ApplicationAccountingSystem.Application.Services;
using ApplicationAccountingSystem.Domain.Interfaces;
using ApplicationAccountingSystem.Infrastructure.Data;
using ApplicationAccountingSystem.Infrastructure.Repository;

namespace ApplicationAccountingSystem.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ISLARepository, SLARepository>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<ICommentService, CommentService>();
            
            return services;
        }
    }
}