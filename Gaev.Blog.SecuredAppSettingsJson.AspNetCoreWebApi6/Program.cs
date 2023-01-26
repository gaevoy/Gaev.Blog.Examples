using Gaev.Blog.SecuredAppSettingsJson;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// This will decrypt the configuration
builder.Configuration.Decrypt(keyPath: "CipherKey", cipherPrefix: "CipherText:");

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly IConfiguration _config;

    public TestController(IConfiguration config)
        => _config = config;

    [HttpGet]
    public string TestConfig()
        => _config["DbConnectionString"];
}
