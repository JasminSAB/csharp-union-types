namespace CSharpUnionTypes;

public static class Checkout
{
    // No default arm. Every case type is handled, so the switch is exhaustive.
    public static string Describe(PaymentMethod payment) => payment switch
    {
        CreditCard card => $"card ending {card.Last4}, expires {card.ExpiryYear}",
        PayPal paypal => $"PayPal account {paypal.Email}",
        BankTransfer bank => $"bank transfer from {bank.Iban}",
#if DISCARD_ARM
        _ => "unknown payment method",
#endif
    };

#if WRONG_TYPE
    // string is not a case type of PaymentMethod: this must not compile.
    public static PaymentMethod NotACase() => "cash";
#endif
}
