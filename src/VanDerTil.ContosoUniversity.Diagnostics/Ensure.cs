using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace VanDerTil.ContosoUniversity.Diagnostics;

/// <summary>
/// Ensure assertions for internal state validation.
/// Used to verify assumptions within methods after arguments are validated.
/// These should never fail in correct code and indicate programming errors if they do.
/// </summary>
public static class Ensure
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void That([DoesNotReturnIf(false)] bool condition, string? message = null, [CallerArgumentExpression("condition")] string conditionExpr = "")
    {
        if (!condition)
        {
            throw new InvalidOperationException($"Expectation failed: {conditionExpr}{(message is not null ? $", {message}" : "")}");
        }
    }
}
