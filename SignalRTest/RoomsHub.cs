using Microsoft.AspNetCore.SignalR;

namespace SignalRTest
{
    public class RoomsHub : Hub
    {
        public IHubContext<RoomsHub> _hubContext;
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("Connect");
            await base.OnConnectedAsync();
        }
        public async Task ShareRoomInfo()
        {
            List<RoomInfo> RoomInfoList = new List<RoomInfo>();

            if (Rooms.rooms.Count == 0)
            {
                await Clients.Caller.SendAsync("ShareRoomInfo", RoomInfoList);
                await Clients.Caller.SendAsync("Notify", RoomInfoList);
            }
            else
            {
                foreach (var room in Rooms.rooms)
                {
                    RoomInfo roominfo = new RoomInfo(room.RoomName, room.Users.Count);
                    RoomInfoList.Add(roominfo);
                }
            }
            await Clients.Caller.SendAsync("ShareRoomInfo", RoomInfoList);
            await Clients.Caller.SendAsync("Notify", RoomInfoList);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}