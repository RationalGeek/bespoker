using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Bespoker.Web
{
    public class BespokerHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello(DateTime.Now.ToString("T"));
        }
    }
}