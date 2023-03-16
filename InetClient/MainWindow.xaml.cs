using Common;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;

namespace InetClient
{
    /// <summary>
    /// Author: Erik Stolpe (estolpe@kth.se)
    /// 
    /// Interaction logic for MainWindow.xaml
    /// 
    /// Initializes a game view which is updated
    /// on the RecieveEvent.
    /// Listens to key presses for user input.
    /// </summary>
    public partial class MainWindow : Window
    {
        private Client _client;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += InitGrid;

            Loaded += ConnectClient;
            Closing += DisposeClient;
        }

        // Builds an empty game grid.
        private void InitGrid(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Protocol.BOARD_SIZE; i++)
            {
                GamePanel.Children.Add(new Image() {
                    Source = new BitmapImage(new Uri("/images/Empty.png", UriKind.Relative))
                });
            }
        }

        // Sends a disconnect request to the server and closes the socket
        // connection to the server.
        private void DisposeClient(object sender, CancelEventArgs e)
        {
            _client.SendMessage(Protocol.DISCONNECT);
            _client.StopListening();
        }

        // Connects a client to the server and starts to listen for updates.
        private void ConnectClient(object sender, RoutedEventArgs e)
        {
            _client = new Client();
            _client.Connect();

            _client.Recieved += UpdateGUI;
        }
        
        // Updates the GUI based on the data recieved from the server.
        private void UpdateGUI(object sender, RecieveEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Parse the incoming response.
                // Data may include one or more EOF tokens. Need to split on EOF.
                // This is an artifact of the BUFFER_SIZE.

                string[] responses = Regex.Split(e.Data, Protocol.EOF);

                foreach (string r in responses)
                {
                    // On response: STATUS MESSAGE 'Message' EOF
                    if (r.StartsWith(Protocol.STATUS + " " + Protocol.MESSAGE))
                    {
                        int length = (Protocol.STATUS + " " + Protocol.MESSAGE).Length + 1; // +1 to account for whitespace
                        string newtxt = r.Substring(length);

                        ConnectionInfoText.Text = newtxt;

                        // On response: GAME MESSAGE 'Message' EOF
                    }
                    else if (r.StartsWith(Protocol.GAME + " " + Protocol.MESSAGE))
                    {
                        int length = (Protocol.GAME + " " + Protocol.MESSAGE).Length + 1;
                        string newtxt = r.Substring(length);

                        GameInfoText.Text = newtxt;

                        // On response: UPDATE GRID 'GridString' EOF
                    }
                    else if (r.StartsWith(Protocol.UPDATE + " " + Protocol.GRID))
                    {
                        int length = (Protocol.UPDATE + " " + Protocol.GRID).Length + 1;
                        string gridData = r.Substring(length);

                        // Parse grid data.
                        int index = 0;
                        while (index < gridData.Length - 1) // One space at the end.
                        {
                            // Update GamePanel.

                            // Read type.
                            string tileType = gridData[index].ToString();
                            string playerID = string.Empty;
                            string itemID = string.Empty;
                            if (tileType == "P")
                            {
                                index++;
                                playerID = gridData[index].ToString();
                            }
                            else if (tileType == "U")
                            {
                                index++;
                                itemID = gridData[index].ToString();
                            }

                            // Read x and y coordinates.
                            index += 2; // Skip '_'.

                            int x = int.Parse(gridData[index].ToString());
                            index += 2; // Skip '_'.

                            int y = int.Parse(gridData[index].ToString());

                            string uri = "";
                            if (tileType == "W")
                            {
                                uri = "/images/Wall.png";
                            }
                            else if (tileType == "G")
                            {
                                uri = "/images/Ground2.png";
                            }
                            else if (tileType == "P")
                            {
                                uri = "/images/Player" + playerID + ".png";
                            }
                            else if (tileType == "U")
                            {
                                uri = "/images/Item" + itemID + ".png";
                            }
                            else if (tileType == "D")
                            {
                                uri = "/images/Blood.png";
                            }
                            else
                            {
                                uri = "/images/Empty.png";
                            }
                            (GamePanel.Children[(y * 10)+x] as Image).Source = new BitmapImage(new Uri(uri, UriKind.Relative));

                            index++;
                        }

                        // On response: UPDATE INVENTORY 'InventoryString' EOF
                    }
                    else if (r.StartsWith(Protocol.UPDATE + " " + Protocol.INVENTORY))
                    {
                        int length = (Protocol.UPDATE + " " + Protocol.INVENTORY).Length + 1;
                        string temp = r.Substring(length); // Will have a space after item name.

                        // Remove the space.
                        string itemId = temp.Substring(0, temp.Length - 1);

                        // Update inventory gui.
                        string uri = "/images/Item" + itemId + ".png";
                        Equipment.Source = new BitmapImage(new Uri(uri, UriKind.Relative));

                    }   // On response: UPDATE PLAYER 'id' EOF
                    else if (r.StartsWith(Protocol.UPDATE + " " + Protocol.PLAYER))
                    {
                        int length = (Protocol.UPDATE + " " + Protocol.PLAYER).Length + 1;
                        string playerId = r.Substring(length);

                        string color = "RED";
                        if (int.Parse(playerId) == 2) color = "BLUE";

                        Title =  "Your are " + color + ": Player" + playerId;

                    } // On response: EXIT EOF
                    else if (r.StartsWith(Protocol.EXIT))
                    {
                        // Close the server connection.
                        _client.StopListening();

                        // Signal to the user that the server was shut down.
                        ConnectionInfoText.Text = "Server was shut down. Please close.";
                    }
                }
            });
        }

        // Listens for keydown and sends according message to the server
        // to move the player. Informs the user if the server was offline.
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            int res = 1;
            if (e.Key == Key.Up)
            {
                string msg = Protocol.MOVEUP;
                res = _client.SendMessage(msg);

            } else if (e.Key == Key.Down)
            {
                string msg = Protocol.MOVEDOWN;
                res = _client.SendMessage(msg);
             
            } else if (e.Key == Key.Left)
            {
                string msg = Protocol.MOVELEFT;
                res = _client.SendMessage(msg);

            } else if (e.Key == Key.Right)
            {
                string msg = Protocol.MOVERIGHT;
                res = _client.SendMessage(msg);
                
            }
            if (res == -1) ConnectionInfoText.Text = "Server offline.";
        }
    }
}
