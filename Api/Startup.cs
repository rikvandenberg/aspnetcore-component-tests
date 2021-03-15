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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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

            services.AddHttpClient<IProductsService, ShopifyProductsClient>("Shopify", client =>
            {
                client.BaseAddress = new Uri(Configuration["Shopify:BaseUrl"]);
                client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", Configuration["Shopify:BaseUrl"]);
            }).AddPolicyHandlerFromRegistry("defaultJsonResponse");
        }

        private void ConfigureDataLayer(IServiceCollection services)
        {
            services.AddDbContext<OrderDbContext>(
                options => options.UseSqlite("Data Source=orders.db"));
            services.AddScoped(typeof(IRepository<>), typeof(EntityFrameworkRepository<>));
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