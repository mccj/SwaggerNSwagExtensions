using SwaggerExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddNSwagSwagger(new NSwagConfig { ApiGroupNames = new[] { "test_V1" }, Logo = new XLogo(), Title = "tttttt", Description = "tttttttttteeeeeeeeeeeeee", Version = "v1111" });
builder.Services.AddNSwagSwagger(new NSwagConfig { ApiGroupNames = new[] { "test_V2" } });
builder.Services.AddNSwagSwagger(new NSwagConfig { });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseNSwagSwaggerUI();

app.MapControllers();

app.Run();
