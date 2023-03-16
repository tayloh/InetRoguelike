using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Common;

namespace InetServer
{
    /// <summary>
    /// Author: Erik Stolpe (estolpe@kth.se)
    /// 
    /// A server class that connects and
    /// listens to two clients.
    /// </summary>

    public class Server
    {

        private GameState _game;

        public Server(GameState game)
        {
            _game = game;
        }

        public void StartListening()
        {
            // Data buffer.
            byte[] bytes = new byte[Protocol.BUFFER_SIZE];

            // Get local endpoint.
            IPHostEntry iPHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress iPAddress = iPHostInfo.AddressList.First();
            IPEndPoint localEndPoint = new IPEndPoint(iPAddress, Protocol.PORT);

            // TCP Socket for listening.
            Socket socketListener = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {

                // -----------------Initial setup-----------------

                socketListener.Bind(localEndPoint);
                socketListener.Listen(2);

                int player1ID = 1;
                int player2ID = 2;
                string onConnectMsgP1 = Protocol.STATUSMESSAGE("Connected! Waiting for another player.");
                string onPlayersConnectedMsg = Protocol.STATUSMESSAGE("All players connected! Game is ready.");

                // -----------------Player 1 setup-----------------

                // Wait for first player.
                Console.WriteLine("Waiting for Player 1 connection...");
                Socket p1Handler = socketListener.Accept();
                _game.AddClient(new PlayerClient(player1ID, p1Handler, new Player(1, 4, player1ID)));

                // Send on connect message to player 1.
                p1Handler.Send(_UTF8Bytes(onConnectMsgP1));
                p1Handler.Send(_UTF8Bytes(Protocol.SETPLAYER(player1ID)));

                // Send grid update to player 1.
                string gridMsg1 = Protocol.UPDATEGRID(_game.GetGameBoardUpdateMsg());
                p1Handler.Send(_UTF8Bytes(gridMsg1));

                // -----------------Player 2 setup-----------------

                // Wait for second player.
                Console.WriteLine("Waiting for Player 2 connection...");
                Socket p2Handler = socketListener.Accept();
                _game.AddClient(new PlayerClient(player2ID, p2Handler, new Player(8, 5, player2ID)));

                // Send on connect message.
                p2Handler.Send(_UTF8Bytes(Protocol.SETPLAYER(player2ID)));

                // -----------------Both connected-----------------
                Console.WriteLine("All players connected!");
                p1Handler.Send(_UTF8Bytes(onPlayersConnectedMsg));
                p2Handler.Send(_UTF8Bytes(onPlayersConnectedMsg));


                // Debug message the startingboard.
                Console.WriteLine(_game.PrettyPrintBoard());

                // Send the starting grid to all clients.
                _sendGridUpdateToClients();

                // Main listening loop.
                while (true)
                {
                    // For each client, listen, then act (perform gamelogic and send response). 
                    // If client could not recieve, it is treated as disconnnected
                    // and the server (and game) will shut down gracefully.
                    foreach (PlayerClient client in _game.PlayerClients) {

                        // If this player is dead, continue.
                        if (client.PlayerReference.Dead) continue;

                        // Continue recieving until player takes a valid step.
                        bool validStep = false;

                        while (!validStep)
                        {
                            Console.WriteLine("Reading from: " + client.ID);

                            // Send turn message to all clients.
                            string turnMsg = Protocol.GAMEMESSAGE("Player" + client.ID + ":s turn!");

                            foreach (PlayerClient c in _game.PlayerClients)
                            {
                                c.Connection.Send(_UTF8Bytes(turnMsg));
                            }

                            // Process data from client.
                            while (true)
                            {
                                int garbageLength = client.Connection.Available;
                                if (garbageLength != 0)
                                {
                                    // Dispose of data sent while it wasn't this clients turn.
                                    int temp = client.Connection.Receive(new byte[garbageLength]);
                                }

                                int bytesRec = client.Connection.Receive(bytes);
                                client.Data += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                                if (client.Data.Contains(Protocol.EOF))
                                {
                                    break;
                                }

                            }

                            int stepResult = -1;

                            // Perform game logic.
                            // See protocol.
                            switch (client.Data)
                            {
                                case Protocol.MOVEUP:
                                    stepResult = _game.MovePlayer(client.PlayerReference, Protocol.UP);
                                    break;

                                case Protocol.MOVEDOWN:
                                    stepResult = _game.MovePlayer(client.PlayerReference, Protocol.DOWN);
                                    break;

                                case Protocol.MOVELEFT:
                                    stepResult = _game.MovePlayer(client.PlayerReference, Protocol.LEFT);
                                    break;

                                case Protocol.MOVERIGHT:
                                    stepResult = _game.MovePlayer(client.PlayerReference, Protocol.RIGHT);
                                    break;

                                case Protocol.DISCONNECT:
                                    // Handle a disconnect request.
                                    _shutDownServer();
                                    break;

                                default:
                                    stepResult = 0; // Turn will change to next player.
                                    client.Connection.Send(Encoding.UTF8.GetBytes(Protocol.STATUSMESSAGE("Non valid request.")));
                                    break;
                            }

                            // Server debug messages.
                            Console.Clear();
                            Console.WriteLine("Recieved: " + client.Data);
                            Console.WriteLine(_game.PrettyPrintBoard());
                            Console.WriteLine(_game.GetGameBoardUpdateMsg());

                            // Reset data (get ready for next message).
                            client.Data = null;
                            
                            // Send an update of the grid to all clients.
                            _sendGridUpdateToClients();

                            // Act based on the step result.
                            if (stepResult == 0)
                            {
                                validStep = true;

                            } else if (stepResult == 1)
                            {
                                Console.WriteLine(client.ID + " killed an enemy.");
                            } else if (stepResult == 2)
                            {
                                Console.WriteLine(client.ID + " died.");

                                // Dying is a valid step, now the winning player
                                // can move around as he/she wishes.
                                validStep = true;
                            } else if (stepResult == 3)
                            {

                                // Tie and shut down.
                                Console.WriteLine("It's a tie.");
                                _sendTieMessage();
                                _shutDownServer();

                            } else if (stepResult == 4)
                            {
                                // Send inventory update.
                                Pickup item = client.PlayerReference.Equipment;
                                string inventoryMsg = Protocol.UPDATEINVENTORY(item.ItemId.ToString());
                                client.Connection.Send(_UTF8Bytes(inventoryMsg));

                                validStep = true;
                                Console.WriteLine(client.ID + " picked up " + item.ItemName);
                            } else if (stepResult == 5)
                            {
                                // Game is over.
                                // Shut down.
                                Console.WriteLine("Shutting down server.");
                                _sendWinMessage(client);
                                _shutDownServer();
                            }
                        }
                    }

                }

            }
            catch (SocketException e)
            {
                Console.WriteLine("It seems like a player disconnected! The game cannot continue.");
                Console.WriteLine("Sending EXIT message to connected clients before shutting down server...");
                _shutDownServer();
            }
            catch (Exception e)
            {
                Console.WriteLine("An unexpected error occured on the server.");
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("Press any key to close the program.");
            Console.ReadKey();
            Environment.Exit(0);
        }

        // Gracefully tells all connected clients they are about 
        // to be disconnected, then disconnects them.
        private void _shutDownServer()
        {
            // Send EXIT message to clients
            foreach (PlayerClient c in _game.PlayerClients)
            {
                try
                {
                    c.Connection.Send(Encoding.UTF8.GetBytes(Protocol.EXITMSG));

                } catch (SocketException e)
                {
                    Console.WriteLine("Client could not recieve : " + c.ID);
                }
            }

            // Then dispose->close the clients.
            _closeClients();

            Console.WriteLine("Press any key to close the program.");
            Console.ReadKey();
            Environment.Exit(0);
        }

        // Closes all client connections.
        private void _closeClients()
        {
            foreach (PlayerClient c in _game.PlayerClients)
            {
                try
                {
                    c.Connection.Close();
                } catch (SocketException e)
                {
                    Console.WriteLine("No such client (client already disconnected) : " + c.ID);
                }
                
            }
        }

        // Sends a message to all clients informing who won.
        // See protocol.
        private void _sendWinMessage(PlayerClient winningClient)
        {
            string msg = Protocol.GAMEMESSAGE("Player " + winningClient.ID + " wins!");

            foreach (PlayerClient c in _game.PlayerClients)
            {
                try
                {
                    c.Connection.Send(Encoding.UTF8.GetBytes(msg));
                } catch (SocketException e)
                {
                    Console.WriteLine("Client could not recieve : " + c.ID);
                }
            }
        }

        // Sends a message to all clients informing a tie.
        // See protocol.
        private void _sendTieMessage()
        {
            string msg = Protocol.GAMEMESSAGE("It's a tie!");

            foreach (PlayerClient c in _game.PlayerClients)
            {
                try
                {
                    c.Connection.Send(Encoding.UTF8.GetBytes(msg));
                }
                catch (SocketException e)
                {
                    Console.WriteLine("Client could not recieve : " + c.ID);
                }
            }
        }

        // Sends a grid update message to all clients.
        // See protocol.
        private void _sendGridUpdateToClients()
        {
            string gridMsg = Protocol.UPDATEGRID(_game.GetGameBoardUpdateMsg());

            foreach (PlayerClient c in _game.PlayerClients)
            {
                // Let the outside try catch take care of any errors here.
                    c.Connection.Send(_UTF8Bytes(gridMsg));
            }
        }

        // Converts a string to bytes using UTF-8.
        private byte[] _UTF8Bytes(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }
    }
}
