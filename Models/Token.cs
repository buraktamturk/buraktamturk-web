
using System;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.ModelBinding;
using System.Threading;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Text.RegularExpressions;

namespace org.buraktamturk.web.Models {

	public class AuthorModelBinder : IModelBinder
  {
      public async Task<ModelBindingResult> BindModelAsync(ModelBindingContext bindingContext)
      {
				var requestServices = bindingContext.OperationBindingContext.HttpContext.RequestServices;
        DatabaseContext db = (DatabaseContext)requestServices.GetService(typeof(DatabaseContext));

        if (bindingContext.ModelType == typeof(authors))
        {
					var a = bindingContext.OperationBindingContext.HttpContext.Request.Headers.Get("Token");
					Match match = Regex.Match(a, @"user=(.*?);\s*password=(.*?);", RegexOptions.IgnoreCase);

					if(!match.Success) {
						throw new Exception("Yazar bulunamadı");
					}

					string user = match.Groups[1].Value, pass = match.Groups[2].Value;
					authors b = await db.authors.FirstAsync(c => c.mail == user && c.password == pass);

					return new ModelBindingResult(b, bindingContext.ModelName, true);
				}
				return null;
			}
	}
}
