using Microsoft.Extensions.DependencyInjection;
using UKAccounts.Application.Interfaces;

namespace UKAccounts.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
