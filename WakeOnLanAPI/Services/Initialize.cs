using DbData.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.Auth;
using Services.BusinessLogic.NetworkManager;
using Services.Crud;
using Services.Crud.Abstracts;
using Services.Crud.Base;
using Services.DbTransactions;
using Services.DbTransactions.Abstracts;
using Services.User.Provider;
using Services.User.SystemScopeAccessor;

namespace Services;

public static class Initialize
{
    public static void ServicesInit(IServiceCollection services)
    {
        services.AddScoped<ISystemUserScopeAccessor, SystemUserScopeAccessor>();
        services.AddScoped<ITransactionScopeFactory, TransactionScopeFactory>();
        services.AddScoped(typeof(ICrudService<>), typeof(CrudService<>));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserProvider, UserProvider>();

        services.AddScoped<ICrudService<Device>, DeviceCrudService>();

        services.AddScoped<INetworkManagerService, NetworkManagerService>();
    }
}