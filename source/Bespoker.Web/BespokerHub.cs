﻿using Bespoker.Web.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Bespoker.Web
{
    public class SessionManager
    {
        private static Lazy<SessionManager> _instance = new Lazy<SessionManager>(() => new SessionManager(GlobalHost.ConnectionManager.GetHubContext<BespokerHub>()));
        public static SessionManager Instance { get { return _instance.Value; } }

        private IHubConnectionContext _clients;
        private IGroupManager _groups;
        private IDictionary<string, PokerSession> _sessions = new ConcurrentDictionary<string, PokerSession>();

        private SessionManager(IHubContext hubContext)
        {
            _clients = hubContext.Clients;
            _groups = hubContext.Groups;
        }

        public void RegisterForSession(string sessionName, string connId)
        {
            // Find the session if it exists, or create a new one
            if (!_sessions.ContainsKey(sessionName))
                _sessions.Add(sessionName, BuildNewSession(sessionName));
            var session = _sessions[sessionName];

            // Find the player if already attached to session, otherwise create
            //var connId = Context.ConnectionId;
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
                _groups.Add(connId, session.Name);
            }

            // Session and player are initialized - init client
            _clients.Client(connId).InitSession(session);

            // Tell the other clients in this group that a player joined
            _clients.Group(session.Name, connId).PlayerJoined(player);
        }

        private PokerSession BuildNewSession(string sessionName)
        {
            var session = new PokerSession { Id = Guid.NewGuid().ToString(), Name = sessionName, Players = new List<Player>() };
            return session;
        }
    }

    public class BespokerHub : Hub
    {
        /// <summary>
        /// Called when a new player joins in.
        /// </summary>
        public void RegisterForSession(string sessionName)
        {
            SessionManager.Instance.RegisterForSession(sessionName, Context.ConnectionId);
        }
    }
}