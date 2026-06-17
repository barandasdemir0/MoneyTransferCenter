using MoneyTransferCenter.Domain.Interfaces;
using MoneyTransferCenter.Domain.Interfaces.Repositories;
using System.Text;

namespace MoneyTransferCenter.Infrastructure.Services;

public sealed class IbanGenerator : IIbanGenerator
{
    private readonly IAccountRepository _accountRepository;

    public IbanGenerator(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    private const string BankCode = "00123";// bankanın kimliğini belirtir
    private const string ReserveField = "0"; 
    public async Task<string> GenerateAsync()
    {
        string iban; 
        bool exists;
        do 
        {
            string accountNumber = GenerateRandomAccountNumber();
            string checkDigits = CalculateCheckDigits(BankCode, ReserveField, accountNumber);
            iban = $"TR{checkDigits}{BankCode}{ReserveField}{accountNumber}"; 
            exists = await _accountRepository.IbanExistsAsync(iban);
        } while (exists);
        return iban;
    }

    private static string GenerateRandomAccountNumber()
    {
        StringBuilder sb = new StringBuilder(16); // Hesap numarasının uzunluğunu belirtir
        for (int i = 0; i < 16; i++)
        {
            sb.Append(Random.Shared.Next(0, 10));
        }
        return sb.ToString();
    }


    //bu method, banka kodu, rezerv alanı ve hesap numarasını kullanarak IBAN'ın kontrol basamaklarını hesaplar. IBAN'ın doğruluğunu sağlamak için Mod97 algoritmasını kullanır. Kontrol basamakları, IBAN'ın başında yer alır ve IBAN'ın geçerli olup olmadığını doğrulamak için kullanılır.
    private static string CalculateCheckDigits(string bankCode, string reserveField, string accountNumber)
    {
        string numericString = $"{bankCode}{reserveField}{accountNumber}292700";
        int remainder = Mod97(numericString);
        int checkDigit = 98 - remainder;
        return checkDigit.ToString("D2");
    }
    private static int Mod97(string number) 
    {
        int checksum = 0; // Mod97 algoritması için kalan değeri tutar
        foreach (char c in number) 
        {
            checksum = (checksum * 10 + (c - '0')) % 97; // Her karakteri işleyerek kalan değeri günceller
        }
        return checksum;
    }
}
