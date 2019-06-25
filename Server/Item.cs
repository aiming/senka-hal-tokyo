using WebSocketSample.RPC;

namespace WebSocketSample.Server
{
    public class Item
    {
        public readonly int Id;
        public readonly int Type;
        public readonly Position Position;

        public Item(int id, Position position)
        {
            Id = id;
            Position = position;
        }
    }
}

