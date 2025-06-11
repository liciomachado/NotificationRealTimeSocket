using NotificationRealTime.Net10;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapOpenApi();
app.UseHttpsRedirection();


app.MapGet("/live", (CancellationToken ct) =>
{
    return TypedResults.ServerSentEvents(StreamData(ct));

    async IAsyncEnumerable<SseItem<User>> StreamData([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var userDao = new UserDao();
        await foreach (var user in userDao.GetUsersAsync(ct).WithCancellation(cancellationToken))
        {
            await Task.Delay(1000, cancellationToken);

            yield return new SseItem<User>(user, "list-itens")
            {
                ReconnectionInterval = TimeSpan.FromSeconds(60),
                EventId = user.Id
            };
        }
    }
})
.WithName("SSE");

app.Run();