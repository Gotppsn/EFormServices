// EFormServices.Web/Extensions/ServiceCollectionExtensions.cs
// Got code 30/05/2025
using EFormServices.Web.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EFormServices.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("ManageUsers", policy => policy.Requirements.Add(new PermissionRequirement("manage_users")));
            options.AddPolicy("CreateForms", policy => policy.Requirements.Add(new PermissionRequirement("create_forms")));
            options.AddPolicy("EditForms", policy => policy.Requirements.Add(new PermissionRequirement("edit_forms")));
            options.AddPolicy("DeleteForms", policy => policy.Requirements.Add(new PermissionRequirement("delete_forms")));
            options.AddPolicy("ViewReports", policy => policy.Requirements.Add(new PermissionRequirement("view_reports")));
        });

        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        services.AddHttpContextAccessor();

        return services;
    }
}