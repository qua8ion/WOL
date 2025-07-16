using DbData;
using Dto.AppSettings;
using Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace WakeOnLanApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin",
                policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
        });

        builder.Services.Configure<JwtAppSettings>(builder.Configuration.GetSection("Jwt"));
        builder.Services.Configure<RouterSshSettings>(builder.Configuration.GetSection("Ssh"));
        builder.Services.Configure<SystemUserSettings>(builder.Configuration.GetSection("SystemUser"));

        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtAppSettings>();

        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings?.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = jwtSettings?.Key.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true
                };
            });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDbContext<Context>(option => option.UseSqlServer(builder.Configuration["DbConnectionString"]));
        builder.Services.AddHttpContextAccessor();

        Services.Initialize.ServicesInit(builder.Services);

        var app = builder.Build();

        app.UseCors("AllowSpecificOrigin");
        app.UseAuthentication();
        app.UseAuthorization();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
}
