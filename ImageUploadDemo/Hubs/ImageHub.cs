using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ImageUploadDemo.Hubs
{
    public class ImageHub : Hub
    {
        public async Task SendMessage(string msg)
        {
            await Clients.All.SendAsync("UploadedImage", msg);
        }

        //public async Task SendMessageCaller(string user, string message)
        //{
        //    await Clients.Caller.SendAsync("UploadedImage", user, message);
        //}
    }
}
