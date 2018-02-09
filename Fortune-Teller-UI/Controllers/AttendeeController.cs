using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Workshop_UI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Pivotal.Helper;
using Pivotal.Utilities;

namespace Workshop_UI.Controllers
{
    public class AttendeeController : Controller
    {
        private readonly AttendeeContext _context;
        ILogger<AttendeeController> _logger;
        private IConfiguration Config { get; set; }


        public AttendeeController(ILogger<AttendeeController> logger, IConfiguration config, AttendeeContext context)
        {
            _context = context;
            _logger = logger;
            Config = config;
        }

        // GET: AttendeeModels
        public async Task<IActionResult> Index()
        {
            var _dbstring = Config.GetConnectionString("AttendeeContext");
            ViewData["ConnectSource"] = "appsettings.json";
            IConfigurationSection configurationSection = Config.GetSection("ConnectionStrings");
            if (configurationSection != null)
            {
                if (configurationSection.GetValue<string>("AttendeeContext") != null)
                    ViewData["ConnectSource"] = "Config Server";
            }

            var cfe = new CFEnvironmentVariables();
            var _connect = cfe.getConnectionStringForDbService("user-provided", "AttendeeContext");
            if (!string.IsNullOrEmpty(_connect))
            {
                ViewData["ConnectSource"] = "User Provided Service";
                _dbstring = _connect;
            }

            ViewData["ConnectionString"] = StringCleaner.GetDisplayString("Password=", ";", _dbstring, "*****");
            return View(await _context.AttendeeModel.ToListAsync());
        }

        // GET: AttendeeModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendeeModel = await _context.AttendeeModel
                .SingleOrDefaultAsync(m => m.Id == id);
            if (attendeeModel == null)
            {
                return NotFound();
            }

            return View(attendeeModel);
        }

        // GET: AttendeeModels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AttendeeModels/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,Title,Department")] AttendeeModel attendeeModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(attendeeModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(attendeeModel);
        }

        // GET: AttendeeModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendeeModel = await _context.AttendeeModel.SingleOrDefaultAsync(m => m.Id == id);
            if (attendeeModel == null)
            {
                return NotFound();
            }
            return View(attendeeModel);
        }

        // POST: AttendeeModels/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Title,Department")] AttendeeModel attendeeModel)
        {
            if (id != attendeeModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(attendeeModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AttendeeModelExists(attendeeModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(attendeeModel);
        }

        // GET: AttendeeModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var attendeeModel = await _context.AttendeeModel
                .SingleOrDefaultAsync(m => m.Id == id);
            if (attendeeModel == null)
            {
                return NotFound();
            }

            return View(attendeeModel);
        }

        // POST: AttendeeModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attendeeModel = await _context.AttendeeModel.SingleOrDefaultAsync(m => m.Id == id);
            _context.AttendeeModel.Remove(attendeeModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AttendeeModelExists(int id)
        {
            return _context.AttendeeModel.Any(e => e.Id == id);
        }
    }
}
