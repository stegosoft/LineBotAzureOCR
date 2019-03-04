using LinebotAzureOCRGit.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LinebotAzureOCRGit
{
    public class AzureOCRService
    {
        public async Task<string> getOCR(string ImageUrl)
        {
            var client = new HttpClient();
            var uriBase = "https://eastasia.api.cognitive.microsoft.com/vision/v1.0/ocr?";
            var subscriptionKey = "!!!改成你的subscriptionKey!!!";

            client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);


            var ocrUrl = new OCRUrl()
            {
                url = ImageUrl
            };
            var json = JsonConvert.SerializeObject(ocrUrl);

            var content = new StringContent(json, Encoding.UTF8, "application/json");


            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request parameters
            queryString["language"] = "zh-Hant";
            queryString["detectOrientation "] = "true";
            var uri = uriBase + queryString;

            HttpResponseMessage response;

            response = await client.PostAsync(uri, content);

            return await response.Content.ReadAsStringAsync();
        }

        public string GetOCRResponseString(string str)
        {
            OCRResponseJson ocr = JsonConvert.DeserializeObject<OCRResponseJson>(str);
            var result = "";
            foreach (var region in ocr.regions)
            {
                foreach (var line in region.lines)
                {
                    string strline = "";
                    foreach (var word in line.words)
                    {
                        strline += word.text;
                    }
                    result += strline + "\n";
                }
            }
            if (result == "")
                result = "無法辨識";
            return result;
        }
    }
}