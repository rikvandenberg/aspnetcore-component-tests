using System;
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
            services.AddHttpClient<IProductsService, ShopifyProductsClient>(client =>
            {
                client.BaseAddress = new Uri(Configuration["Shopify:BaseUrl"]);
                client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", Configuration["Shopify:BaseUrl"]);
            });
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