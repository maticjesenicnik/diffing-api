namespace DiffingApi.Core;

/// <summary>
/// Describes the side of the diff we store the byte array in
/// </summary>
public enum DiffSide
{
    /// <summary> Left side to store the byte array </summary>
    Left,
    /// <summary> Right side to store the byte array </summary>
    Right
}