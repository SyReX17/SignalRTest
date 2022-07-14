namespace SignalRTest
{
    public class ConversationRoom
    {
        public string RoomName { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }

    public class RoomInfo
    {
        public string Name { get; set; }
        public int NumberOfClients { get; set; }

        public RoomInfo(string Name, int NumberOfClients)
        {
            this.Name = Name;
            this.NumberOfClients = NumberOfClients;
        }
    }
}
