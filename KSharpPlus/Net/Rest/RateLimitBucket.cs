using System.Collections.Concurrent;

namespace KSharpPlus.Net.Rest;

internal class RateLimitBucket : IEquatable<RateLimitBucket> {
    internal RateLimitBucket(string hash, string guildId, string channelId, string webhookId) {
        Hash = hash;
        ChannelId = channelId;
        GuildId = guildId;
        WebhookId = webhookId;

        BucketId = GenerateBucketId(hash, guildId, channelId, webhookId);
        RouteHashes = new ConcurrentBag<string>();
    }
    
    #region Fields and Properties

    /// <summary>
    /// Gets the Id of the guild bucket.
    /// </summary>
    public string GuildId { get; internal set; }

    /// <summary>
    /// Gets the Id of the channel bucket.
    /// </summary>
    public string ChannelId { get; internal set; }

    /// <summary>
    /// Gets the ID of the webhook bucket.
    /// </summary>
    public string WebhookId { get; internal set; }

    /// <summary>
    /// Gets the Id of the ratelimit bucket.
    /// </summary>
    public volatile string BucketId;

    /// <summary>
    /// Gets or sets the ratelimit hash of this bucket.
    /// </summary>
    public string Hash {
        get => Volatile.Read(ref _hash);
        internal set {
            IsUnlimited = value.Contains(UnlimitedHash);

            if (BucketId != null && !BucketId.StartsWith(value)) {
                string id = GenerateBucketId(value, GuildId, ChannelId, WebhookId);
                BucketId = id;
                RouteHashes.Add(id);
            }

            Volatile.Write(ref _hash, value);
        }
    }

    internal string _hash;

    /// <summary>
    /// Gets the past route hashes associated with this bucket.
    /// </summary>
    public ConcurrentBag<string> RouteHashes { get; }

    /// <summary>
    /// Gets when this bucket was last called in a request.
    /// </summary>
    public DateTimeOffset LastAttemptAt { get; internal set; }

    /// <summary>
    /// Gets the number of uses left before pre-emptive rate limit is triggered.
    /// </summary>
    public int Remaining => _remaining;

    /// <summary>
    /// Gets the maximum number of uses within a single bucket.
    /// </summary>
    public int Maximum { get; set; }

    /// <summary>
    /// Gets the timestamp at which the rate limit resets.
    /// </summary>
    public DateTimeOffset Reset { get; internal set; }

    /// <summary>
    /// Gets the time interval to wait before the rate limit resets.
    /// </summary>
    public TimeSpan? ResetAfter { get; internal set; }

    internal DateTimeOffset ResetAfterOffset { get; set; }

    internal volatile int _remaining;

    /// <summary>
    /// Gets whether this bucket has it's ratelimit determined.
    /// <para>This will be <see langword="false" /> if the ratelimit is determined.</para>
    /// </summary>
    internal volatile bool IsUnlimited;

    /// <summary>
    /// If the initial request for this bucket that is determining  the rate limits is currently executing
    /// This is a int because booleans can't be accessed atomically
    /// 0 => False, all other values => True
    /// </summary>
    internal volatile int LimitTesting;

    /// <summary>
    /// Task to wait for the rate limit test to finish
    /// </summary>
    internal volatile Task LimitTestFinished;

    /// <summary>
    /// If the rate limits have been determined
    /// </summary>
    internal volatile bool LimitValid;

    /// <summary>
    /// Rate limit reset in ticks, UTC on the next response after the rate limit has been reset
    /// </summary>
    internal long NextReset;

    /// <summary>
    /// If the rate limit is currently being reset.
    /// This is a int because booleans can't be accessed atomically.
    /// 0 => False, all other values => True
    /// </summary>
    internal volatile int LimitResetting;

    const string UnlimitedHash = "unlimited";

    #endregion

    #region Methods

    /// <summary>
    /// Generates an ID for this request bucket.
    /// </summary>
    /// <param name="hash">Hash for this bucket.</param>
    /// <param name="guildId">Guild Id for this bucket.</param>
    /// <param name="channelId">Channel Id for this bucket.</param>
    /// <param name="webhookId">Webhook Id for this bucket.</param>
    /// <returns>Bucket Id.</returns>
    public static string GenerateBucketId(string hash, string guildId, string channelId, string webhookId) => $"{hash}:{guildId}:{channelId}:{webhookId}";

    public static string GenerateHashKey(RestRequestMethod method, string route) => $"{method}:{route}";

    public static string GenerateUnlimitedHash(RestRequestMethod method, string route) => $"{GenerateHashKey(method, route)}:{UnlimitedHash}";

    /// <summary>
    /// Sets remaining number of requests to the maximum when the ratelimit is reset
    /// </summary>
    /// <param name="now"></param>
    internal async Task TryResetLimitAsync(DateTimeOffset now) {
        if (ResetAfter.HasValue) ResetAfter = ResetAfterOffset - now;
        if (NextReset == 0) return;
        if (NextReset > now.UtcTicks) return;

        while (Interlocked.CompareExchange(ref LimitResetting, 1, 0) != 0) await Task.Yield();

        if (NextReset != 0) {
            _remaining = Maximum;
            NextReset = 0;
        }

        LimitResetting = 0;
    }

    internal void SetInitialValues(int max, int usesLeft, DateTimeOffset newReset) {
        Maximum = max;
        _remaining = usesLeft;
        NextReset = newReset.UtcTicks;

        LimitValid = true;
        LimitTestFinished = null!;
        LimitTesting = 0;
    }

    #endregion

    #region Utils

    /// <summary>
    /// Returns a string representation of this bucket.
    /// </summary>
    /// <returns>String representation of this bucket.</returns>
    public override string ToString() {
        string guildId = GuildId != string.Empty ? GuildId : "guild_id";
        string channelId = ChannelId != string.Empty ? ChannelId : "channel_id";
        string webhookId = WebhookId != string.Empty ? WebhookId : "webhook_id";

        return $"rate limit bucket [{Hash}:{guildId}:{channelId}:{webhookId}] [{Remaining}/{Maximum}] {(ResetAfter.HasValue ? ResetAfterOffset : Reset)}";
    }

    /// <summary>
    /// Checks whether this <see cref="RateLimitBucket" /> is equal to another object.
    /// </summary>
    /// <param name="obj">Object to compare to.</param>
    /// <returns>Whether the object is equal to this <see cref="RateLimitBucket" />.</returns>
    public override bool Equals(object? obj) => Equals(obj as RateLimitBucket);

    /// <summary>
    /// Checks whether this <see cref="RateLimitBucket" /> is equal to another <see cref="RateLimitBucket" />.
    /// </summary>
    /// <param name="e"><see cref="RateLimitBucket" /> to compare to.</param>
    /// <returns>Whether the <see cref="RateLimitBucket" /> is equal to this <see cref="RateLimitBucket" />.</returns>
    public bool Equals(RateLimitBucket? e) {
        if (e is null) return false;

        return ReferenceEquals(this, e) || BucketId == e.BucketId;
    }

    /// <summary>
    /// Gets the hash code for this <see cref="RateLimitBucket" />.
    /// </summary>
    /// <returns>The hash code for this <see cref="RateLimitBucket" />.</returns>
    public override int GetHashCode() => BucketId.GetHashCode();

    #endregion
}