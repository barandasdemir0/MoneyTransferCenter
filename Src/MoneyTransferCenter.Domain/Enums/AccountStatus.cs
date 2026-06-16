namespace MoneyTransferCenter.Domain.Enums;

public enum AccountStatus
{
    //pasif hesaplarım için adres girmeden önce hesap
    Passive = 0,

    //aktif hesaplarım için
    Active = 10,

    //hesabı silerse
    Closed = 20
}
