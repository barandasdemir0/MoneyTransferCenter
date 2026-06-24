using System.Xml.Serialization;

namespace MoneyTransferCenter.Application.Dtos.ExchangeRate;

[XmlRoot("Tarih_Date")]
public sealed class TcmbKurlar
{
    [XmlElement("Currency")]
    public List<TcmbCurrency> Currencies { get; set; } = new();
}
