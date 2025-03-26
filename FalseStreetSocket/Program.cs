var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<WebSocketManager>();
builder.Services.AddSingleton<FinnhubService>();

var app = builder.Build();

app.UseWebSockets();

var webSocketManager = app.Services.GetRequiredService<WebSocketManager>();
var finnhubService = app.Services.GetRequiredService<FinnhubService>();

string? finnhubApiKey = Environment.GetEnvironmentVariable("FINNHUB_API_KEY") ?? throw new Exception("API Key missing");
Console.WriteLine($"Using Finnhub API Key: {finnhubApiKey}");

string[] symbols = builder.Configuration.GetSection("Symbols").Get<string[]>() ?? throw new Exception("Symbols missing");

await finnhubService.ConnectAsync(finnhubApiKey);
await finnhubService.SubscribeToSymbol(symbols);

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
