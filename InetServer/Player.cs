

namespace InetServer
{
    /// <summary>
    /// Author: Erik Stolpe (estolpe@kth.se)
    /// 
    /// A class for representing a player tile
    /// on the game grid in the game logic.
    /// Contains functionality for keeping
    /// track of the player.
    /// </summary>

    public class Player : Tile
    {

        public Pickup Equipment;
        public int X, Y;
        public bool Dead;

        private readonly int _idReference;

        public Player(int x, int y, int idRef)
        {
            Equipment = null;
            Dead = false;
            X = x;
            Y = y;
            _idReference = idRef;
        }

        public override string ToString()
        {
            return "P" + _idReference;
        }
    }
}
