using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DJApp_MVC.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Text;
using Newtonsoft.Json;

namespace DJApp_MVC.Controllers
{
    [Authorize]
    public class DJ : Controller
    {
        public ActionResult SelectLocation()
        {
            List<Location> locs = new List<Location>();
            using (RelayDJDevEntities db = new RelayDJDevEntities())
            {
                locs = db.Locations.ToList();
            }
            return View(locs);
        }

        public ActionResult UserSelectedLocation(string locationId)
        {
            HttpCookie cookie = new HttpCookie("selected_location");
            HttpContext.Response.Cookies.Remove("selected_location");
            cookie.Value = locationId;
            HttpContext.Response.SetCookie(cookie);

            using (RelayDJDevEntities db = new RelayDJDevEntities())
            {
                List<RelayOrder> ro = db.RelayOrders.Where(x => x.Id == locationId).ToList();
                if (ro.Count > 0)
                {
                    // there are already people in the list you need to get the max count
                    RelayOrder addingAnotherUser = new RelayOrder()
                    {
                        Id = Guid.NewGuid().ToString(),
                        RelayDate = DateTime.Today,
                        UserId = User.Identity.GetUserId(),
                        EnteredLocationDate = DateTime.Now,
                        RelayOrder1 = ro.Max(x => x.RelayOrder1) + 1,
                        PlaylistId = db.Locations.Where(x => x.LocationId == locationId).SingleOrDefault().SpotifyPlayListId,
                        MaxPlays = 2,
                        Selected = 0,
                        NextInQue = false,
                        ProcessCount = 0,
                        LocationId = locationId
                    };

                    db.RelayOrders.Add(addingAnotherUser);

                }
                else
                {
                    //there is no one is the list right now this user is the first one
                    RelayOrder addFirstUser = new RelayOrder()
                    {
                        Id = Guid.NewGuid().ToString(),
                        RelayDate = DateTime.Today,
                        UserId = User.Identity.GetUserId(),
                        EnteredLocationDate = DateTime.Now,
                        RelayOrder1 = 0,
                        PlaylistId = db.Locations.Where(x => x.LocationId == locationId).SingleOrDefault().SpotifyPlayListId,
                        MaxPlays = 2,
                        Selected = 0,
                        NextInQue = false,
                        ProcessCount = 0,
                        LocationId = locationId
                    };

                    db.RelayOrders.Add(addFirstUser);
                }

                db.SaveChanges();

            }
            return RedirectToAction("SearchArtist");
        }

        public ActionResult SearchArtist()
        {
            Search start = new Search();
            return View(start);
        }

        public async Task<ActionResult> SongsByArtist(string Id)
        {
            List<Models.Songs> topSongs = new List<Models.Songs>();
            string token = Request.Cookies["spot_toke"].Value.ToString();
            if (!String.IsNullOrEmpty(token))
            {
                topSongs = await GetArtistsSongs(Id, token);
            }
            return View(topSongs);
        }

        public async Task<List<Models.Songs>> GetArtistsSongs(string Id, string token)
        {
            List<Models.Songs> topSongs = new List<Models.Songs>();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            string url = "https://api.spotify.com/v1/artists/" + Id + "/top-tracks";
            client.BaseAddress = new Uri(url);
            string urlParameters = "?country=US";
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                string replacement = System.Text.RegularExpressions.Regex.Replace(result, @"\t|\n|\r|\\\\", "");
                JObject s = JObject.Parse(replacement);

                JArray tracks = (JArray)s["tracks"];

                List<Album> albums = tracks.ToObject<List<Album>>();

                if (albums.Count > 0 && albums != null)
                {
                    foreach (var song in albums)
                    {
                        Models.Songs adding = new Models.Songs();
                        adding.SongTitle = song.name;
                        adding.SongSpotifyId = song.id;
                        StringBuilder arts = new StringBuilder();
                        int artsCount = song.artists.Count;
                        int counter = 0;
                        foreach (var artist in song.artists)
                        {
                            counter++;
                            arts.Append(artist.name);
                            if (artsCount == counter)
                            {

                            }
                            else
                            {
                                arts.Append(" / ");
                            }
                        }
                        adding.SongArtist = arts.ToString();
                        if (song.images != null && song.images.Count > 0)
                        {

                            string imageWanted = song.images[0].ToString();
                            Image imageing = JsonConvert.DeserializeObject<Image>(imageWanted);
                            adding.SongImageUrl = imageing.Url;
                        }

                        topSongs.Add(adding);
                    }
                }
            }

            return topSongs;
        }

        public ActionResult ConfirmPlay(string songId, string songTitle)
        {
            Songs song = new Songs();
            song.SongTitle = songTitle;
            song.SongSpotifyId = songId;
            return View(song);
        }


        public async Task<ActionResult> ConfirmedPlay(string id)
        {
            bool resultOfAdd = await AddToPlayList(id);
            if (resultOfAdd)
            {
                return RedirectToAction("SongAdded");
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        public async Task<bool> AddToPlayList(string id)
        {
            string spotifyplayListId = "";
            string locationId = "";
            using (RelayDJDevEntities db = new RelayDJDevEntities())
            {
                string userId = User.Identity.GetUserId();
                spotifyplayListId = db.RelayOrders.Where(x => x.UserId == userId).SingleOrDefault().PlaylistId;
                locationId = db.RelayOrders.Where(x => x.UserId == userId).SingleOrDefault().LocationId;
            }
            bool addedToPlaylist = false;
            string playlist = "";

            string userName = "";
            Location pulled = new Location();
            using (RelayDJDevEntities db = new RelayDJDevEntities())
            {
                pulled = db.Locations.Where(x => x.LocationId == locationId).FirstOrDefault();
            }
            if (!string.IsNullOrEmpty(pulled.SportifyUserName))
            {
                userName = pulled.SportifyUserName;
                playlist = pulled.SpotifyPlayListId;
            }

            string locationTokenPull = pulled.LocationId + ":token";
            string token = (string)HttpContext.Cache[locationTokenPull];

            if (!String.IsNullOrEmpty(token))
            {
                addedToPlaylist = await AddingToPlayList(playlist, userName, id, token);
            }
            return addedToPlaylist;
        }

        public async Task<bool> AddingToPlayList(string playlist, string userName, string id, string token)
        {
            string thing = "{\"uris\":[\"spotify:track:" + id + "\"]}";

            string url = "https://api.spotify.com/v1/users/" + userName + "/playlists/" + playlist + "/tracks";
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.BaseAddress = new Uri(url);
            var httpContent = new StringContent(thing, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(client.BaseAddress.ToString(), httpContent);
            if (response.IsSuccessStatusCode)
            {
                var check = await PlayerCheck(playlist, userName, token);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> PlayerCheck(string playlist, string userName, string token)
        {
            bool nothing = false;
            PlayerCheck playerCheck = new PlayerCheck();

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            string url = "https://api.spotify.com/v1/me/player";
            client.BaseAddress = new Uri(url);
            string urlParameters = "";
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                string replacement = System.Text.RegularExpressions.Regex.Replace(result, @"\t|\n|\r", "");
                JObject s = JObject.Parse(replacement);

                playerCheck = JsonConvert.DeserializeObject<PlayerCheck>(replacement);
                if (!playerCheck.is_playing)
                {
                    // need device id

                    string locationId = "";
                    using (RelayDJDevEntities db = new RelayDJDevEntities())
                    {
                        string userId = User.Identity.GetUserId();
                        locationId = db.RelayOrders.Where(x => x.UserId == userId).SingleOrDefault().LocationId;
                    }

                    Location pulled = new Location();
                    using (RelayDJDevEntities db = new RelayDJDevEntities())
                    {
                        pulled = db.Locations.Where(x => x.LocationId == locationId).FirstOrDefault();
                    }

                    string locationdevicePull = pulled.LocationId + ":device";
                    string deviceId = (string)HttpContext.Cache[locationdevicePull];


                    // we need to get the play list content

                    string thing = "{\"context_uri\":\"spotify:user:" + userName + ":playlist:" + playlist + "\",\"position_ms\":0}";

                    string urlpc = "https://api.spotify.com/v1/me/player/play" + "?device_id=" + deviceId;
                    HttpClient clientpc = new HttpClient();
                    clientpc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    clientpc.BaseAddress = new Uri(urlpc);
                    var httpContent = new StringContent(thing, Encoding.UTF8, "application/json");
                    HttpResponseMessage responsepc = await clientpc.PutAsync(clientpc.BaseAddress.ToString(), httpContent);
                    if (responsepc.IsSuccessStatusCode)
                    {
                        //play back has started.
                    }
                }
            }
            return nothing;
        }

        public ActionResult SongAdded()
        {
            return View();
        }

    }
}