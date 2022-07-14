using Microsoft.AspNetCore.SignalR;

namespace SignalRTest
{
    public static class HubContext
    {
        public static IHubContext<RoomsHub> hubContext { get; set; }

        public static async Task GetRooms()
        {
            List<RoomInfo> RoomInfoList = new List<RoomInfo>();

            if (Rooms.rooms.Count == 0)
            {
                await hubContext.Clients.All.SendAsync("ShareRoomInfo", RoomInfoList);
                await hubContext.Clients.All.SendAsync("Notify", RoomInfoList);
            }
            else
            {
                foreach (var room in Rooms.rooms)
                {
                    RoomInfo roominfo = new RoomInfo(room.RoomName, room.Users.Count);
                    RoomInfoList.Add(roominfo);
                }
            }
            await hubContext.Clients.All.SendAsync("ShareRoomInfo", RoomInfoList);
            await hubContext.Clients.All.SendAsync("Notify", RoomInfoList);
        }
    }
}
