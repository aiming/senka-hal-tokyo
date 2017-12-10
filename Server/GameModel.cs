using System;
using System.Collections.Generic;
using System.Timers;
using Newtonsoft.Json;
using WebSocketSample.RPC;

namespace WebSocketSample.Server
{
    public class GameModel
    {
        Dictionary<int, Player> players = new Dictionary<int, Player>();
        int uidCounter;

        public event Action<string, string> sendTo;
        public event Action<string> broadcast;

        public GameModel()
        {
            StartSpawnTimer();
        }

        public void OnUpdate()
        {
            Sync();
        }

        public void OnPing(string senderId)
        {
            Console.WriteLine(">> Ping");

            var pingRpc = new Ping(new PingPayload("pong"));
            var pingJson = JsonConvert.SerializeObject(pingRpc);
            sendTo(pingJson, senderId);

            Console.WriteLine("<< Pong");
        }

        public void OnLogin(string senderId, LoginPayload loginPayload)
        {
            Console.WriteLine(">> Login");

            var player = new Player(uidCounter++, loginPayload.Name, new Position(0f, 0f, 0f));
            players[player.Uid] = player;

            var loginResponseRpc = new LoginResponse(new LoginResponsePayload(player.Uid));
            var loginResponseJson = JsonConvert.SerializeObject(loginResponseRpc);
            sendTo(loginResponseJson, senderId);

            Console.WriteLine(player.ToString() + " login.");
        }

        public void OnPlayerUpdate(string senderId, PlayerUpdatePayload playerUpdatePayload)
        {
            Console.WriteLine(">> PlayerUpdate");

            Player player;
            if (players.TryGetValue(playerUpdatePayload.Id, out player))
            {
                player.SetPosition(playerUpdatePayload.Position);
            }
        }

        void Sync()
        {
            if (players.Count == 0) return;

            var movedPlayers = new List<RPC.Player>();
            foreach (var player in players.Values)
            {
                if (!player.isPositionChanged) continue;

                var playerRpc = new RPC.Player(player.Uid, player.Position);
                movedPlayers.Add(playerRpc);
                player.isPositionChanged = false;
            }

            if (movedPlayers.Count == 0) return;

            var syncRpc = new Sync(new SyncPayload(movedPlayers));
            var syncJson = JsonConvert.SerializeObject(syncRpc);
            broadcast(syncJson);
        }

        void StartSpawnTimer()
        {
            var random = new Random();
            var timer = new Timer(3000);
            timer.Elapsed += (_, __) =>
            {
                if (players.Count == 0) return;

                var randomX = random.Next(-5, 5);
                var randomZ = random.Next(-5, 5);
                var position = new Position(randomX, 0.5f, randomZ);
                var spawnRpc = new Spawn(new SpawnPayload(position));
                var spawnJson = JsonConvert.SerializeObject(spawnRpc);
                broadcast(spawnJson);

                Console.WriteLine("<< Spawn");
            };
            timer.Start();
        }
    }
}
