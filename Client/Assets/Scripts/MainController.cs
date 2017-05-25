﻿using UnityEngine;

using WebSocketSharp;
using RPC = WebSocketSample.RPC;

public class MainController : MonoBehaviour
{
    WebSocket webSocket;    // WebSocketコネクション

    GameObject playerObj;
    int playerId; // プレイヤーID

    [SerializeField]
    GameObject playerPrefab;

    void Start()
    {
        webSocket = new WebSocket("ws://localhost:5678");

        // コネクション確立したときのハンドラ
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
        playerObj = Instantiate(playerPrefab, new Vector3(0.0f, 0.5f, 0.0f), Quaternion.identity) as GameObject;
    }
}