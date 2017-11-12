using UnityEngine;
using WebSocketSharp;
using RPC = WebSocketSample.RPC;

public class MainController : MonoBehaviour
{
    WebSocket webSocket;    // WebSocketコネクション

    [SerializeField]
    string connectAddress;

    [SerializeField]
    GameObject playerPrefab;
    [SerializeField]
    GameObject otherPlayerPrefab;

    GameObject playerObj;
    Vector3 previousPlayerObjPosition; // 前フレームでの位置
    int playerId;

    void Start()
    {
        webSocket = new WebSocket(connectAddress);

        // コネクションを確立したときのハンドラ
        webSocket.OnOpen += (sender, eventArgs) =>
        {
            Debug.Log("WebSocket Opened");
        };

        // エラーが発生したときのハンドラ
        webSocket.OnError += (sender, eventArgs) =>
        {
            Debug.Log("WebSocket Error Message: " + eventArgs.Message);
        };

        // コネクションを閉じたときのハンドラ
        webSocket.OnClose += (sender, eventArgs) =>
        {
            Debug.Log("WebSocket Closed");
        };

        // メッセージを受信したときのハンドラ
        webSocket.OnMessage += (sender, eventArgs) => 
        {
            Debug.Log("WebSocket Message: " + eventArgs.Data);

            var header = JsonUtility.FromJson<RPC.Header>(eventArgs.Data);
            switch (header.Method)
            {
                case "ping":
                    {
                        var pong = JsonUtility.FromJson<RPC.Ping>(eventArgs.Data);
                        Debug.Log(pong.Payload.Message);
                        break;
                    }
                case "login_response":
                    {
                        var loginResponse = JsonUtility.FromJson<RPC.LoginResponse>(eventArgs.Data);
                        MainThreadExecutor.Enqueue(() => OnLoginResponse(loginResponse.Payload));
                        break;
                    }
            }
        };

        webSocket.Connect();

        Login();
    }

    void Update()
    {
        UpdatePosition();
    }

    void OnDestroy()
    {
        webSocket.Close();    
    }

    void Login()
    {
        var jsonMessage = JsonUtility.ToJson(new RPC.Login(new RPC.LoginPayload("PlayerName")));
        Debug.Log(jsonMessage);

        webSocket.Send(jsonMessage);
        Debug.Log(">> Login");
    }

    void OnLoginResponse(RPC.LoginResponsePayload response)
    {
        Debug.Log("<< LoginResponse");
        playerId = response.Id;
        Debug.Log(playerId);
        playerObj = Instantiate(playerPrefab, new Vector3(0.0f, 0.5f, 0.0f), Quaternion.identity) as GameObject;
    }

    void UpdatePosition()
    {
        if (playerObj == null) return;

        var currentPlayerPosition = playerObj.transform.position;
        if (currentPlayerPosition == previousPlayerObjPosition) return;

        Debug.Log(">> Update");

        previousPlayerObjPosition = currentPlayerPosition;

        var rpcPosition = new RPC.Position(currentPlayerPosition.x, currentPlayerPosition.y, currentPlayerPosition.z);
        var jsonMessage = JsonUtility.ToJson(new RPC.PlayerUpdate(new RPC.PlayerUpdatePayload(playerId, rpcPosition)));
        Debug.Log(jsonMessage);
        webSocket.Send(jsonMessage);
    }
}
