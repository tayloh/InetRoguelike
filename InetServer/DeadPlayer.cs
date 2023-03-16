

namespace InetServer
{
    /// <summary>
    /// Author: Erik Stolpe (estolpe@kth.se)
    /// 
    /// A class for representing a dead player
    /// on the game grid in the game logic.
    /// </summary>
    
    class DeadPlayer : Tile
    {
        public override string ToString()
        {
            return "D";
        }
    }
}
