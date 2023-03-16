using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Protocol
    {
        // Specifies 'connection status' or 'game' related message.
        public const string STATUS = "STATUS";
        public const string GAME = "GAME";
        public const string EXIT = "EXIT";

        public const string MESSAGE = "MESSAGE";
        public const string UPDATE = "UPDATE";

        public const string GRID = "GRID";
        public const string INVENTORY = "INVENTORY";
        public const string PLAYER = "PLAYER";

        // End of message.
        public const string EOF = "EOF";

        // Movement message.
        public const string MOVE = "MOVE";
        public const string UP = "UP";
        public const string DOWN = "DOWN";
        public const string LEFT = "LEFT";
        public const string RIGHT = "RIGHT";

        public static readonly List<string> Directions = new List<string> { "UP", "DOWN", "LEFT", "RIGHT" };

        // Item ID references.
        public const int STICK = 1;
        public const int SWORD = 2;

        // -------------------Shortcuts-------------------
        // -----------------------------------------------
        public const string MOVEUP = MOVE + " " + UP + " " + EOF;
        public const string MOVEDOWN = MOVE + " " + DOWN + " " + EOF;
        public const string MOVELEFT = MOVE + " " + LEFT + " " + EOF;
        public const string MOVERIGHT = MOVE + " " + RIGHT + " " + EOF;
        public const string EXITMSG = "EXIT" + " " + EOF;
        public const string DISCONNECT = "DISCONNECT" + " " + EOF;

        public static string GAMEMESSAGE(string message)
        {
            return GAME + " " + MESSAGE + " " + message + " " + EOF;
        }

        public static string STATUSMESSAGE(string message)
        {
            return STATUS + " " + MESSAGE + " " + message + " " + EOF;
        }

        public static string UPDATEINVENTORY(string inventoryString)
        {
            return UPDATE + " " + INVENTORY + " " + inventoryString + " " + EOF;
        }

        public static string UPDATEGRID(string gridString)
        {
            return UPDATE + " " + GRID + " " + gridString + " " + EOF;
        }

        public static string SETPLAYER(int id)
        {
            return UPDATE + " " + PLAYER + " " + id + " " + EOF;
        }
        // -------------------Shortcuts-------------------
        // -----------------------------------------------

        // Technical data.
        public const int PORT = 8080;
        public const int BUFFER_SIZE = 1024;
        public const int BOARD_SIZE = 100;
        public const int BOARD_X = 10;
        public const int BOARD_Y = 10;

        // SERVER PORT: 8080.
        // MESSAGE ENCODING: UTF-8.
        // -----------Server responses-----------

        // On connect first player:     STATUS MESSAGE 'Connected! Waiting for another player.' EOF
        //                              UPDATE PLAYER '1' EOF  
        //                              UPDATE GRID 'GridString' EOF

        // On connect second player:    UPDATE PLAYER '2' EOF                              
        //                              STATUS MESSAGE 'All players connected! Game is ready.' EOF  (to all)
        //
        // then:                        UPDATE GRID 'GridString' EOF                                (to all)

        // On new turn:                 GAME MESSAGE 'PlayerX turn!' EOF                            (to all)

        // If client not connected 
        // when trying to read:         EXIT EOF                                                    (to all)
        // 
        // On client sending  
        // sending DISCONNECT EOF
        // on their turn:               EXIT EOF                                                    (to all)

        // On match ended:              GAME MESSAGE 'Player X wins!' EOF                           (to all)
        //                              EXIT EOF                                                    (to all)

        // On pickup:                   UPDATE INVENTORY 'ItemId' EOF 
        // On move:                     UPDATE GRID 'GridString' EOF                                (to all)

        // On non valid request:        STATUS MESSAGE 'Non valid request.' EOF


        // -----------Client messages-----------

        // To move up:                             MOVE UP EOF
        // To move down:                           MOVE DOWN EOF
        // To move left:                           MOVE LEFT EOF
        // To move right:                          MOVE RIGHT EOF
        // To signal disconnect:                   DISCONNECT EOF

        // -----------GridString documentation-----------
        // GridString is a string packaged to bytes using UTF8,
        // which contains data about the game grid.
        // Its intended use is to update a game gui.
        //
        // Format: 
        // TileType_x_yTileType_x_yTileType_x_y... etc.
        //
        // TileTypes: 
        // Ground = 'G'
        // Wall = 'W'
        // DeadPlayer = 'D'
        // Player = 'P'+playerId    
        // Pickup = 'U'+itemId
        // where itemId, playerId are numbers.
        //
        // ItemId:s => Stick=1, Sword=2
        // PlayerId:s => Player1=1, Player2=2
        //
        // GridString EXAMPLE (Gridsize 10x10):
        // W_0_0W_1_0W_2_0W_3_0W_4_0W_5_0W_6_0W_7_0W_8_0W_9_0W_0_1G_1_1G_2_1G_3_1G_4_1G_5_1G_6_1G_7_1G_8_1W_9_1W_0_2G_1_2G_2_2G_3_2
        // G_4_2U2_5_2G_6_2G_7_2G_8_2W_9_2W_0_3G_1_3G_2_3G_3_3G_4_3G_5_3G_6_3G_7_3G_8_3W_9_3W_0_4G_1_4P1_2_4G_3_4G_4_4G_5_4G_6_4G_7_4
        // G_8_4W_9_4W_0_5G_1_5G_2_5G_3_5G_4_5G_5_5G_6_5G_7_5P2_8_5W_9_5W_0_6G_1_6G_2_6G_3_6G_4_6G_5_6G_6_6G_7_6G_8_6W_9_6W_0_7G_1_7
        // G_2_7G_3_7U1_4_7G_5_7G_6_7G_7_7G_8_7W_9_7W_0_8G_1_8G_2_8G_3_8G_4_8G_5_8G_6_8G_7_8G_8_8W_9_8W_0_9W_1_9W_2_9W_3_9W_4_9W_5_9
        // W_6_9W_7_9W_8_9W_9_9

        // -----------ItemId documentation-----------
        // An id describing what item was picked up.
        // Sword ID = 2
        // Stick ID = 1

    }
}
