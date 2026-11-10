namespace CSharpUnionTypes;

// A hand-written union that does not box, taken from the "Non-boxing access pattern" example in the
// Microsoft Learn union language reference:
// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/union
// Pattern matching calls TryGetValue(out T) instead of reading Value, so int and bool stay unboxed.
[System.Runtime.CompilerServices.Union]
public struct IntOrBool : System.Runtime.CompilerServices.IUnion
{
    private readonly int _intValue;
    private readonly bool _boolValue;
    private readonly byte _tag; // 0 = none, 1 = int, 2 = bool

    public IntOrBool(int? value)
    {
        if (value.HasValue)
        {
            _intValue = value.Value;
            _tag = 1;
        }
    }

    public IntOrBool(bool? value)
    {
        if (value.HasValue)
        {
            _boolValue = value.Value;
            _tag = 2;
        }
    }

    public object? Value => _tag switch
    {
        1 => _intValue,
        2 => _boolValue,
        _ => null
    };

    public bool HasValue => _tag != 0;

    public bool TryGetValue(out int value)
    {
        value = _intValue;
        return _tag == 1;
    }

    public bool TryGetValue(out bool value)
    {
        value = _boolValue;
        return _tag == 2;
    }
}
