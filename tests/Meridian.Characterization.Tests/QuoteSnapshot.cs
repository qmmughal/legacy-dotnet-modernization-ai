namespace Meridian.Characterization.Tests;

/// <summary>Runtime-neutral shape used for the golden master file.</summary>
public sealed record QuoteSnapshot(
    string Id,
    decimal Subtotal,
    decimal Discount,
    decimal Freight,
    decimal Tax,
    decimal Total,
    string DisplayTotal,
    IReadOnlyList<string> Warnings)
{
    public bool SameAs(QuoteSnapshot other) =>
        Id == other.Id && Subtotal == other.Subtotal && Discount == other.Discount &&
        Freight == other.Freight && Tax == other.Tax && Total == other.Total &&
        DisplayTotal == other.DisplayTotal && Warnings.SequenceEqual(other.Warnings);

    public override string ToString() =>
        $"{Id}: sub={Subtotal} disc={Discount} frt={Freight} tax={Tax} tot={Total} '{DisplayTotal}' [{string.Join(" | ", Warnings)}]";
}
