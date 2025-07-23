using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DevTools.Controllers
{
    public class GitIgnoreGeneratorController : Controller
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetTemplates()
        {
            var url = "https://api.github.com/repos/github/gitignore/contents/";
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0"); // GitHub API requires user-agent

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return Json(new List<string>());

            var content = JsonConvert.DeserializeObject<List<GithubContent>>(await response.Content.ReadAsStringAsync());
            var templates = new List<string>();
            foreach (var item in content)
            {
                if (item.Name.EndsWith(".gitignore"))
                    templates.Add(item.Name.Replace(".gitignore", ""));
            }
            templates.Sort();
            return Json(templates);
        }

        [HttpGet]
        public async Task<IActionResult> GetTemplateContent(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Template name is required.");

            var url = $"https://raw.githubusercontent.com/github/gitignore/main/{name}.gitignore";
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return NotFound();

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "text/plain");
        }

        public class GithubContent
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public string Sha { get; set; }
            public long Size { get; set; }
            public string Url { get; set; }
            public string Html_url { get; set; }
            public string Git_url { get; set; }
            public string Download_url { get; set; }
            public string Type { get; set; }
        }
    }
}
