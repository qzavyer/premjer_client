using System;

namespace TicketClient
{
    public class Film
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public decimal Price { get; set; }
        public int FreeCount { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}