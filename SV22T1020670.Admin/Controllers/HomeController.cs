using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020670.Admin.Models;
using SV22T1020670.DataLayers;
using SV22T1020670.Admin;
using System.Collections.Generic;
using System.Diagnostics;


namespace SV22T1020670.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }

}
