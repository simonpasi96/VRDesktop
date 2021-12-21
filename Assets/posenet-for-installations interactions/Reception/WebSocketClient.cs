using UnityEngine;
using WebSocketSharp;

public class WebSocketClient : MonoBehaviour
{
    [SerializeField]
    string url = "ws://localhost:8080";
    WebSocket ws;

    public delegate void MessageEvent(string message);
    public MessageEvent MessageReceived;


    private void Start()
    {
        ws = new WebSocket(url);
        ws.OnMessage += (sender, e) => ShareData(e.Data);
        ws.Connect();
    }

    private void OnDestroy()
    {
        if (ws != null)
            ws.Close();
    }


    void ShareData(string data)
    {
        MessageReceived?.Invoke(data);
    }
}
