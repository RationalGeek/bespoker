using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bespoker.Web.Models
{
    public class PokerSession
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Player> Players { get; set; }
    }
}