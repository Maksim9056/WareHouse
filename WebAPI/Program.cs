
using ClassLibrary.Date;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(options =>


              options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"))
            );

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowBlazorClient", policy =>
                {
                    policy
                        .WithOrigins("https://localhost:7104")   // здесь адрес вашего WASM-клиента
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            builder.Services.AddMemoryCache();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                // Проверяем, есть ли неприемленные миграции
                if (dbContext.Database.GetPendingMigrations().Any())
                {
                    dbContext.Database.Migrate();
                }
            }
            app.UseHttpsRedirection();
            app.UseCors("AllowBlazorClient");

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
