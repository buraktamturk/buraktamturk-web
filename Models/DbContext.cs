using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Text.RegularExpressions;

namespace org.buraktamturk.web.Models
{
	public class authors {
		[Key]
		public int id { get; set; }
		
		public string name { get; set; }
		
		public string mail { get; set; }
		
		public string password { get; set; }
	}
	
	public class posts {
		
		[Key]
		[Column(Order = 0)]
		public int id { get; set; }
		
		[Key]
		[Column(Order = 1)]
		public string lang { get; set; }
		
		[Key]
		[Column(Order = 2)]
		public int version { get; set; }
		
		public int author { get; set; }
		
		public string title { get; set; }
		
		public string path { get; set; }
		
		public string data { get; set; }
		
		public DateTime created_at { get; set; }
		
		public bool show { get; set; }
		
		public bool active { get; set; }
		
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
		public DbSet<authors> authors { get; set; }
		
		public DbSet<posts> posts { get; set; }
		
        public DatabaseContext() : base(Startup.config.Get("db")) {
            
        }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
	}
}