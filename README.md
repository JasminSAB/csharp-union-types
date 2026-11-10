# C# 15 Union Types, Checked on the .NET 11 SDK

Source code for the dotnetleet.com post
[C# Union Types: The Compiler Finally Checks Every Case](https://dotnetleet.com/Blogs/CSharpUnionTypes).

A console application that turns every claim of the post into a check that prints `PASS` or `FAIL`.
The run-time claims live in `Program.cs`. The compile-time claims (a missing case, a discard arm, a
type that is not a case) are checked by `verify.ps1`, which builds the project four ways and reads the
real compiler output.

## Requirements

- .NET 11 SDK 11.0.100 or later ([download](https://dotnet.microsoft.com/download/dotnet/11.0))
- No `LangVersion` setting: C# 15 is the default language version for `net11.0`

## Run

```
dotnet run -c Release
```

Prove the compile-time claims as well (Windows PowerShell or PowerShell 7):

```
pwsh ./verify.ps1
```

Or build a single variant by hand:

```
dotnet build -p:GiftCard=true                      # a fourth case: warning CS8509
dotnet build -p:GiftCard=true -p:DiscardArm=true   # the same case hidden by a discard arm: no warning
dotnet build -p:WrongType=true                     # a string assigned to the union: error CS0029
```

## What the checks show

Output of `verify.ps1` on SDK 11.0.100, Windows x64:

```
[PASS] Three cases and an exhaustive switch build without warnings
       build succeeded, 0 warnings
[PASS] Adding GiftCard produces CS8509 naming the missing case
       warning CS8509: The switch expression does not handle all possible values of its input type
       (it is not exhaustive). For example, the pattern 'CSharpUnionTypes.GiftCard' is not covered.
[PASS] A discard arm hides the missing GiftCard case (no CS8509)
       build succeeded, no CS8509
[PASS] Assigning a string to PaymentMethod is a compile error
       error CS0029: Cannot implicitly convert type 'string' to 'CSharpUnionTypes.PaymentMethod'
[PASS] Case types convert implicitly and the switch without a default arm handles every case
[PASS] A type pattern tests the value inside the union
[PASS] A union declaration becomes a struct
[PASS] The case is stored in an object? Value property
[PASS] default(PaymentMethod) holds nothing
[PASS] A switch without a null arm throws on a default union
       SwitchExpressionException
[PASS] Value-type cases in a declared union are boxed
       1,000 ints stored in union Amount(int, decimal) allocated 24000 bytes
[PASS] A [Union] struct with TryGetValue is pattern matched without boxing
       1,000 matches of IntOrBool allocated 0 bytes
```

## Files

| File | What it shows |
|---|---|
| `PaymentMethod.cs` | `public union PaymentMethod(CreditCard, PayPal, BankTransfer);` and a union with value-type cases |
| `Checkout.cs` | The exhaustive switch with no default arm, plus the build variants used by `verify.ps1` |
| `IntOrBool.cs` | The non-boxing `[Union]` struct from the Microsoft Learn language reference |
| `Program.cs` | The run-time checks |
| `verify.ps1` | The compile-time checks and a full run |

## Official Microsoft documentation

- [What's new in C# 15](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15)
- [Union types, C# language reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/union)
- [Unions, C# feature specification](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-15.0/unions)
- [Explore union types in C# 15](https://devblogs.microsoft.com/dotnet/csharp-15-union-types/) (.NET Blog)
