// C# 15 union types: every claim from the blog post, checked at run time.
// The compile-time claims (CS8509, discard arm, wrong type) are checked by verify.ps1.
using System.Runtime.CompilerServices;
using CSharpUnionTypes;

var failed = 0;

void Check(string claim, bool ok, string detail)
{
    Console.WriteLine($"[{(ok ? "PASS" : "FAIL")}] {claim}");
    Console.WriteLine($"       {detail}");
    if (!ok) failed++;
}

Console.WriteLine($".NET {Environment.Version}, {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
Console.WriteLine();

// 1. Each case type converts implicitly to the union, and the exhaustive switch handles all three.
PaymentMethod[] payments =
[
    new CreditCard("4111", 2027),
    new PayPal("ana@example.com"),
    new BankTransfer("BA39 1290 0794 0102 8494"),
];
foreach (var payment in payments)
    Console.WriteLine($"       {Checkout.Describe(payment)}");
Check("Case types convert implicitly and the switch without a default arm handles every case",
    payments.Length == 3, "three payments described above");

// 2. The type pattern tests the value inside the union, not the union itself.
PaymentMethod paypal = new PayPal("ana@example.com");
Check("A type pattern tests the value inside the union",
    paypal is PayPal { Email: "ana@example.com" } && paypal.Value is PayPal,
    $"paypal.Value is {paypal.Value?.GetType().Name}");

// 3. The generated union is a struct.
Check("A union declaration becomes a struct",
    typeof(PaymentMethod).IsValueType,
    $"typeof(PaymentMethod).IsValueType = {typeof(PaymentMethod).IsValueType}");

// 4. It stores its case in a single object? Value property.
var valueProperty = typeof(PaymentMethod).GetProperty("Value");
Check("The case is stored in an object? Value property",
    valueProperty?.PropertyType == typeof(object),
    $"Value property type = {valueProperty?.PropertyType}");

// 5. default(PaymentMethod) holds nothing.
PaymentMethod empty = default;
Check("default(PaymentMethod) holds nothing",
    empty.Value is null, $"empty.Value is null = {empty.Value is null}");

// 6. A switch without a null arm throws SwitchExpressionException on a default union.
string outcome;
try { Checkout.Describe(empty); outcome = "no exception"; }
catch (SwitchExpressionException) { outcome = "SwitchExpressionException"; }
Check("A switch without a null arm throws on a default union",
    outcome == "SwitchExpressionException", outcome);

// 7. Value-type cases are boxed: 1,000 ints allocate 24 bytes each on x64.
long before = GC.GetAllocatedBytesForCurrentThread();
for (int i = 0; i < 1_000; i++)
{
    Amount amount = i;
    GC.KeepAlive(amount.Value);
}
long boxed = GC.GetAllocatedBytesForCurrentThread() - before;
Check("Value-type cases in a declared union are boxed",
    boxed >= 1_000 * IntPtr.Size, $"1,000 ints stored in union Amount(int, decimal) allocated {boxed} bytes");

// 8. A hand-written [Union] struct with TryGetValue is matched without boxing.
int ints = 0;
for (int i = 0; i < 1_000; i++) Match(new IntOrBool((int?)i)); // warm up the JIT
before = GC.GetAllocatedBytesForCurrentThread();
for (int i = 0; i < 1_000; i++) ints += Match(new IntOrBool((int?)i));
long unboxed = GC.GetAllocatedBytesForCurrentThread() - before;
Check("A [Union] struct with TryGetValue is pattern matched without boxing",
    unboxed == 0 && ints == 1_000, $"1,000 matches of IntOrBool allocated {unboxed} bytes");

Console.WriteLine();
Console.WriteLine(failed == 0 ? "All checks passed." : $"{failed} check(s) failed.");
return failed == 0 ? 0 : 1;

static int Match(IntOrBool value) => value switch
{
    int => 1,
    bool => 0,
    null => 0, // HasValue can be false, so the compiler asks for a null arm (CS8655)
};
