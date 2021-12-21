using System;
using UnityEngine;

/// <summary>
/// Gets the poses from a websocket and stores it into Result.
/// </summary>
[RequireComponent(typeof(WebSocketClient))]
public class PosenetMessageHandler : MonoBehaviour
{
    WebSocketClient websocket;
    public delegate void EventHandler(PoseNetResult result);
    public event EventHandler MessageReceived;
    bool hasMessage;

    public PoseNetResult Result { get; private set; }


    private void Awake()
    {
        websocket = GetComponent<WebSocketClient>();
        // Get a result without keypoints and poses of low confidence.
        websocket.MessageReceived += delegate (string message)
        {
            Result = JsonUtility.FromJson<PoseNetResult>(message);
            hasMessage = true;
        };
    }

    private void Update()
    {
        // Call our message event (from the update to avoid issues).
        if (hasMessage)
        {
            MessageReceived?.Invoke(Result);
            hasMessage = false;
        }
    }
}


#region Posenet classes -----------
[Serializable]
public class PoseNetKeypoint
{
    public float score;
    public string part;
    public Vector2 position;
}

/// <summary>
/// Raw pose from the websocket.
/// </summary>
[Serializable]
public class PoseNetPose
{
    public float score;
    public PoseNetKeypoint[] keypoints;
}

[Serializable]
public class PoseNetImage
{
    public int width;
    public int height;
}

[Serializable]
public class PoseNetResult
{
    public PoseNetPose[] poses;
    public PoseNetImage image = new PoseNetImage();
}
#endregion ----------------------