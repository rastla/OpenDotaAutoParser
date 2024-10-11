using OpenDotaDotNet;
using OpenDotaDotNet.Models.Players;

namespace OpenDotaAutoParser.Services;

public class OpenDotaService : AbstractDotaApiService {
    private readonly OpenDotaApi _openDotaApi = new();
    
    public override List<long> GetUnparsedMatches(long playerId) {
        Console.WriteLine($"[OpenDota] Fetching unparsed matches for player {playerId}");
        var matchDetails = _openDotaApi.Players
            .GetPlayerMatchesAsync(playerId,
                new PlayerEndpointParameters(){ 
                    Date = 30, 
                    Significant = 0, 
                    Project = ["version"], 
                }).Result;
        
        List<long> unparsedMatches = new();
        foreach (var match in matchDetails) {
            if (match.Version == null) {
                unparsedMatches.Add(match.MatchId);
            }
        }

        Console.WriteLine($"[OpenDota] Found {unparsedMatches.Count} matches");
        return unparsedMatches;
    }

    public override void ParseMatch(long matchId) {
        Console.WriteLine($"[OpenDota] Requesting parse of match {matchId}");
        var result = _openDotaApi.Request.SubmitNewParseRequestAsync(matchId).Result;
        Console.WriteLine($"[OpenDota] Success: Received job id {result.Job.JobId}");
    }
}