
using System.Net.Sockets;


namespace InetServer
{
    /// <summary>
    /// Author: Erik Stolpe (estolpe@kth.se)
    /// 
    /// A class for handling a client connected
    /// to a player on the game grid.
    /// </summary>

    public class PlayerClient
    {

        public readonly int ID;
        public Socket Connection { get; private set; }
        public string Data = null;
        public Player PlayerReference;

        public PlayerClient(int playerID, Socket connection, Player playerRef)
        {
            ID = playerID;
            PlayerReference = playerRef;
            Connection = connection;
        }

    }
}
