using WebSocketSample.RPC;

namespace WebSocketSample.Server
{
    public class Item
    {
        public int Id;
        public Position Position;

        public Item(int id, Position position)
        {
            Id = id;
            Position = position;
        }
    }
}
