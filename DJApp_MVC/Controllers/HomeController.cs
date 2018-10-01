using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DJApp_MVC.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            string usersID = User.Identity.GetUserId();
            using (RelayDJDevEntities db = new RelayDJDevEntities())
            {
                Dj dj = db.Djs.Where(x => x.DjUserId == usersID).FirstOrDefault();
                //string userName = await GetUsersSpotifyUserName();
                if (dj != null)
                {
                    //set location of playlist
                    return View();
                }
                else
                {
                    return RedirectToAction("RegisterDJ", "Account", new { id = usersID });
                }
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}