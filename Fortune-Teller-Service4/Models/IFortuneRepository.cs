using FortuneTeller.Models;
using System.Collections.Generic;

namespace FortuneTellerService4.Models
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFortuneRepository
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Fortune> GetAll();

        /// <summary>
        /// Randoms the fortune.
        /// </summary>
        /// <returns></returns>
        Fortune RandomFortune();
    }
}
