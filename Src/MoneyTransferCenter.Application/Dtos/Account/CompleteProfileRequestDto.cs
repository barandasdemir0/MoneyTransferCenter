namespace MoneyTransferCenter.Application.Dtos.Account;

public class CompleteProfileRequestDto
{
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string TelephoneNumber { get; set; } = string.Empty;
}
