using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DiscussionForum.Controllers
{
    public class PostController : Controller
    {
        private HttpClient httpClient;

        public PostController()
        {
            InitializeClient();
        }

        private void InitializeClient()
        {
            string baseUrl = ConfigurationManager.AppSettings["baseUrl"];

            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(baseUrl);
            httpClient.DefaultRequestHeaders.Accept.Clear();
        }

        [HttpGet]
        public ActionResult Create()
        {
            if(Session["token"] == null)
            {
                return RedirectToAction("login", "users");
            }
            return View();

        }

        [HttpPost, ValidateInput(false)]
        public async Task<ActionResult> Create(Post post)
        {
            post.CreatedTime = DateTime.Now;
            post.CreatedBy = Session["username"].ToString();
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Session["token"].ToString());
                using (HttpResponseMessage response = await httpClient.PostAsJsonAsync<Post>("api/Posts", post))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("index", "home");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return View(post);
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            IList<Post> postData = null;
            try
            {
                using (var response = await httpClient.GetAsync("api/Posts"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        postData = response.Content.ReadAsAsync<IList<Post>>().Result;
                    }
                    else
                    {
                        ModelState.AddModelError(String.Empty, "Try again after some time.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return View(postData);
        }

        [HttpGet]
        public async Task<ActionResult> View(int id)
        {
            Post post = null;
            try
            {
                using (var response = await httpClient.GetAsync("api/Posts/" + id.ToString()))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        post = response.Content.ReadAsAsync<Post>().Result;
                    }
                    else
                    {
                        ModelState.AddModelError(String.Empty, "Post not found");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return View(post);
        }
    }
}