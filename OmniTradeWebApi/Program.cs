using Microsoft.EntityFrameworkCore;
using OmniTradeWebApi.Data;

namespace OmniTradeWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddDbContext<OmniTradeHubContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("OmniTradeDb")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
