namespace SignalRTest
{
    public class User
    {
        public string UserName { get; set; }

        public string FullName { get; set; }
        public ICollection<Connection> Connections { get; set; }
        public virtual ICollection<String> Rooms { get; set; }
    }
}
