var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<WebSocketManager>();
builder.Services.AddSingleton<FinnhubService>();

var app = builder.Build();

app.UseWebSockets();

var webSocketManager = app.Services.GetRequiredService<WebSocketManager>();
var finnhubService = app.Services.GetRequiredService<FinnhubService>();

await finnhubService.ConnectAsync();
await finnhubService.SubscribeToSymbol(["AAPL"]);

app.Map("/ws", async (HttpContext context) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var ws = await context.WebSockets.AcceptWebSocketAsync();
        await webSocketManager.HandleClientAsync(ws);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});


//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

app.Run();
