using System.Collections.Generic;

namespace Bespoker.Web.Models
{
    public class PokerSession
    {
        public PokerSession()
        {
            CardsRevealed = false;
            NextPlayerNum = 1;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public List<Player> Players { get; set; }
        public bool CardsRevealed { get; set; }
        public int NextPlayerNum { get; set; }
    }
}