using E_Commerce_BackendAPI.Authentication;
using E_Commerce_BackendAPI.DAL;
using E_Commerce_BackendAPI.Middleware;
using E_Commerce_BackendAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;         

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;

    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(secretKey!)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthApplicationService, AuthApplicationService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token here"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedAsync(context);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce API V1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();