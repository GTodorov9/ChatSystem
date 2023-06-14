namespace ChatSystem.Models
{
    public class User
    {
        public required string Id { get; set; }
        public List<Connection> Connections { get; set; } = new();
        public string OnlineStatus
        {
            get
            {
                return Connections.Any(e => e.IsConnected) ? "green" : "red";
            }
        }
    }
}
