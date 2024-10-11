namespace OpenDotaAutoParser.Services;

public abstract class AbstractDotaApiService {
    public abstract List<long> GetUnparsedMatches(long playerId);
    public abstract void ParseMatch(long matchId);
}