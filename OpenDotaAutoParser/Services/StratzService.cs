using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using STRATZ;

namespace OpenDotaAutoParser.Services;

public class StratzService : AbstractDotaApiService {
    private readonly GraphQLHttpClient _graphQlHttpClient;

    public StratzService(string accessToken) {
        _graphQlHttpClient = new GraphQLHttpClient("https://api.stratz.com/graphql", new NewtonsoftJsonSerializer());
        _graphQlHttpClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"bearer {accessToken}");
    }
    
    public override List<long> GetUnparsedMatches(long playerId) {
        Console.WriteLine($"[Stratz] Fetching unparsed matches for player {playerId}");
        var query = new DotaQueryQueryBuilder()
            .WithPlayer(
                new PlayerTypeQueryBuilder()
                    .WithMatches(
                        new MatchTypeQueryBuilder()
                            .WithId()
                            .WithAnalysisOutcome(),
                        new PlayerMatchesRequestType() {
                            Take = 100,
                            StartDateTime = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeSeconds(),
                            IsParsed = false,
                        }
                    ),
                playerId
            )
            .Build(STRATZ.Formatting.Indented);

        var request = new GraphQLRequest() { Query = query };
        var response = _graphQlHttpClient.SendQueryAsync<PlayerTypeResponse>(request).Result;
        
        List<long> unparsedMatches = new();
        foreach (var match in response.Data.Player.Matches) {
            if (match.Id != null) {
                unparsedMatches.Add((long)match.Id);
            }
        }
        
        Console.WriteLine($"[Stratz] Found {unparsedMatches.Count} matches");
        return unparsedMatches;
    }

    public override void ParseMatch(long matchId) {
        Console.WriteLine($"[Stratz] Requesting parse of match {matchId}");
        var mutation = new DotaMutationQueryBuilder().WithRetryMatchDownload(matchId).Build(STRATZ.Formatting.Indented);
        var request = new GraphQLRequest() { Query = mutation };
        var result = _graphQlHttpClient.SendQueryAsync<RetryMatchDownloadResponse>(request).Result;
        Console.WriteLine($"[Stratz] Success: {result.Data.RetryMatchDownload}");
    }
}