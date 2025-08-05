using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorApp.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.Services.AddScoped(sp =>
             new HttpClient { BaseAddress = new Uri(builder.Configuration["Urls:Api"]) }
            );

            await builder.Build().RunAsync();
        }
    }
}
