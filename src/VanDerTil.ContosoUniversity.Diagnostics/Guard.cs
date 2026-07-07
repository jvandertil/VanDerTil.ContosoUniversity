using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace VanDerTil.ContosoUniversity.Diagnostics;

/// <summary>
/// Guard clauses for argument validation.
/// Fail fast with clear ArgumentException variants at method entry points.
/// </summary>
public static class Guard
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNull([NotNull] object? value, [CallerArgumentExpression(nameof(value))] string paramName = "")
        => ArgumentNullException.ThrowIfNull(value, paramName);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNullOrEmpty([NotNull] string? value, [CallerArgumentExpression(nameof(value))] string paramName = "")
        => ArgumentException.ThrowIfNullOrEmpty(value, paramName);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNullOrWhiteSpace([NotNull] string? value, [CallerArgumentExpression(nameof(value))] string paramName = "")
        => ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotEmpty<T>([NotNull] IEnumerable<T>? collection, [CallerArgumentExpression(nameof(collection))] string paramName = "")
    {
        ArgumentNullException.ThrowIfNull(collection, paramName);

        if (!collection.Any())
        {
            throw new ArgumentException($"{paramName} cannot be empty.", paramName);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InRange(int value, int min, int max, [CallerArgumentExpression(nameof(value))] string paramName = "")
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, min, paramName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, max, paramName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InRange(decimal value, decimal min, decimal max, [CallerArgumentExpression(nameof(value))] string paramName = "")
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, min, paramName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, max, paramName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GreaterThanZero(int value, [CallerArgumentExpression(nameof(value))] string paramName = "")
        => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, paramName);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GreaterThanOrEqualToZero(int value, [CallerArgumentExpression(nameof(value))] string paramName = "")
        => ArgumentOutOfRangeException.ThrowIfNegative(value, paramName);
}
