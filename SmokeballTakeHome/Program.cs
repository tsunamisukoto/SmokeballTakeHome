using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Web;

partial class Program
{
    static readonly HttpClient client = new HttpClient();

    static async Task Main(string[] args)
    {
        await ProcessRequest(args);
    }
    const int GOOGLE_API_RESPONSE_PAGE_SIZE = 10;
    private static async Task ProcessRequest(string[] args)
    {
        // Validating inputs. Note, in a proper scenario (not a console app haha) you'd either throw an arg exception, or have proper error handling for the validation. Because this is a console app I'm playing a bit fast an loose with that.
        // Also, note, in the above case, I would pull this to a seperate injectable validator piece/method.
        if (!args.Any())
        {
            Console.WriteLine("No target site provided;");
            return;
        }
        string targetSite = args.Take(1).First();  // The website you want to check the rank for.
        if (!Uri.TryCreate(targetSite, UriKind.RelativeOrAbsolute, out var uri))
        {
            Console.WriteLine("Target site is expected to be a valid url.");
            return;
        }
        var keywords = args.Skip(1).ToArray();
        if (!keywords.Any())
        {
            Console.WriteLine("No keywords provided;");
            return;
        }


        var query = string.Join(" ", keywords);
        var ranksContainingTargetSite = await GetRanksContainingTargetSite(targetSite, query); // Note, this piece would probably be injected.

        if (!ranksContainingTargetSite.Any())
        {
            Console.WriteLine($"No search results for the query '{query}' contained '{targetSite}'");
        }
        else
        {
            Console.WriteLine($"The following search ranks for the query '{query}' contained '{targetSite}'");
            Console.WriteLine(string.Join(", ", ranksContainingTargetSite));
        }
    }

    private static async Task<List<int>> GetRanksContainingTargetSite(string targetSite, string query)
    {
        var ranksContainingTargetSite = new List<int>();
        try
        {
            for (int page = 0; page < 10; page++)
            {
                var searchResult = await GetResultsPageForQuery(query, page);

                if (searchResult?.Items != null)
                {
                    var i = 0;
                    foreach (var item in searchResult.Items)
                    {
                        if (item.Link.Contains(targetSite))
                        {
                            var rank = (page * GOOGLE_API_RESPONSE_PAGE_SIZE) + i;
                            ranksContainingTargetSite.Add(rank);
                        }
                        i++;
                    }
                }
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Exception caught hitting the api end point. Message :{0} ", e.Message);
        }

        return ranksContainingTargetSite;
    }

    private static async Task<GoogleResponseModel?> GetResultsPageForQuery(string query, int page)
    {
        string apiKey = ""; // Note: in a proper system this would come from a keystore or something I have provided Eileen with the api key I used for this piece.
        string cx = "f7c4264cb7f034af1"; // Same with this. This is just a simple key I set up for this demo, it would be stored in a store or whatever.
        string uri = $"https://www.googleapis.com/customsearch/v1?key={apiKey}&cx={cx}&q={HttpUtility.UrlEncode(query)}&start={page * GOOGLE_API_RESPONSE_PAGE_SIZE}";
        HttpResponseMessage response = await client.GetAsync(uri);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();

        var searchResult = JsonConvert.DeserializeObject<GoogleResponseModel>(responseBody);
        return searchResult;
    }
}