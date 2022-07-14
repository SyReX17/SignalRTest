using Microsoft.AspNetCore.SignalR;
using SignalRTest;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
var app = builder.Build();

app.UseDeveloperExceptionPage();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();



app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chat");
    endpoints.MapHub<RoomsHub>("/rooms");
    endpoints.MapGet("/api/room", () =>
    {
        List<RoomInfo> RoomInfoList = new List<RoomInfo>();

        if (Rooms.rooms.Count == 0)
            return RoomInfoList;
        //Results.NotFound(new { message = "Не найдено ни одной комнаты." });
        else
        {
            foreach (var room in Rooms.rooms)
            {
                RoomInfo roominfo = new RoomInfo(room.RoomName, room.Users.Count);
                RoomInfoList.Add(roominfo);
            }
        }
        return RoomInfoList;
    });
    endpoints.MapPost("/api/room", async (HttpContext context) =>
    {
        string roomName = "";

        using (var reader = new StreamReader(context.Request.Body))
        {
            roomName = await reader.ReadToEndAsync();
        }
        var room = Rooms.rooms.FirstOrDefault(room => room.RoomName == roomName);

        if (room == null)
        {
            ConversationRoom cr = new ConversationRoom()
            {
                RoomName = roomName,
                Users = new List<User>()
            };

            Rooms.rooms.Add(cr);

            Console.WriteLine(Rooms.rooms.Count.ToString());
        }
    });
});

app.Run();
