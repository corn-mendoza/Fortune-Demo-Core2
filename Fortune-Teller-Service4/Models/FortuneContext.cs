

using FortuneTeller.Models;
using System.Collections.Generic;

namespace FortuneTellerService4.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class FortuneContext 
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="FortuneContext"/> class.
        /// </summary>
        /// <param name="dbset">The dbset.</param>
        public FortuneContext(Dictionary<int, Fortune> dbset) 
        {
            Fortunes = dbset;
        }
        /// <summary>
        /// Gets the fortunes.
        /// </summary>
        /// <value>
        /// The fortunes.
        /// </value>
        public Dictionary<int, Fortune> Fortunes { get; private set; }
    }
}
