using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kazan_Session1_Mobile_14_9
{
    public class WebApi
    {
        string mainSite = "http://10.0.2.2:54165/";

        public async Task<string> PostAsync(string Data, string extSite)
        {
            var client = new HttpClient();
            var requestWeb = mainSite + extSite;
            var response = "";
            if (Data == null)
            {
                var emptyContent = new StringContent("", Encoding.UTF8, "application/json");
                response = await client.PostAsync(requestWeb, emptyContent).Result.Content.ReadAsStringAsync();
            }
            else
            {
                var jsonContent = new StringContent(Data, Encoding.UTF8, "application/json");
                response = await client.PostAsync(requestWeb, jsonContent).Result.Content.ReadAsStringAsync();
            }
            return response;
        }
    }
}
