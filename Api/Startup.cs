using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Api.BusinessLayer;
using Api.DataLayer;
using Api.Shopify;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;

namespace Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api", Version = "v1" });
            });

            ConfigureBusinessLayer(services);
            ConfigureDataLayer(services);
        }

        private void ConfigureBusinessLayer(IServiceCollection services)
        {
            PolicyRegistry registry = new PolicyRegistry();
            registry.Add("defaultJsonResponse", HttpPolicyExtensions.HandleTransientHttpError()
                .OrResult(response => response.StatusCode == HttpStatusCode.Forbidden)
                .FallbackAsync(_ => Task.FromResult(new HttpResponseMessage { Content = new StringContent("{}", Encoding.UTF8) })));
            services.AddPolicyRegistry(registry);

            services.AddHttpClient<IProductsService, ShopifyProductsClient>(client =>
            {
                client.BaseAddress = new Uri(_configuration["Shopify:BaseUrl"]!);
                client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", _configuration["Shopify:ApiKey"]);
            }).AddPolicyHandlerFromRegistry("defaultJsonResponse");
        }

        private void ConfigureDataLayer(IServiceCollection services)
        {
            services.AddDbContext<OrderDbContext>(
                optionsBuilder => ConfigureDbContextOptions(optionsBuilder.UseSqlite("Data Source=orders.db")),
                optionsLifetime: ServiceLifetime.Singleton);
            services.AddScoped(typeof(IRepository<>), typeof(EntityFrameworkRepository<>));
        }

        public static void ConfigureDbContextOptions(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, OrderDbContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1"));
                context.Database.EnsureCreated();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}