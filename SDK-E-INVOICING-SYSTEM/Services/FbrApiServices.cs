//using LiveCharts;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using static QRCoder.PayloadGenerator;

public class FbrApiService
{
    private readonly HttpClient _client;

    public FbrApiService()
    {
        _client = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        });
    }


    public async Task<string> PostInvoiceDataAsync(string jsonPayload, string sellerToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://gw.fbr.gov.pk/di_data/v1/di/postinvoicedata_sb");

            request.Headers.Add("Authorization", $"Bearer {sellerToken}");
            request.Headers.Add("Cookie", "key=value; JSESSIONID=6dh2TLgZ6MNrzrPMw2tQonVWS6CdgRt-MgoupnKM.i01-irisdmz55; cookiesession1=678B2A2ED92C48287169612504B199D0");

            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();

            return $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}\n" +
                   $"Content-Type: {response.Content.Headers.ContentType}\n\n" +
                   $"Response Body:\n{responseBody}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
    public async Task<string> ValidateInvoiceDataAsync(string jsonPayload, string sellerToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://gw.fbr.gov.pk/di_data/v1/di/validateinvoicedata_sb");

            request.Headers.Add("Authorization", $"Bearer {sellerToken}");
            request.Headers.Add("Cookie", "key=value; JSESSIONID=6dh2TLgZ6MNrzrPMw2tQonVWS6CdgRt-MgoupnKM.i01-irisdmz55; cookiesession1=678B2A2ED92C48287169612504B199D0");

            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _client.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();

            return $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}\n" +
                   $"Content-Type: {response.Content.Headers.ContentType}\n\n" +
                   $"Response Body:\n{responseBody}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

}   // is mai ak or changing krni jon sa seller selcy ho ga usi ka token ho ga mean har sleeler ka apna to0ken 