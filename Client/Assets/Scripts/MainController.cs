using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SerializeField]
    GameObject itemPrefab;

    GameObject playerObj;
    Vector3 previousPlayerObjPosition; // 前フレームでの位置
    int playerId;
    Dictionary<int, GameObject> otherPlayerObjs = new Dictionary<int, GameObject>();
    Dictionary<int, GameObject> items = new Dictionary<int, GameObject>();

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
                case "sync":
                    {
                        var syncMessage = JsonUtility.FromJson<RPC.Sync>(eventArgs.Data);
                        MainThreadExecutor.Enqueue(() => OnSync(syncMessage.Payload));
                        break;
                    }
                case "spawn":
                    {
                        var spawnResponse = JsonUtility.FromJson<RPC.Spawn>(eventArgs.Data);
                        MainThreadExecutor.Enqueue(() => OnSpawn(spawnResponse.Payload));
                        break;
                    }
                case "delete_item":
                    {
                        var deleteMessage = JsonUtility.FromJson<RPC.DeleteItem>(eventArgs.Data);
                        MainThreadExecutor.Enqueue(() => OnDeleteItem(deleteMessage.Payload));
                        break;
                    }
                case "environment":
                    {
                        var environmentMessage = JsonUtility.FromJson<RPC.Environment>(eventArgs.Data);
                        MainThreadExecutor.Enqueue(() => OnEnvironment(environmentMessage.Payload));
                        break;
                    }
                case "delete_player":
                    {
                        var deletePlayerMessage = JsonUtility.FromJson<RPC.DeletePlayer>(eventArgs.Data);
                        MainThreadExecutor.Enqueue(() => OnDeletePlayer(deletePlayerMessage.Payload));
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

        var playerController = playerObj.GetComponent<PlayerController>();
        playerController.OnCollision += (otherPlayerId) =>
        {
            var collisionRpc = new RPC.Collision(new RPC.CollisionPayload(playerId, otherPlayerId));
            var collisionJson = JsonUtility.ToJson(collisionRpc);
            webSocket.Send(collisionJson);
        };
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

    void OnSync(RPC.SyncPayload payload)
    {
        Debug.Log("<< Sync");
        foreach (var rpcPlayer in payload.Players)
        {
            if (rpcPlayer.Id == playerId)
            {
                playerObj.transform.localScale = CalcPlayerScale(rpcPlayer.Score);
                continue;
            }

            var otherPlayerPoision = new Vector3(rpcPlayer.Position.X, rpcPlayer.Position.Y, rpcPlayer.Position.Z);

            if (otherPlayerObjs.ContainsKey(rpcPlayer.Id))
            {
                // 既にGameObjectがいたら更新
                otherPlayerObjs[rpcPlayer.Id].transform.position = otherPlayerPoision;
                otherPlayerObjs[rpcPlayer.Id].transform.localScale = CalcPlayerScale(rpcPlayer.Score);
            }
            else
            {
                // GameObjectがいなかったら新規作成
                var otherPlayerObj = Instantiate(otherPlayerPrefab, otherPlayerPoision, Quaternion.identity) as GameObject;
                otherPlayerObj.GetComponent<OtherPlayerController>().Id = rpcPlayer.Id;
                otherPlayerObj.name = "Other" + rpcPlayer.Id;
                otherPlayerObjs.Add(rpcPlayer.Id, otherPlayerObj);
                Debug.Log("Instantiated a new player: " + rpcPlayer.Id);
            }
        }
    }

    void OnSpawn(RPC.SpawnPayload payload)
    {
        Debug.Log("<< OnSpawn");
        SpawnItem(payload.Item);
    }

    void SpawnItem(RPC.Item rpcItem)
    {
        var position = new Vector3(rpcItem.Position.X, rpcItem.Position.Y, rpcItem.Position.Z);
        var itemObj = Instantiate(itemPrefab, position, Quaternion.identity);
        items.Add(rpcItem.Id, itemObj);

        var item = itemObj.GetComponent<ItemController>();
        item.OnGet += () =>
        {
            items.Remove(rpcItem.Id);
            Destroy(itemObj);

            var getItemRpc = new RPC.GetItem(new RPC.GetItemPayload(rpcItem.Id, playerId));
            var getItemJson = JsonUtility.ToJson(getItemRpc);
            webSocket.Send(getItemJson);
            Debug.Log(">> GetItem");
        };
    }

    void OnDeleteItem(RPC.DeleteItemPayload payload)
    {
        Debug.Log("<< DeleteItem");
        var itemId = payload.ItemId;
        if (items.ContainsKey(itemId))
        {
            Destroy(items[itemId]);
            items.Remove(itemId);
        }
    }

    void OnEnvironment(RPC.EnvironmentPayload payload)
    {
        Debug.Log("<< Environment");

        var serverUnknownItems = new List<KeyValuePair<int, GameObject>>();
        // サーバーからのリスト(payload.Items)にないアイテムを所持していたらserverUnknownItemsに追加
        foreach (var item in items)
        {
            if (payload.Items.Exists(itemRpc => itemRpc.Id == item.Key)) continue;

            serverUnknownItems.Add(item);
        }
        // serverUnknownItemsをクライアントから削除
        foreach (var item in serverUnknownItems)
        {
            items.Remove(item.Key);
            Destroy(item.Value);
        }

        foreach (var rpcItem in payload.Items)
        {
            if (items.ContainsKey(rpcItem.Id)) continue;

            SpawnItem(rpcItem);
        }
    }

    void OnDeletePlayer(RPC.DeletePlayerPayload payload)
    {
        if (otherPlayerObjs.ContainsKey(payload.Id))
        {
            Destroy(otherPlayerObjs[payload.Id]);
            otherPlayerObjs.Remove(payload.Id);
        }
        else if (payload.Id == playerId)
        {
            Destroy(playerObj);
            Invoke("RestartGame", 3);
        }
    }

    Vector3 CalcPlayerScale(int score)
    {
        return Vector3.one + (Vector3.one * score * 0.2f);
    }

    void RestartGame()
    {
        webSocket.Close();
        MainThreadExecutor.Clear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
