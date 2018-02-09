using FortuneTeller.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FortuneService.Client
{
    public interface IFortuneService
    {
        Task<List<Fortune>> AllFortunesAsync();
        Task<Fortune> RandomFortuneAsync();
    }
}
