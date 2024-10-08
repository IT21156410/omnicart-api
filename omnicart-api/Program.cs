// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : APP configurations
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-8.0
//                    https://medium.com/@amund.fremming/integrating-jwt-to-net-8-925c4f60695e
//                    https://stackoverflow.com/questions/64082588/asp-core-api-custom-unauthorized-body
// ***********************************************************************

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using omnicart_api.Models;
using omnicart_api.Requests;
using omnicart_api.Services;
using System.Text;

var myAllowSpecificOrigins = "AllowReactApp";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173")
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials()
                                .WithExposedHeaders("Access-Control-Allow-Origin")
                                .SetIsOriginAllowed((_) => true)
                                .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
                      });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.Response.OnStarting(async () =>
                {
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                    var response = new AppResponse<object>
                    {
                        Success = false,
                        Message = "Unauthorized",
                        ErrorCode = 401,
                        Error = "Please login first."
                    };

                    var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
                    await context.Response.WriteAsync(jsonResponse);
                });

                return Task.CompletedTask;

            },
            OnForbidden = context =>
            {
                context.Response.OnStarting(async () =>
                {

                    context.Response.ContentType = "application/json";
                    //context.Response.StatusCode = StatusCodes.Status403Forbidden;

                    var response = new AppResponse<object>
                    {
                        Success = false,
                        Message = "Forbidden",
                        ErrorCode = 403,
                        Error = "You do not have permissions to perform this action."
                    };

                    var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
                    await context.Response.WriteAsync(jsonResponse);
                });

                return Task.CompletedTask;

            }
        };
    });

builder.Services.AddScoped<ValidateModelAttribute>();
builder.Services.Configure<ApiBehaviorOptions>(options
    => options.SuppressModelStateInvalidFilter = true);

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<ProductService>();
builder.Services.AddSingleton<CategoryService>();
builder.Services.AddSingleton<OrderService>();
builder.Services.AddSingleton<ReviewService>();
builder.Services.AddSingleton<CustomerService>();
builder.Services.AddSingleton<NotificationService>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "OmniCart API", Version = "v1" });

    // Define the OAuth2.0 scheme that's in use (i.e., Implicit Flow)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(myAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
