namespace SignalRTest
{
    public class ConversationRoom
    {
        public string RoomName { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
