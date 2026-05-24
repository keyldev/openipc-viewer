// Polyfill for C# record init-only setters on netstandard2.1.
// The compiler emits references to IsExternalInit; supplying an internal stub
// is the standard workaround when targeting older TFMs with modern C#.
namespace System.Runtime.CompilerServices;

internal static class IsExternalInit;
