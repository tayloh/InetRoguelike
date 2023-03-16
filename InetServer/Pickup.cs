using System;


namespace InetServer
{
    /// <summary>
    /// Author: Erik Stolpe (estolpe@kth.se)
    /// 
    /// A class for representing a pickup tile
    /// on the game grid in the game logic.
    /// Contains functionality for pickups.
    /// </summary>
    
    public class Pickup : Tile
    {
        public readonly string ItemName;
        public readonly int ItemId;

        private int _minDamage;
        private int _maxDamage;

        public Pickup(string name, int minDamage, int maxDamage, int id)
        {
            ItemName = name;
            _minDamage = minDamage;
            _maxDamage = maxDamage;
            ItemId = id;
        }

        /// <summary>
        /// Gets the damage dealt with this pickup.
        /// </summary>
        /// <returns>Int representing damage done.</returns>
        public int DoDamage()
        {
            return new Random().Next(_minDamage, _maxDamage + 1);
        }

        public override string ToString()
        {
            return "U" + ItemId;
        }
    }
}
