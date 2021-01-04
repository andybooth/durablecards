using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace DurableCards
{
    public static class HttpRequestExtensions
    {
        public static JObject ReadFormAsJObject(this HttpRequest httpRequest)
        {
            var jObject = new JObject();

            foreach (var formValue in httpRequest.Form)
            {
                if (formValue.Value.Count == 1)
                {
                    jObject.Add(formValue.Key, JToken.FromObject(formValue.Value.First().Trim()));
                }
                else
                {
                    jObject.Add(formValue.Key, JToken.FromObject(formValue.Value));
                }
            }

            return jObject;
        }
    }
}
