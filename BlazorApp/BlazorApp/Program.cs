using BlazorApp.Client.Pages;
using BlazorApp.Components;
using ClassLibrary.Date;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlazorApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveWebAssemblyComponents();
            //builder.Services.AddHttpClient("Api", client =>
            //{
            //    client.BaseAddress = new Uri("https://localhost:7166/api/");
            //}
            //);
            builder.Services.AddScoped<HttpClient>(provider =>
            {
                string url = builder.Configuration["Urls:Api"];
                HttpClient httpClient = new HttpClient();

                return new HttpClient() { BaseAddress = new Uri(url) };
            });

           // builder.Services.AddScoped(sp =>
           // new HttpClient { BaseAddress = new Uri(builder.Configuration["Urls:Api"]) }
           //);


            //builder.Services.AddHttpClient();
            var app = builder.Build();
          
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

       
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            app.Run();
        }
    }
}
