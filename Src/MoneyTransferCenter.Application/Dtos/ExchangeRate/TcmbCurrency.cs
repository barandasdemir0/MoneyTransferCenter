using System.Xml.Serialization;

namespace MoneyTransferCenter.Application.Dtos.ExchangeRate;

public sealed class TcmbCurrency
{
    [XmlAttribute("Kod")]
    public string Kod { get; set; } = string.Empty;


    [XmlElement("Isim")]
    public string Isim { get; set; } = string.Empty;


    [XmlElement("ForexBuying")]
    public string ForexBuying { get; set; } = string.Empty;


    [XmlElement("ForexSelling")]
    public string ForexSelling { get; set; } = string.Empty;
}
