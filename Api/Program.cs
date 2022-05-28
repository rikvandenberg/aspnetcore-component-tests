using Api.DataLayer;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api", Version = "v1" });
});
builder.Services.ConfigurePolly();
builder.Services.ConfigureApiLayer();
builder.Services.ConfigureBusinessLayer(builder.Configuration);
builder.Services.ConfigureDataLayer();

WebApplication app = builder.Build();
using IServiceScope scope = app.Services.CreateScope();
OrderDbContext context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    context.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.Run();