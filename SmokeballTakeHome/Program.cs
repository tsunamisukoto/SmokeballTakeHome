using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;

class Program
{
    static readonly HttpClient client = new HttpClient();

    static async Task Main(string[] args)
    {
        await ProcessRequest(args);
    }
    const int GOOGLE_API_RESPONSE_PAGE_SIZE = 10;
    private static async Task ProcessRequest(string[] args)
    {

        if (!args.Any())
        {
            Console.WriteLine("No target site provided;");
        }
        string targetSite = args.Take(1).First();  // The website you want to check the rank for. Note because of the above check we can just do first on this.
        var keywords = args.Skip(1).ToArray();
        if (!keywords.Any())
        {
            Console.WriteLine("No keywords provided;");
        }
        string query = string.Join(" ", keywords);

        int rank = 0;

        try
        {
            for (int page = 0; page < 10; page++)
            {
                var searchResult = await MakeApiCall(query, page);

                if (searchResult?.Items != null)
                {
                    var i = 0;
                    foreach (var item in searchResult.Items)
                    {
                        if (item.Link.Contains(targetSite))
                        {
                            rank = (page * GOOGLE_API_RESPONSE_PAGE_SIZE) + i;
                            Console.WriteLine($"Rank: {rank} contains {query}");
                        }
                        i++;
                    }
                }
            }

            Console.WriteLine("Rank: Past 100");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }

    private static async Task<GoogleResponseModel?> MakeApiCall(string query, int page)
    {

        string apiKey = "AIzaSyAovwxQgZSNaiErxkaYCf_WjMEn8nKkyz0"; // Note: in a proper system this would come from a keystore or something
        string cx = "f7c4264cb7f034af1"; // Same with this. This is just a simple key I set up for this demo, it would be stored in a store or whatever.
        string uri = $"https://www.googleapis.com/customsearch/v1?key={apiKey}&cx={cx}&q={HttpUtility.UrlEncode(query)}&start={page * GOOGLE_API_RESPONSE_PAGE_SIZE}";
        HttpResponseMessage response = await client.GetAsync(uri);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();

        var searchResult = JsonConvert.DeserializeObject<GoogleResponseModel>(responseBody);
        return searchResult;
    }

    public class GoogleResponseModel
    {
        public GoogleResponseItemModel[] Items { get; set; }
    }
    public class GoogleResponseItemModel
    {
        public string Link { get; set; }
    }
}