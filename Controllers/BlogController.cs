
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
		public revision post { get; set; }
	}

	public class PostModel {
		public revision post { get; set; }

		public revision first { get; set; }

		public List<revision> other { get; set; }
	}

	public class BlogController : Controller {
		DatabaseContext db;

		public BlogController(DatabaseContext db) {
			this.db = db;
		}

		[HttpGet("/")]
		public async Task<ActionResult> Index() {
			return View("Home",
				await db.posts
					.Include(a => a.revisions.Select(b => b.author))
					.Where(a => a.active == true && a.show == true)
					.OrderByDescending(a => a.id)
					.Select(a => a.revisions.Where(b => b.active == true))
					.Select(a => a.Where(b => b.lang == "EN" && b.active == true))
				 	.ToListAsync()
			);
		}

		[HttpGet("/atom.xml")]
		public async Task<ActionResult> atom() {
      var v = View("atom", await db.posts
								.Include(a => a.revisions.Select(b => b.author))
								.Where(a => a.active == true && a.show == true)
								.OrderByDescending(a => a.id)
								.Select(a => a.revisions.Where(b => b.active == true))
								.Select(a => a.Where(b => b.lang == "EN" && b.active == true))
								.Where(a => a != null)
							 	.ToListAsync());

      v.ContentType = new MediaTypeHeaderValue("application/atom+xml");

      return v;
		}

		[HttpPut("/{path}.html")]
    [HttpPut("/{path1}/{path}.html")]
    [HttpPut("/{path1}/{path2}/{path}.html")]
    [HttpPut("/{path1}/{path2}/{path3}/{path}.html")]
    [HttpPut("/{path1}/{path2}/{path3}/{path4}/{path}.html")]
    public async Task<JsonResult> putPost(author Author, string path1, string path2, string path3, string path4, string path, string title, string hl, bool? active) {
      string pathlast = string.Join("/", new string[]
      {
        path1, path2, path3, path4, path
      }.Where(a => a != null));

      hl = hl ?? "EN";

      post post;
			post = await db.posts.FirstOrDefaultAsync(a => a.revisions.Any(b => b.path == pathlast && b.lang == hl.ToUpper()));
			if(post == null) {
        post = new post();
				db.posts.Add(post);
      }

      revision r = new revision();
			r.post = post;
      db.revision.Add(r);

			r.title = title;
			r.created_at = DateTime.Now;
			r.lang = hl.ToUpper();
			r.path = pathlast;
			r.author_id = Author.id;
			r.active = active.Value;

			r.data = await new StreamReader(Request.Body, Encoding.UTF8).ReadToEndAsync();

			await db.SaveChangesAsync();

			return Json(new { post_id = post.id, revision_id = r.id });
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

			var model = await db.posts
				.Where(a => a.active == true && a.revisions.Any(b => b.path == pathlast))
				.Select(a => a.revisions.Where(b => b.active == true))
				.Include(a => a.Select(b => b.author))
				.Select(a => new PostModel() {
					// current post
					post = a.Where(b => b.lang == "EN").OrderByDescending(b => b.id).FirstOrDefault(),

			    // first post
			    first = a.Where(b => b.lang == "EN").FirstOrDefault(),

			    // other languages (Turkish, English etc.)
			    other = a.GroupBy(b => b.lang).Select(b => b.OrderByDescending(c => c.id).FirstOrDefault()).ToList()
				 })
				 .FirstOrDefaultAsync();

			if(model == null) {
				return HttpNotFound();
			}

			var lastModified = Request.Headers["If-Modified-Since"];
			if (lastModified != null) {
            	var modifiedSince = DateTime.Parse(lastModified);
            	if (modifiedSince >= model.post.created_at.ToUniversalTime()) {
                	return new HttpStatusCodeResult(304);
            	}
        	}

			Response.Headers.Add("Last-Modified", new String[] {model.post.created_at.ToUniversalTime().ToString("R")});

			if(model.post.path != pathlast) {
				return Redirect("/" + model.post.path + ".html");
			}

			return View("Post", model);
		}
	}
}
