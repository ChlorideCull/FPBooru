using Nancy;
using System.Collections.Generic;
using Nancy.ViewEngines;
using Nancy.Responses;
using System.Security.Cryptography;
using System;

namespace FPBooru
{
	public class RawViewEngine : IViewEngine
	{
		public RawViewEngine() {
			this.Extensions = new[] { "rawhtml" };
		}

		public IEnumerable<string> Extensions { get; set; }

		public HttpStatusCode RetCode = (HttpStatusCode)200;

		public dynamic Model { get; set; }

		public void Initialize(ViewEngineStartupContext viewEngineStartupContext) { }

		public Response RenderView(ViewLocationResult viewLocationResult, dynamic model, IRenderContext renderContext) {
			Model = model;
			string etag;
			using (SHA1 gen = new SHA1Managed()) {
				etag = Convert.ToBase64String(gen.ComputeHash(System.Text.Encoding.UTF8.GetBytes((string)Model)));
			}
			HtmlResponse resp = new HtmlResponse(RetCode, ReturnContent);
			resp.Headers["ETag"] = etag;
			return resp;
		}

		protected void ReturnContent(System.IO.Stream stream) {
			byte[] chars = System.Text.Encoding.UTF8.GetBytes((string)Model);
			stream.Write(chars, 0, chars.Length);
		}
	}
}
