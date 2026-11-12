using SongTheSame.Domain.Game;

namespace SongTheSame.Domain.Leagues;

public sealed record LeagueMember(Guid UserId, DateTimeOffset JoinedAt);

/// <summary>A group of friends whose daily results are compared. Joined with an invite code.</summary>
public sealed class League
{
    public const int MaxMembers = 50;
    public const int MaxNameLength = 40;

    private readonly List<LeagueMember> _members = [];

    private League()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = "";

    public string InviteCode { get; private set; } = "";

    public Guid OwnerId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyList<LeagueMember> Members => _members;

    public static League Create(string name, Guid ownerId, string inviteCode, DateTimeOffset now)
    {
        var trimmed = name.Trim();
        if (trimmed.Length == 0) throw new ArgumentException("Name is required.", nameof(name));

        var league = new League
        {
            Id = Guid.NewGuid(),
            Name = trimmed.Length > MaxNameLength ? trimmed[..MaxNameLength] : trimmed,
            InviteCode = inviteCode,
            OwnerId = ownerId,
            CreatedAt = now,
        };
        league._members.Add(new LeagueMember(ownerId, now));
        return league;
    }

    public bool IsMember(Guid userId) => _members.Any(member => member.UserId == userId);

    /// <summary>Adds the player. Joining twice is harmless.</summary>
    public void Join(Guid userId, DateTimeOffset now)
    {
        if (IsMember(userId)) return;
        if (_members.Count >= MaxMembers) throw new GameRuleException("This league is full.");
        _members.Add(new LeagueMember(userId, now));
    }

    public void Leave(Guid userId)
    {
        if (userId == OwnerId) throw new GameRuleException("The owner cannot leave their own league.");
        _members.RemoveAll(member => member.UserId == userId);
    }
}
