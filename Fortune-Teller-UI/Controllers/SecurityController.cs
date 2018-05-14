using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FortuneTeller.Views
{
    public class SecurityController : Controller
    {
        public IActionResult Index()
        {
            var results = new List<string>();
            for (double s = 0; s < 1; s += .02d)
            {
                //results.Add(_utils.HexColorFromDouble(s));
            }
            return View(results);
        }
    }
}