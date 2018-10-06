using DJApp_MVC.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace DJApp_MVC.Controllers
{
    public class LocationsController : Controller
    {
        public ActionResult AddLocation()
        {
            return View();
        }

        public ActionResult AddLocationConfirm(Location model)
        {
            string cacheOneTitle = model.LocationName + ":spotifyplaylistid";
            HttpContext.Cache[cacheOneTitle] = model.SpotifyPlayListId;

            HttpCookie cookie = new HttpCookie("stalled_location_name");
            HttpContext.Response.Cookies.Remove("stalled_location_name");
            cookie.Value = model.LocationName;
            HttpContext.Response.SetCookie(cookie);

            using (RelayDJDevEntities db = new RelayDJDevEntities())
            {
                Location loc = new Location();
                loc.LocationId = Guid.NewGuid().ToString();
                loc.LocationName = model.LocationName;
                loc.SportifyUserName = model.SportifyUserName;
                loc.SpotifyPlayListId = model.SpotifyPlayListId;
                loc.LocationOwnerId = User.Identity.GetUserId();
                db.Locations.Add(loc);
                db.SaveChanges();
            }
            return RedirectToAction("LocationGetCode");
        }

        [HttpGet]
        public ActionResult LocationGetCode()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://accounts.spotify.com/authorize/");
            string scope = HttpUtility.UrlEncode("playlist-modify-private user-read-playback-state user-modify-playback-state");
            string urlParameters = "?client_id="+ WebConfigurationManager.AppSettings["SpotifyAPIClientID"] +"&response_type=code&scope=" + scope + "&redirect_uri=http://whiskeyandtrust.azurewebsites.net/Locations/GetLocationToken/";

            string url = "https://accounts.spotify.com/authorize/" + urlParameters;
            // HttpResponseMessage response = client.GetAsync(urlParameters).Result;  
            return Redirect(url);
        }

        public async Task<ActionResult> GetLocationToken(string code)
        {
            if (!String.IsNullOrEmpty(code))
            {
                var result = await GainLocationToken(code);
                if (!string.IsNullOrEmpty(result))
                {

                    return RedirectToAction("SelectLocationDevice", new { token = result });
                }
                else
                {
                    return RedirectToAction("DeniedAccess");
                }
            }
            return View("Error");
        }

        public async Task<string> GainLocationToken(string code)
        {
            var spotifyClient = WebConfigurationManager.AppSettings["SpotifyAPIClientID"];
            var spotifySecret = WebConfigurationManager.AppSettings["SpotifyAPIClientSecretID"];

            HttpClient client = new HttpClient();
            var authHeader = Convert.ToBase64String(Encoding.Default.GetBytes($"{spotifyClient}:{spotifySecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            client.BaseAddress = new Uri("https://accounts.spotify.com/api/token");
            Dictionary<string, string> prams = new Dictionary<string, string>();
            prams.Add("grant_type", "authorization_code");
            prams.Add("code", code);
            prams.Add("redirect_uri", "http://whiskeyandtrust.azurewebsites.net/Locations/GetLocationToken/");

            HttpResponseMessage response = await client.PostAsync(client.BaseAddress.ToString(), new FormUrlEncodedContent(prams));
             var textResponse = await response.Content.ReadAsStringAsync(); ;

            string first = textResponse.ToString();
            string[] ara = first.Split(',');
            string change = ara[0].ToString();
            string[] aratwo = change.Split(':');
            string finalToken = aratwo[1].Replace("\"", "");

            string locationName = Request.Cookies["stalled_location_name"].Value.ToString();

            string cachedLocationToken = "";
            Location location = new Location();
            using (RelayDJDevEntities db = new RelayDJDevEntities())
            {
                string userId = User.Identity.GetUserId();
                location = db.Locations.Where(x => x.LocationOwnerId == userId).FirstOrDefault();
            }
            if (location != null)
            {
                cachedLocationToken = location.LocationId + ":token";
            }

            HttpContext.Cache[cachedLocationToken] = finalToken;

            return finalToken;
        }


        public async Task<ActionResult> SelectLocationDevice(string token)
        {
            var result = await GetLocationDevices(token);
            if (result != null)
            {
                return View(result);
            }
            else
            {
                return View("Error");
            }
        }


        public async Task<LocationDevices> GetLocationDevices(string token)
        {
            LocationDevices locationDevices = new LocationDevices();
            using (RelayDJDevEntities db = new RelayDJDevEntities())
            {
                string userId = User.Identity.GetUserId();
                locationDevices.LocationId = db.Locations.Where(x => x.LocationOwnerId == userId).SingleOrDefault().LocationId;
            }
            List<Devices> devices = new List<Devices>();
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.spotify.com/v1/me/player/devices");
            string phantomPram = "";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = client.GetAsync(phantomPram).Result;
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                string replacement = System.Text.RegularExpressions.Regex.Replace(result, @"\t|\n|\r|\\\\", "");
                JObject s = JObject.Parse(replacement);

                JArray sentDevices = (JArray)s["devices"];
                locationDevices.devices = sentDevices.ToObject<List<Devices>>();
            }
            return locationDevices;
        }

        public ActionResult LocationDeviceSelected(string deviceId, string locationId)
        {
            string locationName = Request.Cookies["stalled_location_name"].Value.ToString();
            string cachedLocationDevice = "";
            Location location = new Location();
            using (RelayDJDevEntities db = new RelayDJDevEntities())
            {
                location = db.Locations.Where(x => x.LocationId == locationId).FirstOrDefault();
            }
            if (location != null)
            {
                cachedLocationDevice = location.LocationId + ":device";
            }
            HttpContext.Cache[cachedLocationDevice] = deviceId;
            return RedirectToAction("Index", "Home");
        }


    }
}