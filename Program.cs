using RealTimeDriverTracking;
using RealTimeDriverTracking.Repositories;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Services.CustomAddCors();
builder.Services.AddSingleton<WebSocketHandler>();
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379")); // Adicione o Redis
builder.Services.AddSingleton<INotificationsMongoRepository, NotificationsMongoRepository>(); // Adicione o MongoDB repository

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAllOrigins");
app.CustomUseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseWebSockets();
app.Run();