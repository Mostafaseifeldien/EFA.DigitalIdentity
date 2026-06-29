using System.Text;
using EFA.Application.Members.CreateMember;
using EFA.Application.Members.GetMemberById;
using EFA.Application.Members.GetMyMemberProfile;
using EFA.Application.Members.GetMembers;
using EFA.Application.Members.ToggleMemberStatus;
using EFA.Application.Members.UpdateMember;
using EFA.Application.Matches.CreateMatch;
using EFA.Application.Matches.GetMatchById;
using EFA.Application.Matches.GetMatchLookups;
using EFA.Application.Matches.GetMatches;
using EFA.Application.Matches.UpdateMatch;
using EFA.Application.Assignments.BulkCreateAssignments;
using EFA.Application.Assignments.CancelAssignment;
using EFA.Application.Assignments.GetAssignmentById;
using EFA.Application.Assignments.GetAssignmentLookups;
using EFA.Application.Assignments.GetAssignments;
using EFA.Application.Assignments.GetMyAssignments;
using EFA.Application.Assignments.UpdateAssignment;
using EFA.Application.Players.CreatePlayer;
using EFA.Application.Players.GetPlayerById;
using EFA.Application.Players.GetPlayers;
using EFA.Application.Players.UpdatePlayer;
using EFA.Application.Notifications.CreateNotification;
using EFA.Application.Notifications.GetNotificationById;
using EFA.Application.Notifications.GetNotifications;
using EFA.Application.Notifications.GetUnreadNotificationsCount;
using EFA.Application.Common.Interfaces;
using EFA.Api.Hubs;
using EFA.Api.Services;
using EFA.Domain.Identity;
using EFA.Infrastructure;
using EFA.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EFA Digital Identity API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Token"
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
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 8;
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var secretKey = builder.Configuration["Jwt:SecretKey"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey!)),

            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddScoped<CreateMemberHandler>();
builder.Services.AddScoped<GetMembersHandler>();
builder.Services.AddScoped<IValidator<CreateMemberRequest>, CreateMemberRequestValidator>();
builder.Services.AddScoped<GetMemberByIdHandler>();
builder.Services.AddScoped<GetMyMemberProfileHandler>();
builder.Services.AddScoped<UpdateMemberHandler>();
builder.Services.AddScoped<ToggleMemberStatusHandler>();
builder.Services.AddScoped<IValidator<UpdateMemberCommand>, UpdateMemberCommandValidator>();
builder.Services.AddScoped<GetMatchesHandler>();
builder.Services.AddScoped<CreateMatchHandler>();
builder.Services.AddScoped<GetMatchLookupsHandler>();
builder.Services.AddScoped<IValidator<CreateMatchRequest>, CreateMatchRequestValidator>();
builder.Services.AddScoped<GetMatchByIdHandler>();
builder.Services.AddScoped<UpdateMatchHandler>();
builder.Services.AddScoped<IValidator<UpdateMatchCommand>, UpdateMatchCommandValidator>();
builder.Services.AddScoped<GetAssignmentsHandler>();
builder.Services.AddScoped<GetAssignmentLookupsHandler>();
builder.Services.AddScoped<BulkCreateAssignmentsHandler>();
builder.Services.AddScoped<IValidator<BulkCreateAssignmentsRequest>, BulkCreateAssignmentsRequestValidator>();
builder.Services.AddScoped<GetAssignmentByIdHandler>();
builder.Services.AddScoped<UpdateAssignmentHandler>();
builder.Services.AddScoped<IValidator<UpdateAssignmentCommand>, UpdateAssignmentCommandValidator>();
builder.Services.AddScoped<CancelAssignmentHandler>();
builder.Services.AddScoped<GetMyAssignmentsHandler>();
builder.Services.AddScoped<CreatePlayerHandler>();
builder.Services.AddScoped<IValidator<CreatePlayerCommand>, CreatePlayerCommandValidator>();
builder.Services.AddScoped<GetPlayersHandler>();
builder.Services.AddScoped<GetPlayerByIdHandler>();
builder.Services.AddScoped<UpdatePlayerHandler>();
builder.Services.AddScoped<IValidator<UpdatePlayerCommand>, UpdatePlayerCommandValidator>();
builder.Services.AddScoped<CreateNotificationHandler>();
builder.Services.AddScoped<IValidator<CreateNotificationCommand>, CreateNotificationCommandValidator>();
builder.Services.AddScoped<GetNotificationsHandler>();
builder.Services.AddScoped<GetNotificationByIdHandler>();
builder.Services.AddScoped<GetUnreadNotificationsCountHandler>();
builder.Services.AddScoped<INotificationPushService, NotificationPushService>();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.MigrateAsync();

    await DbInitializer.SeedAsync(scope.ServiceProvider);
}

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseCors("AngularClient");

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();