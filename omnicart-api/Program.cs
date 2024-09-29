// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : APP configurations
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-8.0
// ***********************************************************************

using omnicart_api.Models;
using omnicart_api.Services;

var MyAllowSpecificOrigins = "AllowReactApp";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                         policy.WithOrigins("http://localhost:5173")  
                               .AllowAnyHeader()                      
                               .AllowAnyMethod()                      
                               .AllowCredentials()                    
                               .WithExposedHeaders("Access-Control-Allow-Origin")  
                               .SetIsOriginAllowed((origin) => true)  
                               .SetPreflightMaxAge(TimeSpan.FromMinutes(10)); 
                      });
});

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<UserService>();

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

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
