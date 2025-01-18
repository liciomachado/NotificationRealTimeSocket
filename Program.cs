using Microsoft.AspNetCore.Server.Kestrel.Core;
using NotificationRealTimeSocket;
using NotificationRealTimeSocket.Repositories;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Services.CustomAddCors();
builder.Services.Configure<KestrelServerOptions>(o => o.AllowSynchronousIO = true);
builder.Services.AddSingleton<WebSocketHandler>();
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("10.222.31.10:6379,password=S20Bm9fA2H,abortConnect=False,connectTimeout=10000,syncTimeout=30000")); // Adicione o Redis
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