using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OmniTradeWebApi.Data;
using OmniTradeWebApi.Models;
using OmniTradeWebApi.Repositories;
using OmniTradeWebApi.Services;
using System.Text;

namespace OmniTradeWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<OmniTradeHubContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("OmniTradeDb")));

            builder.Services.AddScoped<IProductRepository, ProductRepository>();

            builder.Services.AddScoped<PasswordHasher<User>>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var key = builder.Configuration["Jwt:Key"];

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(key!)),

                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],

                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["Jwt:Audience"],

                        ValidateLifetime = true,

                        ClockSkew = TimeSpan.Zero
                    };
                });



            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider
                    .GetRequiredService<OmniTradeHubContext>();

                var passwordHasher = scope.ServiceProvider
                    .GetRequiredService<PasswordHasher<User>>();

                var adminExists = context.Users
                    .Any(u => u.Role == "Admin");

                if (!adminExists)
                {
                    var admin = new User
                    {
                        Username = "admin",
                        Email = "admin@omnitradehub.com",
                        Role = "Admin"
                    };

                    admin.PasswordHash = passwordHasher.HashPassword(
                        admin,
                        "Admin@123");

                    context.Users.Add(admin);

                    context.SaveChanges();
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();

                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "OmniTrade Web API v1");
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
