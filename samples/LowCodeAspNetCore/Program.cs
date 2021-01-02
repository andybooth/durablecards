using AdaptiveCards;
using AdaptiveCards.Templating;
using LowCode.AdaptiveCards;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

WebHost.CreateDefaultBuilder()
    .Configure(app => app
        .UseRouting()
        .UseDeveloperExceptionPage()
        .UseEndpoints(endpoints => endpoints.MapControllers()))
    .ConfigureServices(services => services.AddControllersWithViews())
    .Build()
    .Run();

[Route("/")]
public class CardController : Controller
{
    [HttpGet]
    public IActionResult Get(string data = "./Data/contacts.json", string template = "./Templates/questionnaire.json")
    {
        var cardTemplate = new AdaptiveCardTemplate(System.IO.File.ReadAllText(template));
        var json = JObject.Parse(System.IO.File.ReadAllText(data));
        var expanded = cardTemplate.Expand(json);
        var result = AdaptiveCard.FromJson(expanded);
        var renderer = new BootstrapCardRenderer();
        var output = renderer.RenderCard(result.Card);
        var html = output.Html.ToString();

        return View("Card", html);
    }

    [HttpPost]
    public IActionResult Post(IFormCollection data)
    {
        var jObject = new JObject();

        foreach (var property in data)
        {
            jObject.Add(new JProperty(property.Key, property.Value.Count == 1 ? property.Value[0] : property.Value.ToArray()));
        }
        
        return Content(jObject.ToString(), "application/json");
    }
}