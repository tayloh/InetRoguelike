using System;

namespace InetClient
{
    /// <summary>
    /// Author: Erik Stolpe (estolpe@kth.se)
    /// 
    /// An EventArgs child-class for storing recieve-event data.
    /// </summary>
    public class RecieveEventArgs : EventArgs
    {
        public string Data;
    }
}
