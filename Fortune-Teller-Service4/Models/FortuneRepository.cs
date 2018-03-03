using FortuneTeller.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FortuneTellerService4.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="FortuneTellerService4.Models.IFortuneRepository" />
    public class FortuneRepository : IFortuneRepository
    {
        private FortuneContext _db;
        Random _random = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="FortuneRepository"/> class.
        /// </summary>
        /// <param name="db">The database.</param>
        public FortuneRepository(FortuneContext db)
        {
            _db = db;
        }
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Fortune> GetAll()
        {
            return _db.Fortunes.Values.AsEnumerable();
        }

        /// <summary>
        /// Randoms the fortune.
        /// </summary>
        /// <returns></returns>
        public Fortune RandomFortune()
        {
            int count = _db.Fortunes.Count();
            var index = _random.Next() % count;
            return GetAll().ElementAt(index);
        }
    }
}
