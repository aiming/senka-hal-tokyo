using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using WebSocketSharp;
using RPC = WebSocketSample.RPC;

public class MainController : MonoBehaviour
{
    WebSocket webSocket;    // WebSocketコネクション

    GameObject playerObj;
    Vector3 previousPlayerObjPosition;   // 前フレームでの位置
    int playerId; // プレイヤーID
    Dictionary<int, GameObject> otherPlayerObjs = new Dictionary<int, GameObject>();   // 他プレイヤー
    Dictionary<int, GameObject> items = new Dictionary<int, GameObject>();

    [SerializeField]
    GameObject playerPrefab;
    [SerializeField]
    GameObject otherPlayerPrefab;
    [SerializeField]
    GameObject itemPrefab;
    [SerializeField]
    GameObject itemPoisonPrefab;


    [SerializeField]
    private string connectAddress;

    void Start()
    {
        webSocket = new WebSocket(connectAddress);

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
        playerObj = Instantiate(playerPrefab, new Vector3(0.0f, 0.5f, 0.0f), Quaternion.identity) as GameObject;
        var playerController = playerObj.GetComponent<PlayerController>();
        playerController.OnCollision += otherPlayerId =>
        {
            var collisionRpc = new RPC.Collision(new RPC.CollisionPayload(playerId, otherPlayerId));
            var collisionJson = JsonUtility.ToJson(collisionRpc);
            webSocket.Send(collisionJson);
        };
    }

    void UpdatePosition()
    {
        if (playerObj == null) return;
        if (playerObj.transform.position == previousPlayerObjPosition) return;

        Debug.Log(">> Update");

        var currentPlayerPosition = playerObj.transform.position;
        previousPlayerObjPosition = currentPlayerPosition;

        var rpcPosition = new RPC.Position(currentPlayerPosition.x, currentPlayerPosition.y, currentPlayerPosition.z);
        var jsonMessage = JsonUtility.ToJson(new RPC.PlayerUpdate(new RPC.PlayerUpdatePayload(playerId, rpcPosition)));
        Debug.Log(jsonMessage);

        webSocket.Send(jsonMessage);
    }

    void OnSync(RPC.SyncPayload payload)
    {
        Debug.Log("<< Sync");

        foreach (var player in payload.Players)
        {
            // 自分の座標は要らない
            if (player.Id == playerId)
            {
                playerObj.transform.localScale = CalcPlayerScale(player.Score);
                continue;
            }

            var playerPosition = new Vector3(player.Position.X, player.Position.Y, player.Position.Z);
            if (otherPlayerObjs.ContainsKey(player.Id))
            {
                // 既にGameObjectが居たら位置更新
                otherPlayerObjs[player.Id].transform.position = playerPosition;
                otherPlayerObjs[player.Id].transform.localScale = CalcPlayerScale(player.Score);
            }
            else
            {
                // GameObjectが居なかったら新規作成
                var otherPlayerObj = Instantiate(otherPlayerPrefab, playerPosition, Quaternion.identity) as GameObject;
                otherPlayerObj.GetComponent<OtherPlayerController>().Id = player.Id;
                otherPlayerObj.name = "Other" + player.Id;
                otherPlayerObjs.Add(player.Id, otherPlayerObj);
                Debug.Log("Instantiated a new player: " + player.Id);
            }
        }
    }

    Vector3 CalcPlayerScale(int score)
    {
        return Vector3.one + (Vector3.one * score * 0.2f);
    }

    void OnSpawn(RPC.SpawnPayload response)
    {
        Debug.Log("<< OnSpawn");

        SpawnItem(response.Item);
    }

    void SpawnItem(RPC.Item itemRpc)
    {
        int rand = System.Random.Range(0, 10);
        if (rand == 9)
        {
           var itemObj = Instantiate(
              itemPoisonPrefab,
              new Vector3(itemRpc.Position.X, itemRpc.Position.Y, itemRpc.Position.Z),
              Quaternion.identity
          );
        }
        else
        {
           var itemObj = Instantiate(
               itemPrefab,
               new Vector3(itemRpc.Position.X, itemRpc.Position.Y, itemRpc.Position.Z),
               Quaternion.identity
           );
        }
       
        
        items.Add(itemRpc.Id, itemObj);

        var item = itemObj.GetComponent<ItemController>();
        item.ItemId = itemRpc.Id;
        item.OnGot += () =>
        {
            
            items.Remove(item.ItemId);
            Destroy(itemObj);

            var getItemRpc = new RPC.GetItem(new RPC.GetItemPayload(item.ItemId, playerId));
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

        var serverUnknowItems = new List<KeyValuePair<int, GameObject>>();
        foreach (var item in items)
        {
            if (payload.Items.Exists(itemRpc => itemRpc.Id == item.Key)) continue;

            serverUnknowItems.Add(item);
        }
        foreach (var item in serverUnknowItems)
        {
            items.Remove(item.Key);
            Destroy(item.Value);
        }

        foreach (var itemRpc in payload.Items)
        {
            if (items.ContainsKey(itemRpc.Id)) continue;

            SpawnItem(itemRpc);
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

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
