using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using VNGTest.Filters;
using VNGTest.Jobs;
using VNGTest.Models;
using VNGTest.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<XAuthHeaderFilter>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddTransient<PasswordUpdateJob>();

builder.Services.AddHangfire(configuration =>
{
    configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseMemoryStorage();
});

builder.Services.AddHangfireServer();

var app = builder.Build();

app.UseHangfireDashboard();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

RecurringJob.AddOrUpdate<PasswordUpdateJob>(
    recurringJobId: "PasswordUpdateJob",
    job => job.ExecuteAsync(),
    Cron.Daily,
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    });

app.Run();
