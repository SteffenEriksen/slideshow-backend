using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageUploadDemo.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ImageUploadDemo
{
    public class NotifyService
    {
        private readonly IHubContext<ImageHub> _hub;

        public NotifyService(IHubContext<ImageHub> hub)
        {
            _hub = hub;
        }

        public Task SendNotificationAsync(string message)
        {
            //return _hub.Clients.All.SendAsync("UploadedImage", message);
            return _hub.Clients.All.SendAsync("SendMessage", message);
        }
    }
}
