using Microsoft.AspNetCore.SignalR;

namespace SignalRTest
{
    
    public class ChatHub : Hub
    {
        
        public async Task Join(string roomName, string fullName)
        {
            Console.WriteLine(roomName);
            var currentUser = Rooms.users.FirstOrDefault(user => user.UserName == Context.ConnectionId);

            if (currentUser == null)
            {
                Console.WriteLine("OK");
                var room = Rooms.rooms.FirstOrDefault(room => room.RoomName == roomName);

                var testrooms = Rooms.rooms.Select(r => r.RoomName).ToList();
                foreach (var rm in testrooms)
                {
                    Console.WriteLine(rm);
                }

                if (room != null)
                {

                    if (room.Users != null)
                    {
                        var clients = room.Users.Select(user => user.UserName).ToList();

                        await Clients.Group(roomName).SendAsync("AddPeer", Context.ConnectionId, false);

                        foreach (var client in clients)
                        {
                            await Clients.Caller.SendAsync("AddPeer", client, true);
                        }
                    }

                    var user = new User()
                    {
                        UserName = Context.ConnectionId,
                        Rooms = new List<string>(),
                        FullName = fullName
                    };

                    user.Rooms.Add(roomName);
                    Rooms.users.Add(user);
                    room.Users.Add(user);
                    await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

                    await Clients.Group(roomName).SendAsync("Notify", $"{Context.ConnectionId} вошел в чат {roomName}");

                    await GetUsers(room);
                    await HubContext.GetRooms();
                }
                else
                {
                    Console.WriteLine("Комната не найдена!!!");
                }
            }
        }

        public async Task GetUsers(ConversationRoom room)
        {
            List<string> userNames = new List<string>();

            Console.WriteLine(room.Users.Count.ToString());

            foreach (var roomuser in room.Users)
            {
                userNames.Add(roomuser.FullName);
            }
            await Clients.Group(room.RoomName).SendAsync("RoomInfo", userNames);
        }

        public async Task Create(string roomName)
        {
            var room = Rooms.rooms.FirstOrDefault(room => room.RoomName == roomName);

            if (room == null)
            {
                ConversationRoom cr = new ConversationRoom()
                {
                    RoomName = roomName,
                    Users = new List<User>()
                };

                Rooms.rooms.Add(cr);
            }
        }

        public async Task Leave(string roomName)
        {
            var room = Rooms.rooms.FirstOrDefault(room => room.RoomName == roomName);

            if (room != null)
            {
                var user = room.Users.FirstOrDefault(user => user.UserName == Context.ConnectionId);


                if (user != null)
                {
                    Console.WriteLine("Work");

                    if (room.Users.Count > 1)
                    {
                        var clients = room.Users.Select(user => user.UserName).ToList();

                        await Clients.Group(room.RoomName).SendAsync("RemovePeer", Context.ConnectionId);

                        foreach (var client in clients)
                        {
                            await Clients.Caller.SendAsync("RemovePeer", client);
                        }
                    }

                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.RoomName);
                    room.Users.Remove(user);
                    user.Rooms.Remove(roomName);
                    if (room.Users.Count == 0)
                    {
                        Rooms.rooms.Remove(room);
                    }
                    await GetUsers(room);
                    await HubContext.GetRooms();
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = Rooms.users.FirstOrDefault(user => user.UserName == Context.ConnectionId);
            if (user != null)
            {
                await Clients.All.SendAsync("Notify", user);
                Rooms.users.Remove(user);
                if (user.Rooms != null)
                {
                    foreach (var room in user.Rooms)
                    {
                        await Leave(room);
                    }
                }
            }
        }

        public async Task RelaySDP(string peerId, string sessionDescription)
        {
            await Clients.Client(peerId).SendAsync("SessionDescription", Context.ConnectionId, sessionDescription);
        }

        public async Task RelayICE(string peerId, string iceCandidate)
        {
            await Clients.Client(peerId).SendAsync("IceCandidate", Context.ConnectionId, iceCandidate);
        }
    }
}
