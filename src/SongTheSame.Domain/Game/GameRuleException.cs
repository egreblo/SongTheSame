namespace SongTheSame.Domain.Game;

/// <summary>Thrown when a player action is not allowed in the current state of the game.</summary>
public sealed class GameRuleException(string message) : Exception(message);
