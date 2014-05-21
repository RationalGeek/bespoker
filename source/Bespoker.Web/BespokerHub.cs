using Bespoker.Web.Models;
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
        // Singleton - always in memory managing persistent poker sessions
        private static Lazy<SessionManager> _instance = new Lazy<SessionManager>(() => new SessionManager(GlobalHost.ConnectionManager.GetHubContext<BespokerHub>()));
        public static SessionManager Instance { get { return _instance.Value; } }

        private IHubConnectionContext _clients;
        private IGroupManager _groups;
        private IDictionary<string, PokerSession> _sessions = new ConcurrentDictionary<string, PokerSession>();
        private IDictionary<string, string> _connectionIdToGroup = new ConcurrentDictionary<string, string>();

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
            var player = session.Players.SingleOrDefault(p => p.ConnectionId == connId);
            if (player == null)
            {
                // Create player
                player = new Player
                {
                    ConnectionId = connId,
                    Id = Guid.NewGuid().ToString(),
                    Name = "Player " + session.NextPlayerNum++,
                };

                // Add player to session
                session.Players.Add(player);

                // Add player to SignalR group
                _groups.Add(connId, session.Name);

                // Track in dictionary so that we can map users back to sessions
                _connectionIdToGroup.Add(connId, session.Name);
            }

            // Session and player are initialized - init client
            _clients.Client(connId).InitSession(session);

            // Tell the other clients in this group that a player joined
            _clients.Group(session.Name, connId).PlayerJoined(player);
        }

        public PokerSession FindSessionByConnectionId(string connectionId)
        {
            if (_connectionIdToGroup.ContainsKey(connectionId))
            {
                var sessionName = _connectionIdToGroup[connectionId];
                if (_sessions.ContainsKey(sessionName))
                    return _sessions[sessionName];
            }
            return null;
        }

        public void SelectCard(string connectionId, string cardValue)
        {
            var session = FindSessionByConnectionId(connectionId);
            if (session != null)
            {
                var player = session.Players.SingleOrDefault(p => p.ConnectionId == connectionId);
                if (player != null)
                {
                    player.SelectedCard = cardValue;
                    _clients.Group(session.Name).CardSelected(player.Id, cardValue);
                }
            }
        }

        public void ShowCards(string sessionName)
        {
            _clients.Group(sessionName).ShowCards();
        }

        public void HideCards(string sessionName)
        {
            _clients.Group(sessionName).HideCards();
        }

        public void Reset(string sessionName)
        {
            if (_sessions.ContainsKey(sessionName))
            {
                var session = _sessions[sessionName];

                // Set all players selected card to null
                session.Players.ForEach(p => p.SelectedCard = null);

                // Cards hidden
                session.CardsRevealed = false;

                // Send reset message to all players
                _clients.Group(sessionName).Reset(session);
            }
        }

        public void ChangeName(string connectionId, string newName)
        {
            var session = FindSessionByConnectionId(connectionId);
            if (session != null)
            {
                var player = session.Players.SingleOrDefault(p => p.ConnectionId == connectionId);
                if (player != null)
                {
                    player.Name = newName;
                    _clients.Group(session.Name).NewPlayerName(player.Id, newName);
                }
            }
        }

        public void RemovePlayer(string connectionId)
        {
            var session = FindSessionByConnectionId(connectionId);
            if (session != null)
            {
                var player = session.Players.SingleOrDefault(p => p.ConnectionId == connectionId);
                if (player != null)
                {
                    session.Players.Remove(player);
                    _connectionIdToGroup.Remove(connectionId);
                    _clients.Group(session.Name).RemovePlayer(player.Id);

                    if (session.Players.Count == 0)
                        _sessions.Remove(session.Name);
                }
            }
        }

        private PokerSession BuildNewSession(string sessionName)
        {
            var session = new PokerSession { Id = Guid.NewGuid().ToString(), Name = sessionName, Players = new List<Player>() };
            return session;
        }
    }

    public class BespokerHub : Hub
    {
        public void RegisterForSession(string sessionName)
        {
            Clients.Caller.SessionName = sessionName;
            SessionManager.Instance.RegisterForSession(sessionName, Context.ConnectionId);
        }

        public void SelectCard(string cardValue)
        {
            SessionManager.Instance.SelectCard(Context.ConnectionId, cardValue);
        }

        public void ShowCards()
        {
            SessionManager.Instance.ShowCards(Clients.Caller.SessionName);
        }

        public void HideCards()
        {
            SessionManager.Instance.HideCards(Clients.Caller.SessionName);
        }

        public void Reset()
        {
            SessionManager.Instance.Reset(Clients.Caller.SessionName);
        }

        public void ChangeName(string newName)
        {
            SessionManager.Instance.ChangeName(Context.ConnectionId, newName);
        }

        public override System.Threading.Tasks.Task OnDisconnected()
        {
            SessionManager.Instance.RemovePlayer(Context.ConnectionId);
            return base.OnDisconnected();
        }
    }
}