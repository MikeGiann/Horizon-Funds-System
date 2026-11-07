namespace FundsApi.Models
{
    public class TransactionEvent
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = "Credit"; // "Credit" or "Debit"
        public string IdempotencyKey { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Wallet Wallet { get; set; }
    }
}