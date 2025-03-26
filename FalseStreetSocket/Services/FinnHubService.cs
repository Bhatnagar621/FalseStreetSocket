using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public class FinnhubService
{
    private readonly WebSocketManager _webSocketManager;
    private readonly ClientWebSocket _finnhubSocket = new();

    public FinnhubService(WebSocketManager webSocketManager)
    {
        _webSocketManager = webSocketManager;
    }

    public async Task ConnectAsync(string APIKEY)
    {
        try
        {
            var finnhubUrl = $"wss://ws.finnhub.io?token={APIKEY}";

            await _finnhubSocket.ConnectAsync(new Uri(finnhubUrl), CancellationToken.None);
            Console.WriteLine("Connected to Finnhub WebSocket");

            _ = Task.Run(async () =>
            {
                var buffer = new byte[4096];

                while (_finnhubSocket.State == WebSocketState.Open)
                {
                    var result = await _finnhubSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    Console.WriteLine($"Received: {message}");

                    // Send the message to all WebSocket clients
                    await _webSocketManager.BroadcastMessageAsync(message);
                }
            });
        }
        catch (Exception ex) { 
            Console.WriteLine($"Error: {ex.Message}");
        }

    }

    public async Task SubscribeToSymbol(string[] symbols)
    {
        foreach(var symbol in symbols)
        {
            var subscriptionMessage = JsonSerializer.Serialize(new { type = "subscribe", symbol });
            var messageBytes = Encoding.UTF8.GetBytes(subscriptionMessage);

            await _finnhubSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"Subscribed to {symbol}");
        }

    }
}
