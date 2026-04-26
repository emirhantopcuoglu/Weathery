namespace Weathery.Helpers;

public static class UnixTime
{
    public static DateTime ToLocalDateTime(long unixSeconds)
        => DateTimeOffset.FromUnixTimeSeconds(unixSeconds).LocalDateTime;
}
