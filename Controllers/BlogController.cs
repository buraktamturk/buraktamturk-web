
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.AspNet.Mvc;
using System.Threading.Tasks;
using System.Data.Entity;
using org.buraktamturk.web.Models;
using Microsoft.Net.Http.Headers;

namespace org.buraktamturk.web.Controllers {

	public class AuthorPost {
		public posts post { get; set; }

		public authors author { get; set; }
	}

	public class PostModel {
		public AuthorPost post { get; set; }

		public AuthorPost first { get; set; }

		public List<AuthorPost> other { get; set; }
	}

	public class BlogController : Controller {
		DatabaseContext db;

		public BlogController(DatabaseContext db) {
			this.db = db;
		}

		[HttpGet("/")]
		public async Task<ActionResult> Index() {
			return View("Home", await db.posts.OrderByDescending(a => a.id).Where(a => a.lang == "EN" && a.show == true && a.active == true).GroupBy(a => a.id).Select(a => a.OrderBy(b => b.version)).ToListAsync());
		}

		[HttpGet("/atom.xml")]
		public async Task<ActionResult> atom() {
			ViewData.Model = await db.posts.Where(a => a.lang == "EN" && a.show == true && a.active == true).OrderByDescending(a => a.id).Join(db.authors, a => a.author, a => a.id, (pos, author) => new AuthorPost { post = pos, author = author }).GroupBy(a => a.post.id).Select(a => a.OrderBy(b => b.post.version)).ToListAsync();

      var v = View("atom");
      v.ContentType = new MediaTypeHeaderValue("application/atom+xml");

      return v;
		}

		/*
		[HttpGet("/sitemap.xml")]
		public async Task<ActionResult> sitemap() {
			ViewData.Model = await db.posts.Where(a => a.lang == "EN" && a.show == true && a.active == true).OrderByDescending(a => a.id).Join(db.authors, a => a.author, a => a.id, (pos, author) => new AuthorPost { post = pos, author = author }).GroupBy(a => a.post.id).Select(a => a.OrderBy(b => b.post.version)).ToListAsync();

			return new ContentTypedViewResult() {
				ViewName = "sitemap",
				ViewData = ViewData,
                TempData = TempData,
				ContentType = new MediaTypeHeaderValue("application/xml")
			};
		}
		*/

            /*

		[HttpPut("/{*path}.html/{state}")]
		[HttpDelete("/{*path}.html/{state}")]
		public async Task<JsonResult> changeShowStatePost(authors Author, string path, string title, int rev, string hl, string state) {
			posts post = await db.posts.FirstOrDefaultAsync(a => a.path == path && a.version == rev && a.lang == hl.ToUpper());
			bool val = Context.Request.Method == "PUT" ? true : false;

			if(state == "show") {
				post.show = val;
			} else if(state == "active") {
				post.active = val;
			} else {
				throw new Exception("Invalid state");
			}

			await db.SaveChangesAsync();
			return Json(new { success = true });
		}
        */

		[HttpPut("/{path}.html")]
        [HttpPut("/{path1}/{path}.html")]
        [HttpPut("/{path1}/{path2}/{path}.html")]
        [HttpPut("/{path1}/{path2}/{path3}/{path}.html")]
        [HttpPut("/{path1}/{path2}/{path3}/{path4}/{path}.html")]
        public async Task<JsonResult> putPost(authors Author, string path1, string path2, string path3, string path4, string path, string title, int rev, string hl, bool? show, bool? active) {
			posts Post = new posts();

            string pathlast = string.Join("/", new string[]
            {
                            path1, path2, path3, path4, path
            }.Where(a => a != null));

            posts oldpost = await db.posts.FirstOrDefaultAsync(a => a.path == pathlast && a.version == rev && a.lang == hl.ToUpper());
			if(oldpost != null) {
				Post = oldpost;
			} else {
				oldpost = await db.posts.OrderByDescending(a => a.version).FirstOrDefaultAsync(a => a.path == path);

				if(oldpost != null) {
					Post.id = oldpost.id;
				} else {
					Post.id = (await db.posts.OrderByDescending(a => a.id).FirstOrDefaultAsync()).id + 1;
				}
			}

			if(title != null)
				Post.title = title;

			if(oldpost != Post) {
				Post.created_at = DateTime.Now;
				Post.version = rev;
				Post.lang = hl.ToUpper();
				Post.path = pathlast;
				Post.author = Author.id;
			}

			if(show != null)
				Post.show = show.Value;

			if(active != null)
				Post.active = active.Value;

			Post.data = await new StreamReader(Request.Body, Encoding.UTF8).ReadToEndAsync();

			if(oldpost != Post)
				db.posts.Add(Post);

			await db.SaveChangesAsync();

			return Json(new { success = true });
		}

		[HttpGet("/{path}.html")]
        [HttpGet("/{path1}/{path}.html")]
        [HttpGet("/{path1}/{path2}/{path}.html")]
        [HttpGet("/{path1}/{path2}/{path3}/{path}.html")]
        [HttpGet("/{path1}/{path2}/{path3}/{path4}/{path}.html")]
        public async Task<ActionResult> getPost(string path1, string path2, string path3, string path4, string path) {

            string pathlast = string.Join("/", new string[]
            {
                path1, path2, path3, path4, path
            }.Where(a => a != null));

			var post = db.posts.Where(a => a.path == pathlast).OrderByDescending(a => a.version).First();
			var model = await db.posts
				.OrderByDescending(a => a.version)
				.Where(a => a.id == post.id && a.active == true)
				.Join(db.authors, a => a.author, a => a.id, (pos, author) => new AuthorPost { post = pos, author = author })
				.GroupBy(a => a.post.id)
				.Select(a => new PostModel() {
					// current post
					post = a.Where(b => b.post.lang == post.lang).OrderByDescending(b => b.post.version).FirstOrDefault(),

				    // first post
				    first = a.Where(b => b.post.lang == post.lang).FirstOrDefault(),

				    // other languages (Turkish, English etc.)
				    other = a.GroupBy(b => b.post.lang).Select(b => b.OrderByDescending(c => c.post.version).FirstOrDefault()).ToList()
				 })
				 .FirstOrDefaultAsync();

			if(model == null) {
				return HttpNotFound();
			}

			var lastModified = Request.Headers["If-Modified-Since"];
			if (lastModified != null) {
            	var modifiedSince = DateTime.Parse(lastModified);
            	if (modifiedSince >= model.post.post.created_at.ToUniversalTime()) {
                	return new HttpStatusCodeResult(304);
            	}
        	}

			Response.Headers.Add("Last-Modified", new String[] {model.post.post.created_at.ToUniversalTime().ToString("R")});

			if(model.post.post.path != pathlast) {
				return Redirect("/" + model.post.post.path + ".html");
			}

			return View("Post", model);
		}
	}
}
