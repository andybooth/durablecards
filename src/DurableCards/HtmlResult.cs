using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mime;

namespace DurableCards
{
    public class HtmlResult : ContentResult
    {
        public HtmlResult(string html)
        {
            StatusCode = (int)HttpStatusCode.OK;
            Content = html;
            ContentType = MediaTypeNames.Text.Html;
        }
    }
}
