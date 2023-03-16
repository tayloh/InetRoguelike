using Common;
using System.Collections.Generic;

namespace InetServer
{
    /// <summary>
    /// Author: Erik Stolpe (estolpe@kth.se)
    /// 
    /// A class for handling the state of the game
    /// and performing game logic.
    /// </summary>
    
    public class GameState
    {
        
        public List<PlayerClient> PlayerClients { get; private set;  }

        private Tile[,] _gameBoard;

        public GameState()
        {
            PlayerClients = new List<PlayerClient>();
            _gameBoard = new Tile[Protocol.BOARD_X, Protocol.BOARD_Y];

            // Build skeleton game level.
            for (int i = 0; i < _gameBoard.GetLength(0); i++)
            {
                for (int j = 0; j < _gameBoard.GetLength(1); j++)
                {
                    _gameBoard[i, j] = new Ground();
                }
            }

            // West wall
            _gameBoard[0, 0] = new Wall();
            _gameBoard[0, 1] = new Wall();
            _gameBoard[0, 2] = new Wall();
            _gameBoard[0, 3] = new Wall();
            _gameBoard[0, 4] = new Wall();
            _gameBoard[0, 5] = new Wall();
            _gameBoard[0, 6] = new Wall();
            _gameBoard[0, 7] = new Wall();
            _gameBoard[0, 8] = new Wall();
            _gameBoard[0, 9] = new Wall();

            // East wall
            _gameBoard[9, 0] = new Wall();
            _gameBoard[9, 1] = new Wall();
            _gameBoard[9, 2] = new Wall();
            _gameBoard[9, 3] = new Wall();
            _gameBoard[9, 4] = new Wall();
            _gameBoard[9, 5] = new Wall();
            _gameBoard[9, 6] = new Wall();
            _gameBoard[9, 7] = new Wall();
            _gameBoard[9, 8] = new Wall();
            _gameBoard[9, 9] = new Wall();

            // North wall
            _gameBoard[1, 0] = new Wall();
            _gameBoard[2, 0] = new Wall();
            _gameBoard[3, 0] = new Wall();
            _gameBoard[4, 0] = new Wall();
            _gameBoard[5, 0] = new Wall();
            _gameBoard[6, 0] = new Wall();
            _gameBoard[7, 0] = new Wall();
            _gameBoard[8, 0] = new Wall();

            // South wall
            _gameBoard[1, 9] = new Wall();
            _gameBoard[2, 9] = new Wall();
            _gameBoard[3, 9] = new Wall();
            _gameBoard[4, 9] = new Wall();
            _gameBoard[5, 9] = new Wall();
            _gameBoard[6, 9] = new Wall();
            _gameBoard[7, 9] = new Wall();
            _gameBoard[8, 9] = new Wall();

            // Other walls
            _gameBoard[1, 3] = new Wall();
            _gameBoard[1, 5] = new Wall();
            _gameBoard[2, 1] = new Wall();
            _gameBoard[2, 7] = new Wall();
            _gameBoard[3, 2] = new Wall();
            _gameBoard[3, 3] = new Wall();
            _gameBoard[3, 5] = new Wall();
            _gameBoard[3, 6] = new Wall();
            _gameBoard[3, 7] = new Wall();
            _gameBoard[5, 1] = new Wall();
            _gameBoard[5, 2] = new Wall();
            _gameBoard[5, 6] = new Wall();
            _gameBoard[5, 7] = new Wall();
            _gameBoard[7, 1] = new Wall();
            _gameBoard[7, 3] = new Wall();
            _gameBoard[7, 4] = new Wall();
            _gameBoard[7, 6] = new Wall();
            _gameBoard[7, 7] = new Wall();
            _gameBoard[8, 4] = new Wall();
            _gameBoard[8, 6] = new Wall();

            // Pickups
            _gameBoard[4, 7] = new Pickup("Stick", 1, 4, Protocol.STICK);
            _gameBoard[8, 1] = new Pickup("Sword", 3, 4, Protocol.SWORD);            

        }

        /// <summary>
        /// Adds a player client to the game state.
        /// </summary>
        /// <param name="client"></param>
        public void AddClient(PlayerClient client)
        {
            PlayerClients.Add(client);
            _gameBoard[client.PlayerReference.X, client.PlayerReference.Y] = client.PlayerReference;
        }

        /// <summary>
        /// Tries to move the player in the specified direction.
        /// See the protocol for valid directions.
        /// </summary>
        /// <param name="player">name of player to be moved.</param>
        /// <param name="direction">direction to move.</param>
        /// <returns>-1 if next tile is a wall,
        /// 1 if next tile is a player and win condition met,
        /// 2 if next tile is a player and lose condition met,
        /// 3 if next tile is a player and draw condition met,
        /// 4 if next tile is a pickup,
        /// 5 if next tile is blood,
        /// 0 else</returns>
        public int MovePlayer(Player player, string direction)
        {
            
            if (player.Dead) return 0;

            int x = player.X;
            int y = player.Y;

            switch (direction)
            {
                case Protocol.UP:
                    return _act(player, x, y - 1);

                case Protocol.DOWN:
                    return _act(player, x, y + 1);

                case Protocol.LEFT:
                    return _act(player, x - 1 , y);

                case Protocol.RIGHT:
                    return _act(player, x + 1, y);

                default: return -1;

            }

        }

        private Tile _getTile(int x, int y)
        {
            return _gameBoard[x, y];
        }

        // Returns -1 if next tile is a wall,
        //          1 if next tile is a player and win condition met,
        //          2 if next tile is a player and lose condition met,
        //          3 if next tile is a player and draw condition met,
        //          4 if next tile is a pickup,
        //          5 if next tile is blood, signal game end,
        //          0 else.
        // Performs manipulation of gameboard accordingly.
        private int _act(Player player, int nextX, int nextY)
        {
            Tile nextTile = _getTile(nextX, nextY);

            if (nextTile is Wall)
            {
                return -1;
            }
            else if (nextTile is Pickup)
            {
                player.Equipment = nextTile as Pickup;

                // Move the player to where the pickup was.
                _gameBoard[player.X, player.Y] = new Ground();
                player.X = nextX;
                player.Y = nextY;
                _gameBoard[player.X, player.Y] = player;

                return 4;
            }
            else if (nextTile is DeadPlayer)
            {
                _gameBoard[player.X, player.Y] = new Ground();
                player.X = nextX;
                player.Y = nextY;
                _gameBoard[player.X, player.Y] = player;
                return 5;
            }
            else if (nextTile is Player)
            {
                Player otherPlayer = nextTile as Player;
                // Do damage comparison and win check condition
                // If no one has pickup, dont move the player
                Pickup eq1 = player.Equipment;
                Pickup eq2 = otherPlayer.Equipment;

                // None has weapon.
                if (eq1 == null && eq2 == null) return -1;

                // One has weapon.
                if (eq1 != null && eq2 == null)
                {
                    _gameBoard[otherPlayer.X, otherPlayer.Y] = new DeadPlayer();
                    otherPlayer.Dead = true;
                    return 1;
                }
                if (eq1 == null && eq2 != null)
                {
                    _gameBoard[player.X, player.Y] = new DeadPlayer();
                    player.Dead = true;
                    return 2;
                }

                // Both have weapon.
                if (eq1 != null && eq2 != null)
                {
                    int dmgP1 = eq1.DoDamage();
                    int dmgP2 = eq2.DoDamage();

                    if (dmgP1 > dmgP2)
                    {
                        _gameBoard[otherPlayer.X, otherPlayer.Y] = new DeadPlayer();
                        otherPlayer.Dead = true;
                        return 1;
                    }
                    if (dmgP1 < dmgP2)
                    {
                        _gameBoard[player.X, player.Y] = new DeadPlayer();
                        player.Dead = true;
                        return 2;
                    }
                    if (dmgP1 == dmgP2)
                    {
                        _gameBoard[player.X, player.Y] = new DeadPlayer();
                        _gameBoard[otherPlayer.X, otherPlayer.Y] = new DeadPlayer();
                        player.Dead = true;
                        otherPlayer.Dead = true;
                        return 3;
                    }
                }

                // Must return something (redundant).
                return -1;
            }
            else
            {
                // Set ground where the player is, then move the player
                // to the new position.
                _gameBoard[player.X, player.Y] = new Ground();
                player.X = nextX;
                player.Y = nextY;
                _gameBoard[player.X, player.Y] = player;
                return 0;
            }
        } 
        
        /// <summary>
        /// Gets a pretty printed string containing
        /// the game grid.
        /// </summary>
        /// <returns>String with game grid.</returns>
        public string PrettyPrintBoard()
        {
            string res = "";
            for (int y = 0; y < _gameBoard.GetLength(0); y++)
            {
                for (int x = 0; x < _gameBoard.GetLength(1); x++)
                {
                    Tile t = _gameBoard[x, y];
                    string symbol = "";

                    if (t is Ground)
                    {
                        symbol = " ";
                    } else if (t is Wall)
                    {
                        symbol = "+";
                    } else if (t is Pickup)
                    {
                        symbol = "*";
                    } else if (t is Player)
                    {
                        symbol = "P";
                    } else if (t is DeadPlayer)
                    {
                        symbol = "_";
                    }

                    res += symbol;
                }
                res += "\n";
            }

            return res;
        } 

        /// <summary>
        /// Gets a update message for the game grid
        /// according to the protocol.
        /// </summary>
        /// <returns>String with protocol game grid.</returns>
        public string GetGameBoardUpdateMsg()
        {
            string res = "";

            for (int y = 0; y < _gameBoard.GetLength(0); y++)
            {
                for (int x = 0; x < _gameBoard.GetLength(1); x++)
                {
                    string tileType = _gameBoard[x, y].ToString();
                    res += tileType + "_" + x + "_" + y;
                }
            }

            return res;
        }

    }
}
