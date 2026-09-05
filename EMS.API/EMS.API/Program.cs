using EMS.Application.Interfaces;
using EMS.Application.Services;
using EMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;

// Enable Npgsql legacy timestamp behavior so DateTime values work seamlessly across PostgreSQL timestamp/timestamptz columns
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// ---- Add services to the container ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "EMS.API - Employee Management System",
        Version = "v1",
        Description = "Enterprise Employee Management System REST API"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Paste ONLY your raw token here (no need to type 'Bearer')"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database
builder.Services.AddDbContext<EmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    }));

// Dependency Injection: register our services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IReportService, ReportService>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuperSecretEmployeeManagementSystemJwtKey2026!MustBeAtLeast512BitsLongForSecurity#9876543210";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "EMS.API",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "EMS.Client",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// CORS - allow Angular frontend and API consumers from any host
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Forwarded headers for reverse proxies (Render, Docker, Nginx)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline
// Expose Swagger UI in both Development and Production for API exploration and verification
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EMS API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");

app.UseAuthentication();   // WHO are you?
app.UseAuthorization();    // WHAT are you allowed to do?

// Root redirect to Swagger and health check endpoint
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "EMS.API", timestamp = DateTime.UtcNow }));

app.MapControllers();

// Auto-migrate and seed initial admin, departments, and employees
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<EmsDbContext>();
        logger.LogInformation("Verifying and applying database migrations...");
        await context.Database.MigrateAsync();

        // 1. Seed Admin user if not exists
        if (!await context.Users.AnyAsync(u => u.Username == "admin"))
        {
            logger.LogInformation("Seeding default administrator account (admin / Admin@123)...");
            context.Users.Add(new EMS.Domain.Entities.User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin"
            });
            await context.SaveChangesAsync();
        }

        // 2. Seed Departments if empty
        if (!await context.Departments.AnyAsync())
        {
            logger.LogInformation("Seeding default departments...");
            var defaultDepts = new List<EMS.Domain.Entities.Department>
            {
                new() { Name = "Information Technology" },
                new() { Name = "Human Resources" },
                new() { Name = "Finance & Accounts" },
                new() { Name = "Operations & Logistics" },
                new() { Name = "Marketing & Sales" }
            };
            context.Departments.AddRange(defaultDepts);
            await context.SaveChangesAsync();
        }

        // 3. Seed Starter Employees if empty
        if (!await context.Employees.AnyAsync())
        {
            logger.LogInformation("Seeding initial employees...");
            var itDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Information Technology");
            var hrDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Human Resources");
            var finDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Finance & Accounts");

            var starterEmployees = new List<EMS.Domain.Entities.Employee>
            {
                new()
                {
                    FullName = "Karan Sahoo",
                    Email = "karan.sahoo@ems.corp",
                    Phone = "+91 9876543210",
                    DepartmentId = itDept?.DepartmentId,
                    Designation = "Senior Lead Architect",
                    Salary = 125000,
                    JoiningDate = DateTime.SpecifyKind(DateTime.UtcNow.AddYears(-2), DateTimeKind.Utc),
                    IsActive = true
                },
                new()
                {
                    FullName = "Priya Sharma",
                    Email = "priya.sharma@ems.corp",
                    Phone = "+91 9876543211",
                    DepartmentId = hrDept?.DepartmentId,
                    Designation = "HR Business Partner",
                    Salary = 75000,
                    JoiningDate = DateTime.SpecifyKind(DateTime.UtcNow.AddYears(-1), DateTimeKind.Utc),
                    IsActive = true
                },
                new()
                {
                    FullName = "Rahul Verma",
                    Email = "rahul.verma@ems.corp",
                    Phone = "+91 9876543212",
                    DepartmentId = finDept?.DepartmentId,
                    Designation = "Financial Analyst",
                    Salary = 68000,
                    JoiningDate = DateTime.SpecifyKind(DateTime.UtcNow.AddMonths(-8), DateTimeKind.Utc),
                    IsActive = true
                },
                new()
                {
                    FullName = "Ananya Patel",
                    Email = "ananya.patel@ems.corp",
                    Phone = "+91 9876543213",
                    DepartmentId = itDept?.DepartmentId,
                    Designation = "Frontend Engineer",
                    Salary = 85000,
                    JoiningDate = DateTime.SpecifyKind(DateTime.UtcNow.AddMonths(-5), DateTimeKind.Utc),
                    IsActive = true
                }
            };
            context.Employees.AddRange(starterEmployees);
            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred during startup database migration/seeding.");
    }
}

app.Run();
