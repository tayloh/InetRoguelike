
namespace InetServer
{
    /// <summary>
    /// Author: Erik Stolpe (estolpe@kth.se)
    /// 
    /// Creates a game state and starts a server
    /// which listens to two clients and lets them 
    /// perform movement which is handled by the 
    /// game state. Responses are then sent to
    /// clients regarding the game state.
    /// 
    /// See the protocol for technical information.
    /// </summary>

    public class Program
    {
        static void Main(string[] args)
        {
            GameState game = new GameState();
            Server server = new Server(game);

            // Start server.
            server.StartListening();
        }
    }
}
