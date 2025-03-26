using System.Net.WebSockets;
using System.Text;
using System.Collections.Concurrent;

public class WebSocketManager
{
    private readonly ConcurrentBag<WebSocket> _clients = new();

    public void AddClient(WebSocket socket)
    {
        _clients.Add(socket);
    }

    public async Task BroadcastMessageAsync(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var tasks = _clients
            .Where(client => client.State == WebSocketState.Open)
            .Select(client => client.SendAsync(
                new ArraySegment<byte>(messageBytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None));

        await Task.WhenAll(tasks);
    }

    public async Task HandleClientAsync(WebSocket socket)
    {
        AddClient(socket);
        var buffer = new byte[1024 * 4];

        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                _clients.TryTake(out _);
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
        }
    }
}
