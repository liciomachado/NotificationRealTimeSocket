using Microsoft.AspNetCore.Server.Kestrel.Core;
using NotificationRealTimeSocket;
using NotificationRealTimeSocket.Repositories;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
builder.Services.CustomAddCors();
builder.Services.Configure<KestrelServerOptions>(o => o.AllowSynchronousIO = true);
builder.Services.AddSingleton<WebSocketHandler>();
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379")); // Adicione o Redis
builder.Services.AddSingleton<INotificationsRepository, NotificationsRepository>();

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
