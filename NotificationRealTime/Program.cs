using Microsoft.AspNetCore.Server.Kestrel.Core;
using NotificationRealTime;
using NotificationRealTime.Repositories;
using NotificationRealTime.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configurar a porta dinamicamente
//var port = builder.Configuration.GetValue("PORT", 5000); // Porta padrão: 5000
//builder.WebHost.UseUrls($"http://localhost:{port}");

builder.Services.CustomAddCors();

builder.Services.Configure<KestrelServerOptions>(o => o.AllowSynchronousIO = true);
builder.Services.AddSingleton<WebSocketHandler>();
builder.Services.AddSingleton<IPubSubService, PubSubService>();

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379")); // Adicione o Redis
builder.Services.AddSingleton<INotificationsRepository, NotificationsRepository>();
builder.Services.AddSingleton<INotificationStreamManager, NotificationStreamManager>();
builder.Services.AddHostedService<RedisNotificationService>();

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
