using Microsoft.Win32;
using ProjectManagementAPI.Middlewares;
using ProjectManagementAPI.Services;
using ProjectManagementAPI.Settings;
using RabbitMQ.Client;
using StackExchange.Redis;
using ProjectManagementAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// --- MongoDB Settings ---
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

// --- MongoDB Service ---
builder.Services.AddSingleton<MongoDbService>();

//Register your RabbitMQ service here

// --- RabbitMQ ---
builder.Services.AddSingleton(new ConnectionFactory()
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest"
});

// event publisher and consumer using RabbitMQ
builder.Services.AddSingleton<IEventPublisher, EventPublisher>();
builder.Services.AddSingleton<IEventConsumer, EventConsumer>();
builder.Services.AddHostedService<RabbitMqHostedService>();



// SignalR
builder.Services.AddSignalR();




// ====================
// Redis Configuration
// ====================
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetValue<string>("Redis:Connection") ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(configuration);
});


// Register custom Redis service
builder.Services.AddScoped<IRedisService, RedisService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true); // allow any origin for dev
    });
});

// --- Controllers and Swagger ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddCors();

var app = builder.Build();


// --- Swagger ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- Middleware ---
app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<JwtMiddleware>();

app.MapControllers();

app.MapHub<NotificationHub>("/hubs/notifications"); // frontend will connect here
app.Run();
