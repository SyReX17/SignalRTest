using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace SignalRTest
{
    public class RoomsHub : Hub
    {
        bool stop;
        public override async Task OnConnectedAsync()
        {
            stop = false;
            await Clients.All.SendAsync("Notify", $"{Context.ConnectionId} вошел");
            var count = Rooms.rooms.Count;
            while (!stop)
            {
                if(Rooms.rooms.Count != count)
                {
                    count = Rooms.rooms.Count;
                    await ShareRoomInfo();
                }
            }
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

        public async Task Leave()
        {
            stop = true;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Leave();
            await base.OnDisconnectedAsync(exception);
        }
    }
}