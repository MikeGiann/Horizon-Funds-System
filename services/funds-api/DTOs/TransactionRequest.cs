namespace FundsApi.DTOs;

public class TransactionRequest
{
    public int WalletId { get; set; }
    public decimal Amount { get; set; }
}