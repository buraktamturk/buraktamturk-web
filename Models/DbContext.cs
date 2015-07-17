using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace org.buraktamturk.web.Models
{
	public class author {
		[Key]
		public int id { get; set; }

		public string name { get; set; }

		public string mail { get; set; }

		public string password { get; set; }
	}

	public class post {
		[Key]
		public int id { get; set; }

		public bool show { get; set; }

		public bool active { get; set; }

		public int author_id { get; set; }

		public DateTime created_at { get; set; }

		[ForeignKey("post_id")]
		public virtual IQueryable<revision> revisions { get; set; }

		[ForeignKey("author_id")]
		public virtual author author { get; set; }
  }

	public class revision {

		[Key]
		public int id { get; set; }

		public int post_id { get; set;  }

    public string lang { get; set; }

		public int author_id { get; set; }

		public string title { get; set; }

		public string path { get; set; }

		public string data { get; set; }

		public DateTime created_at { get; set; }

		public bool active { get; set; }

		[ForeignKey("post_id")]
		public virtual post post { get; set; }

		[ForeignKey("author_id")]
		public virtual author author { get; set; }

		public string summary() {
			Match m = Regex.Match(data, @"<p>\s*(.+?)\s*</p>");
      if (m.Success)
      {
          return m.Groups[1].Value;
      }
      else
      {
          return data;
      }
		}
	}

	public class DatabaseContext : DbContext
	{
		public DbSet<author> authors { get; set; }

		public DbSet<post> posts { get; set; }

		public DbSet<revision> revision { get; set; }

    public DatabaseContext() : base(Startup.config.Get("db")) {

    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder) {
        modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
    }
	}
}
