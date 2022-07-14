using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace SignalRTest
{
    public class RoomsHub : Hub
    {
        public static List<User> users = new List<User>();
        public static List<Connection> connections = new List<Connection>();
        public static List<ConversationRoom> rooms = new List<ConversationRoom>();
        public override async Task OnConnectedAsync()
        {
            // Retrieve user.
            var user = users.FirstOrDefault(user => user.UserName == Context.ConnectionId);

            // If user does not exist in database, must add.
            if (user == null)
            {
                user = new User()
                {
                    UserName = Context.ConnectionId
                };


                users.Add(user);
            }
            else
            {
                // Add to each assigned group.
                foreach (var item in user.Rooms)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, item);
                }
            }

            await Clients.All.SendAsync("Notify", $"{Context.ConnectionId} вошел");
            await base.OnConnectedAsync();
        }
        public async Task ShareRoomInfo()
        {
            var roomNames = rooms.Select(room => room.RoomName).ToList();
            await Clients.All.SendAsync("ShareRoomInfo", rooms);
            await Clients.All.SendAsync("Notify", rooms);
        }

        public async Task Join(string roomName)
        {
            var room = rooms.FirstOrDefault(room => room.RoomName == roomName);

            var testrooms = rooms.Select(r => r.RoomName).ToList();
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
                    Rooms = new List<string>()
                };

                user.Rooms.Add(roomName);
                users.Add(user);
                room.Users.Add(user);
                await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

                await Clients.Group(roomName).SendAsync("Notify", $"{Context.ConnectionId} вошел в чат {roomName}");

                await ShareRoomInfo();
            }
            else
            {
                Console.WriteLine("Комната не найдена!!!");
            }
        }

        public async Task Create(string roomName)
        {
            var room = rooms.FirstOrDefault(room => room.RoomName == roomName);

            if (room == null)
            {
                ConversationRoom cr = new ConversationRoom()
                {
                    RoomName = roomName,
                    Users = new List<User>()
                };

                rooms.Add(cr);

                Console.WriteLine(rooms.Count.ToString());

                await Clients.All.SendAsync("Notify", $"{Context.ConnectionId} создал чат {roomName}");


                await Join(roomName);
            }
        }

        public async Task Leave(string roomName)
        {
            var room = rooms.FirstOrDefault(room => room.RoomName == roomName);

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
                        rooms.Remove(room);
                    }
                    await Clients.Group(roomName).SendAsync("Notify", $"{Context.ConnectionId} покинул чат {room.RoomName}");
                }
                await ShareRoomInfo();
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = users.FirstOrDefault(user => user.UserName == Context.ConnectionId);
            if (user != null)
            {
                await Clients.All.SendAsync("Notify", user);
                users.Remove(user);
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