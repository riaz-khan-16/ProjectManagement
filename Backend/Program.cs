using ProjectManagementAPI.Middlewares;
using ProjectManagementAPI.Services;
using ProjectManagementAPI.Settings;

var builder = WebApplication.CreateBuilder(args);

// --- MongoDB Settings ---
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

// --- MongoDB Service ---
builder.Services.AddSingleton<MongoDbService>();

// --- Controllers and Swagger ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

var app = builder.Build();

// --- Swagger ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- Middleware ---
app.UseHttpsRedirection();
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseMiddleware<JwtMiddleware>();

app.MapControllers();
app.Run();
