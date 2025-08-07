using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class WebSocketClient
{
    public string url = "localhost:8080";
    public string userId = Guid.NewGuid().ToString();

    private static WebSocketClient _instance;

    public static WebSocketClient Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new WebSocketClient();
            }
            return _instance;
        }
    }

    private ClientWebSocket _webSocket;
    private CancellationTokenSource _cancellationTokenSource;
    private Uri _serverUri;

    public event Action<string> OnMessageReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnError;

    public WebSocketClient()
    {
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task ConnectAsync()
    {
        _serverUri = new Uri("ws://" + url + "/api/ws/" + userId);
        Debug.Log("Connecting to: " + _serverUri);
        try
        {
            _webSocket = new ClientWebSocket();
            await _webSocket.ConnectAsync(_serverUri, _cancellationTokenSource.Token);

            Debug.Log("await finished");

            OnConnected?.Invoke();
            StartReceiving();
            Debug.Log("WebSocket connected: " + _serverUri);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Connection failed: {ex.Message}");
            OnError?.Invoke($"Connection failed: {ex.Message}");
        }
    }

    private async void StartReceiving()
    {
        Debug.Log("Start receiving messages");
        var buffer = new byte[4096];
        var segment = new ArraySegment<byte>(buffer);

        try
        {
            while (_webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(segment, _cancellationTokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await CloseAsync();
                }
                else
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    OnMessageReceived?.Invoke(message);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Receive error: {ex.Message}");
            OnError?.Invoke($"Receive error: {ex.Message}");
            await CloseAsync();
        }
        Debug.Log("Receive finished");
    }

    // 发送 Message<T> 对象
    public async Task SendMessageAsync<T>(Message<T> message)
    {
        try
        {
            string json = JsonUtility.ToJson(message);
            await SendAsync(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to serialize and send message: {ex.Message}");
            OnError?.Invoke($"Failed to serialize and send message: {ex.Message}");
        }
    }

    // 原始发送方法保持不变
    public async Task SendAsync(string message)
    {
        Debug.Log("Sending message: " + message);
        if (_webSocket?.State != WebSocketState.Open)
        {
            OnError?.Invoke("WebSocket is not connected");
            return;
        }

        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(bytes);

            await _webSocket.SendAsync(
                segment,
                WebSocketMessageType.Text,
                true,
                _cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Send failed: {ex.Message}");
            OnError?.Invoke($"Send failed: {ex.Message}");
        }
        Debug.Log("Sending finished");
    }

    public async Task CloseAsync()
    {
        Debug.Log("Closing WebSocket");
        if (_webSocket?.State == WebSocketState.Open)
        {
            try
            {
                await _webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Client closing",
                    _cancellationTokenSource.Token);
            }
            catch
            {
                // 忽略关闭过程中的异常
            }
        }

        _webSocket?.Dispose();
        _webSocket = null;
        OnDisconnected?.Invoke();
        Debug.Log("WebSocket closed");
    }
}