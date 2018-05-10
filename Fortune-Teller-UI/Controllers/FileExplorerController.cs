using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.JavaScript.DataVisualization.Models;
using Syncfusion.JavaScript.DataVisualization.Models.Diagram;
using Syncfusion.JavaScript.DataVisualization.DiagramEnums;
using System.Text.RegularExpressions;
using Syncfusion.JavaScript.DataVisualization.Models.Collections;
using Syncfusion.JavaScript;
using Microsoft.AspNetCore.Hosting;

namespace FortuneTeller.Controllers
{
    public partial class FileExplorerController : Controller
    {
	    public FileExplorerOperations operation;
        // GET: /<controller>/
        public ActionResult Index()
        {	
		return View();
        }
         public FileExplorerController(IHostingEnvironment hostingEnvironment)
        {
            this.operation = new FileExplorerOperations(hostingEnvironment.ContentRootPath);
        }
        public ActionResult Download(FileExplorerParams args)
        {
            return operation.Download(args.Path, args.Names);
        }
        public ActionResult Upload(FileExplorerParams args)
        {
            operation.Upload(args.FileUpload, args.Path);
            return Json("");            
        }
        public ActionResult GetImage(FileExplorerParams args)
        {
            return operation.GetImage(args.Path);
        }
        public ActionResult FileActionDefault([FromBody] FileExplorerParams args)
        {
                switch (args.ActionType)
                {
                    case "Read":
                        return Json(operation.Read(args.Path, args.ExtensionsAllow));
                    case "CreateFolder":
                        return Json(operation.CreateFolder(args.Path, args.Name));
                    case "Paste":
                        return Json(operation.Paste(args.LocationFrom, args.LocationTo, args.Names, args.Action, args.CommonFiles));
                    case "Remove":
                        return Json(operation.Remove(args.Names, args.Path, args.SelectedItems));
                    case "Rename":
                        return Json(operation.Rename(args.Path, args.Name, args.NewName, args.CommonFiles));
                    case "GetDetails":
                        return Json(operation.GetDetails(args.Path, args.Names));
                    case "Search":
                        return Json(operation.Search(args.Path, args.ExtensionsAllow, args.SearchString, args.CaseSensitive));
                }
                return Json("");
        }
    }    
}
