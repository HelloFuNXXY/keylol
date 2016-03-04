﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CsQuery;
using CsQuery.Output;
using Ganss.XSS;
using Keylol.Utilities;

namespace Keylol.Controllers.Article
{
    [Authorize]
    [RoutePrefix("article")]
    public partial class ArticleController : KeylolApiController
    {
        private static async Task SanitizeArticle(Models.Article article, bool extractUnstyledContent,
            bool proxyExternalImages)
        {
            Config.HtmlEncoder = new HtmlEncoderMinimum();
            var sanitizer =
                new HtmlSanitizer(
                    new[]
                    {
                        "br", "span", "a", "img", "b", "strong", "i", "strike", "u", "p", "blockquote", "h1", "hr",
                        "comment", "spoiler", "table", "colgroup", "col", "thead", "tr", "th", "tbody", "td"
                    },
                    null,
                    new[] {"src", "alt", "width", "height", "data-non-image", "href", "style"},
                    null,
                    new[] {"text-align"});
            var dom = CQ.Create(sanitizer.Sanitize(article.Content));
            article.ThumbnailImage = string.Empty;
            foreach (var img in dom["img"])
            {
                var url = string.Empty;
                if (string.IsNullOrEmpty(img.Attributes["src"]))
                {
                    img.Remove();
                }
                else
                {
                    var fileName = Upyun.ExtractFileName(img.Attributes["src"]);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        url = img.Attributes["src"];
                        if (proxyExternalImages)
                        {
                            var request = WebRequest.CreateHttp(url);
                            request.Referer = url;
                            request.UserAgent =
                                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36";
                            request.Accept = "image/webp,image/*,*/*;q=0.8";
                            request.Headers["Accept-Language"] = "en-US,en;q=0.8,zh-CN;q=0.6,zh;q=0.4";
                            try
                            {
                                using (var response = await request.GetResponseAsync())
                                using (var ms =
                                    new MemoryStream(response.ContentLength > 0 ? (int) response.ContentLength : 0))
                                {
                                    var responseStream = response.GetResponseStream();
                                    if (responseStream != null)
                                    {
                                        await responseStream.CopyToAsync(ms);
                                        var fileData = ms.ToArray();
                                        if (fileData.Length > 0)
                                        {
                                            var uri = new Uri(url);
                                            var extension = Path.GetExtension(uri.AbsolutePath);
                                            if (!string.IsNullOrEmpty(extension))
                                            {
                                                var name = await Upyun.UploadFile(fileData, extension);
                                                if (!string.IsNullOrEmpty(name))
                                                {
                                                    url = $"keylol://{name}";
                                                    img.Attributes["article-image-src"] = url;
                                                    img.RemoveAttribute("src");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (WebException)
                            {
                            }
                        }
                    }
                    else
                    {
                        url = $"keylol://{fileName}";
                        img.Attributes["article-image-src"] = url;
                        img.RemoveAttribute("src");
                    }
                }
                if (string.IsNullOrEmpty(article.ThumbnailImage))
                    article.ThumbnailImage = url;
            }
            article.Content = dom.Render();
            if (extractUnstyledContent)
                article.UnstyledContent = dom.Render(OutputFormatters.PlainText);
        }
    }
}