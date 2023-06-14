using ChatSystem.Models.DataModels;
using ChatSystem.Models.ViewModels;
using ChatSystem.StaticData;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace ChatSystem.Hubs;

public class ChatSystemHub : Hub
{

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public void SendChatMessage(string toUserId, string message)
    {
        string currentSessionUserId = Context.GetHttpContext().Session.GetString("UserGuid")!;
        var newMessage = new Message
        {
            Content = message,
            FromUser_Id = currentSessionUserId,
            ToUser_Id = toUserId,
            PublishedOn = DateTime.Now
        };
        //Add message to memory
        MessageCache.Messages.Add(newMessage);
        //Send message to users
        var currentConnectionOtherUserId = ConnectedChatUsers.Users.FirstOrDefault(e => e.Id == toUserId).Connections.FirstOrDefault(e => e.IsConnected).Id;
        Clients.Client(currentConnectionOtherUserId).SendAsync("ReceiveMessage", newMessage);

    }

    public override Task OnConnectedAsync()
    {
        string currentSessionUserId = Context.GetHttpContext().Session.GetString("UserGuid")!;
        string connectionId = Context.ConnectionId;
        var currentLoggedUser = ConnectedChatUsers.Users.FirstOrDefault(e => e.Id == currentSessionUserId);
        currentLoggedUser.Connections.Add(new Models.Connection { IsConnected = true, Id = connectionId });

        // ConnectedChatUsers.Users.Add(new Models.User { Id = connectionId, IsConnected = true });
        Clients.All.SendAsync("ShowUsers", ConnectedChatUsers.Users);

        // Clients.AllExcept(ConnectedChatUsers.Users.Where(i => i != connectionId))
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        string currentSessionUserId = Context.GetHttpContext().Session.GetString("UserGuid");
        string connectionId = Context.ConnectionId;

        var disconectedUser = ConnectedChatUsers.Users.FirstOrDefault(e => e.Id == currentSessionUserId);
        if (disconectedUser != null)
        {
            var currentConnection = disconectedUser.Connections.FirstOrDefault(e => e.Id == connectionId);
            if (currentConnection != null)
            {
                currentConnection.IsConnected = false;
            }

        }


        Clients.All.SendAsync("ShowUsers", ConnectedChatUsers.Users);

        return base.OnDisconnectedAsync(exception);
    }


}
