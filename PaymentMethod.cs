namespace CSharpUnionTypes;

public record CreditCard(string Last4, int ExpiryYear);
public record PayPal(string Email);
public record BankTransfer(string Iban);

#if GIFT_CARD
public record GiftCard(string Code);

public union PaymentMethod(CreditCard, PayPal, BankTransfer, GiftCard);
#else
public union PaymentMethod(CreditCard, PayPal, BankTransfer);
#endif

// A union with value-type cases. The generated union stores its case as object?, so these are boxed.
public union Amount(int, decimal);
