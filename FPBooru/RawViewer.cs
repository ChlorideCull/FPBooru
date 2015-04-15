using Nancy;
using System.Collections.Generic;
using Nancy.ViewEngines;
using Nancy.Responses;

namespace Nancy.ViewEngines
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
			return new HtmlResponse(RetCode, ReturnContent);
		}

		protected void ReturnContent(System.IO.Stream stream) {
			byte[] chars = System.Text.Encoding.UTF8.GetBytes((string)Model);
			stream.Write(chars, 0, chars.Length);
		}
	}
}
