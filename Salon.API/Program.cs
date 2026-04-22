using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Salon.API.Middleware;
using Salon.Application.Security;
using Salon.Application.UseCases.Analytics;
using Salon.Application.UseCases.Auth;
using Salon.Application.UseCases.Auth.Users;
using Salon.Application.UseCases.Bookings;
using Salon.Application.UseCases.Commissions;
using Salon.Application.UseCases.Customers;
using Salon.Application.UseCases.Sales;
using Salon.Application.UseCases.Services;
using Salon.Application.UseCases.StaffManagement;
using Salon.Application.UseCases.StaffSchedules;
using Salon.Application.UseCases.Users;
using Salon.Domain.Entities;
using Salon.Domain.Interfaces;
using Salon.Infrastructure.Persistence;
using Salon.Infrastructure.Persistence.Repository;
using Salon.Infrastructure.Repositories;
using Salon.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// 1️⃣ Configure DbContext
// -----------------------------
builder.Services.AddDbContext<SalonDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// -----------------------------
// 2️⃣ Register Repositories
// -----------------------------
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IStaffScheduleRepository, StaffScheduleRepository>();
builder.Services.AddScoped<ICommissionRepository, CommissionRepository>();
builder.Services.AddScoped<ICommissionRuleRepository, CommissionRuleRepository>();
builder.Services.AddScoped<ICommissionStrategyResolver, CommissionStrategyResolver>();

// -----------------------------
// 3️⃣ Register Current User Service (reads email from JWT for audit entries)
// -----------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// -----------------------------
// 4️⃣ Register Handlers / Use Cases
// -----------------------------
builder.Services.AddScoped<CreateBookingHandler>();
builder.Services.AddScoped<CreateServiceHandler>();
builder.Services.AddScoped<CreateStaffHandler>();
builder.Services.AddScoped<GetBookingByIdHandler>();
builder.Services.AddScoped<GetBookingsHandler>();
builder.Services.AddScoped<CreateCustomerHandler>();
builder.Services.AddScoped<GetAllCustomersHandler>();
builder.Services.AddScoped<GetServiceByIdHandler>();
builder.Services.AddScoped<GetServicesHandler>();
builder.Services.AddScoped<GetStaffHandler>();
builder.Services.AddScoped<GetStaffByIdHandler>();
builder.Services.AddScoped<CreateSaleHandler>();
builder.Services.AddScoped<GetSalesHandler>();
builder.Services.AddScoped<RegisterUserHandler>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<UpdateBookingHandler>();
builder.Services.AddScoped<ConfirmBookingHandler>();
builder.Services.AddScoped<CancelBookingHandler>();
builder.Services.AddScoped<DeleteBookingHandler>();
builder.Services.AddScoped<GetAuditLogsHandler>();
builder.Services.AddScoped<UpdateServiceHandler>();
builder.Services.AddScoped<DeleteServiceHandler>();
builder.Services.AddScoped<GetServiceAuditLogsHandler>();
builder.Services.AddScoped<RefundSaleHandler>();
builder.Services.AddScoped<VoidSaleHandler>();
builder.Services.AddScoped<GetSaleByIdHandler>();
builder.Services.AddScoped<GetSalesByBookingHandler>();
builder.Services.AddScoped<GetSaleAuditLogsHandler>();;
builder.Services.AddScoped<UpdateStaffHandler>();
builder.Services.AddScoped<DeleteStaffHandler>();
builder.Services.AddScoped<GetStaffScheduleHandler>();
builder.Services.AddScoped<GetStaffAuditLogsHandler>();
builder.Services.AddScoped<UpdateCustomerHandler>();
builder.Services.AddScoped<UpdateCustomerNotesHandler>();
builder.Services.AddScoped<DeleteCustomerHandler>();
builder.Services.AddScoped<GetCustomerByIdHandler>();
builder.Services.AddScoped<SearchCustomersHandler>();
builder.Services.AddScoped<GetCustomerProfileHandler>();
builder.Services.AddScoped<GetCustomerAuditLogsHandler>();
builder.Services.AddScoped<CompleteBookingHandler>();
builder.Services.AddScoped<GetWeeklyScheduleHandler>();
builder.Services.AddScoped<UpsertStaffScheduleHandler>();
builder.Services.AddScoped<DeleteStaffScheduleHandler>();
builder.Services.AddScoped<CalculateCommissionHandler>();
builder.Services.AddScoped<AdjustCommissionOnRefundHandler>();
builder.Services.AddScoped<MarkCommissionPaidHandler>();
builder.Services.AddScoped<GetStaffCommissionsHandler>();
builder.Services.AddScoped<UpsertCommissionRuleHandler>();
builder.Services.AddScoped<GetCommissionRulesHandler>();

// User management
builder.Services.AddScoped<GetUsersHandler>();
builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<ResetPasswordHandler>();
builder.Services.AddScoped<ChangePasswordHandler>();
builder.Services.AddScoped<UpdateUserStatusHandler>();
builder.Services.AddScoped<DeleteUserHandler>();
builder.Services.AddScoped<GetDashboardAnalyticsHandler>();

// -----------------------------
// 5️⃣ Configure JWT Authentication
// -----------------------------
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// -----------------------------
// 6️⃣ Controllers & Swagger
// -----------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Salon API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// -----------------------------
// 7️⃣ Configure CORS
// -----------------------------
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5174",
                "http://localhost:3000",
                "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// This runs once on startup. If no users exist, it creates the Owner account.
// After the first run it is skipped automatically.

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SalonDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    // Only seed if the Users table is completely empty
    if (!db.Users.Any())
    {
        db.Users.Add(new User(
            email: "owner@salon.com",
            passwordHash: hasher.Hash("ChangeMe123!"),
            role: "Owner"));

        await db.SaveChangesAsync();

        Console.WriteLine("Seed: Owner account created → owner@salon.com / ChangeMe123!");
    }
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SalonDbContext>();
    await SalonSeeder.SeedAsync(context);
}


// -----------------------------
// 8️⃣ Middleware Pipeline
// -----------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Salon API v1"));
}

app.UseHttpsRedirection();

app.UseCors(); 

app.UseMiddleware<ExceptionMiddleware>(); 

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();