using System;
using WebSocketSharp.Server;

namespace WebSocketSample.Server
{
    public class GameServer
    {
        const string SERVICE_NAME = "/";
        const ConsoleKey EXIT_KEY = ConsoleKey.Q;

        GameModel model;

        WebSocketServer WebSocketServer;

        public GameServer(string address)
        {
            model = new GameModel();
            model.sendTo += SendTo;
            model.broadcast += Broadcast;
            WebSocketServer = new WebSocketServer(address);
            WebSocketServer.AddWebSocketService<GameService>(SERVICE_NAME, () =>
            {
                var service = new GameService();
                service.OnPing += model.OnPing;
                service.OnLogin += model.OnLogin;
                service.OnPlayerUpdate += model.OnPlayerUpdate;
                service.OnGetItem += model.OnGetItem;
                return service;
            });
        }

        public void RunForever()
        {
            WebSocketServer.Start();
            Console.WriteLine("Game Server started.");

            while (!IsInputtedExitKey())
            {
                model.OnUpdate();
            }
        }

        bool IsInputtedExitKey()
        {
            if (!Console.KeyAvailable) { return false; }

            switch (Console.ReadKey(true).Key)
            {
                default:
                    Console.WriteLine("Enter " + EXIT_KEY + " to exit the game.");
                    return false;

                case EXIT_KEY:
                    WebSocketServer.Stop();
                    Console.WriteLine("Game Server terminated.");
                    return true;
            }
        }

        void SendTo(string message, string id)
        {
            WebSocketServer.WebSocketServices[SERVICE_NAME].Sessions.SendTo(message, id);
            Console.WriteLine("<< SendTo: " + id + " " + message);
        }

        void Broadcast(string message)
        {
            WebSocketServer.WebSocketServices[SERVICE_NAME].Sessions.Broadcast(message);
            Console.WriteLine("<< Broeadcast: " + message);
        }
    }
}