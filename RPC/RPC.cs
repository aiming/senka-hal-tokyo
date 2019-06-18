using System.Collections.Generic;

namespace WebSocketSample.RPC
{
    [System.Serializable]
    public class Header
    {
        public string Method;
    }

    [System.Serializable]
    public class Position
    {
        public float X;
        public float Y;
        public float Z;

        public Position(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }

    [System.Serializable]
    public class Item
    {
        public int Id;
        public Position Position;

        public Item(int id, Position position)
        {
            this.Id = id;
            this.Position = position;
        }
    }

    [System.Serializable]
    public class PlayerUpdate
    {
        public string Method = "player_update";
        public PlayerUpdatePayload Payload;

        public PlayerUpdate(PlayerUpdatePayload payload)
        {
            this.Payload = payload;
        }
    }

    [System.Serializable]
    public class PlayerUpdatePayload
    {
        public int Id;
        public Position Position;

        public PlayerUpdatePayload(int id, Position position)
        {
            this.Id = id;
            this.Position = position;
        }
    }

    [System.Serializable]
    public class Login
    {
        public string Method = "login";
        public LoginPayload Payload;

        public Login(LoginPayload payload)
        {
            this.Payload = payload;
        }
    }

    [System.Serializable]
    public class LoginPayload
    {
        public string Name;

        public LoginPayload(string name)
        {
            this.Name = name;
        }
    }

    [System.Serializable]
    public class LoginResponse
    {
        public string Method = "login_response";
        public LoginResponsePayload Payload;

        public LoginResponse(LoginResponsePayload payload)
        {
            this.Payload = payload;
        }
    }

    [System.Serializable]
    public class LoginResponsePayload
    {
        public int Id;

        public LoginResponsePayload(int id)
        {
            this.Id = id;
        }
    }

    [System.Serializable]
    public class Sync
    {
        public string Method = "sync";
        public SyncPayload Payload;

        public Sync(SyncPayload payload)
        {
            this.Payload = payload;
        }
    }

    [System.Serializable]
    public class SyncPayload
    {
        public List<Player> Players;

        public SyncPayload(List<Player> players)
        {
            this.Players = players;
        }
    }

    [System.Serializable]
    public class Player
    {
        public int Id;
        public Position Position;
        public int Score;

        public Player(int id, Position position, int score)
        {
            this.Id = id;
            this.Position = position;
            this.Score = score;
        }
    }

    [System.Serializable]
    public class Ping
    {
        public string Method = "ping";
        public PingPayload Payload;

        public Ping(PingPayload payload)
        {
            this.Payload = payload;
        }
    }

    [System.Serializable]
    public class PingPayload
    {
        public string Message;

        public PingPayload(string message)
        {
            this.Message = message;
        }
    }

    [System.Serializable]
    public class Spawn
    {
        public string Method = "spawn";
        public SpawnPayload Payload;

        public Spawn(SpawnPayload payload)
        {
            this.Payload = payload;
        }
    }

    [System.Serializable]
    public class SpawnPayload
    {
        public Item Item;

        public SpawnPayload(Item item)
        {
            this.Item = item;
        }
    }

    [System.Serializable]
    public class GetItem
    {
        public string Method = "get_item";
        public GetItemPayload Payload;

        public GetItem(GetItemPayload payload)
        {
            this.Payload = payload;
        }
    }

    [System.Serializable]
    public class GetItemPayload
    {
        public int ItemId;
        public int PlayerId;

        public GetItemPayload(int itemId, int playerId)
        {
            this.ItemId = itemId;
            this.PlayerId = playerId;
        }
    }

    [System.Serializable]
    public class DeleteItem
    {
        public string Method = "delete_item";
        public DeleteItemPayload Payload;

        public DeleteItem(DeleteItemPayload payload)
        {
            this.Payload = payload;
        }
    }

    [System.Serializable]
    public class DeleteItemPayload
    {
        public int ItemId;

        public DeleteItemPayload(int itemId)
        {
            this.ItemId = itemId;
        }
    }

    [System.Serializable]
    public class Environment
    {
        public string Method = "environment";
        public EnvironmentPayload Payload;

        public Environment(EnvironmentPayload payload)
        {
            this.Payload = payload;
        }
    }

    [System.Serializable]
    public class EnvironmentPayload
    {
        public List<Item> Items;

        public EnvironmentPayload(List<Item> items)
        {
            this.Items = items;
        }
    }

    [System.Serializable]
    public class Collision
    {
        public string Method = "collision";
        public CollisionPayload Payload;

        public Collision(CollisionPayload payload)
        {
            this.Payload = payload;
        }
    }

    [System.Serializable]
    public class CollisionPayload
    {
        public int AlphaId;
        public int BravoId;

        public CollisionPayload(int alphaId, int bravoId)
        {
            this.AlphaId = alphaId;
            this.BravoId = bravoId;
        }
    }

    [System.Serializable]
    public class DeletePlayer
    {
        public string Method = "delete_player";
        public DeletePlayerPayload Payload;

        public DeletePlayer(DeletePlayerPayload payload)
        {
            this.Payload = payload;
        }
    }

    [System.Serializable]
    public class DeletePlayerPayload
    {
        public int Id;

        public DeletePlayerPayload(int id)
        {
            this.Id = id;
        }
    }
}
