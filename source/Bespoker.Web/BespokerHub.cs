using Microsoft.AspNet.SignalR;

namespace Bespoker.Web
{
    public class BespokerHub : Hub
    {
        //public void Hello()
        //{
        //    Clients.All.hello(DateTime.Now.ToString("T"));
        //}

        public void RegisterForSession(string sessionName)
        {
            // Find the session if it exists
        }
    }
}