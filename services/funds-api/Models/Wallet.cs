namespace FundsApi.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal AvailableFunds { get; set; }
        public List<TransactionEvent> Transactions { get; set; } = new();
    }
}