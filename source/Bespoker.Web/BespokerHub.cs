using Bespoker.Web.Models;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bespoker.Web
{
    public class BespokerHub : Hub
    {
        public BespokerHub()
        {
            _Sessions = new Dictionary<string, PokerSession>();
        }

        private IDictionary<string, PokerSession> _Sessions { get; set; }

        /// <summary>
        /// Called when a new player joins in.
        /// </summary>
        public void RegisterForSession(string sessionName)
        {
            // Find the session if it exists, or create a new one
            if (!_Sessions.ContainsKey(sessionName))
                _Sessions.Add(sessionName, BuildNewSession(sessionName));
            var session = _Sessions[sessionName];

            // Find the player if already attached to session, otherwise create
            var connId = Context.ConnectionId;
            var player = session.Players.SingleOrDefault(p => p.ConnectionId == connId);
            if (player == null)
            {
                // Create player
                player = new Player
                {
                    ConnectionId = connId,
                    Id = Guid.NewGuid().ToString(),
                    Name = "Player" + (session.Players.Count() + 1),
                };

                // Add player to session
                session.Players.Add(player);

                // Add player to SignalR group
                Groups.Add(connId, session.Name);
            }

            // Session and player are initialized - init client
            Clients.Caller.InitSession(session);

            // Tell the other clients in this group that a player joined
            Clients.Group(session.Name, connId).PlayerJoined(player);
        }

        private PokerSession BuildNewSession(string sessionName)
        {
            var session = new PokerSession { Id = Guid.NewGuid().ToString(), Name = sessionName, Players = new List<Player>() };
            return session;
        }
    }
}