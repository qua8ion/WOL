using DbData;
using Dto.AppSettings;
using Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace WakeOnLanApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //WOLLib.PC pc; //чтобы дебаг работал

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    policy =>
                    {
                        //policy.WithOrigins("http://localhost:5173")
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            builder.Services.Configure<JwtAppSettings>(builder.Configuration.GetSection("Jwt"));
            builder.Services.Configure<MikroTikAppSettings>(builder.Configuration.GetSection("MikroTik"));
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

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<Context>(option => option.UseSqlServer(builder.Configuration["DbConnectionString"]));
            builder.Services.AddHttpContextAccessor();
            
            Services.Initialize.ServicesInit(builder.Services);

            var app = builder.Build();

            app.UseCors("AllowSpecificOrigin");
            app.UseAuthentication();
            app.UseAuthorization();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}