using Microsoft.OpenApi;
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

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter your JWT token."
                    });

                options.AddSecurityRequirement(document =>
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                    });
            });

            builder.Services.AddDbContext<OmniTradeHubContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("OmniTradeDb")));

            builder.Services.AddScoped<PasswordHasher<User>>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddScoped<IVendorRepository, VendorRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IAdminRepository, AdminRepository>();

            builder.Services.AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var key = builder.Configuration["Jwt:Key"];

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,

                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(key!)),

                            ValidateIssuer = true,
                            ValidIssuer =
                                builder.Configuration["Jwt:Issuer"],

                            ValidateAudience = true,
                            ValidAudience =
                                builder.Configuration["Jwt:Audience"],

                            ValidateLifetime = true,

                            ClockSkew = TimeSpan.Zero
                        };
                });

            var app = builder.Build();

            // Seed Admin account if one does not already exist.

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

            // Configure the HTTP request pipeline.

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Skipped in Development: forcing HTTPS redirection here was
            // silently bouncing local http://localhost:5108 calls back to
            // https://localhost:7286, whose self-signed dev cert isn't
            // trusted by .NET's HttpClient (only by the browser/Swagger),
            // causing every server-to-server call from OmniTradeMvc to
            // fail with a connection error. Re-enable for real deployments,
            // where both sides sit behind a properly trusted certificate.
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}