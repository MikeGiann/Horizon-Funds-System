using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using FundsApi.Data;
using FundsApi.Models;
using FundsApi.Filters;
using FundsApi.Hubs;
using FundsApi.DTOs;
using System.Net.Http.Json;

namespace FundsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly WalletDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHubContext<FundsHub> _hubContext;

        // Simple in-memory circuit breaker state
        private bool _circuitOpen = false;
        private DateTime _lastFailureTime = DateTime.MinValue;
        private readonly TimeSpan _breakerTimeout = TimeSpan.FromMinutes(1);

        public TransactionsController(WalletDbContext context, IHttpClientFactory httpClientFactory, IHubContext<FundsHub> hubContext)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _hubContext = hubContext;
        }

        [HttpGet("getBalance/{walletId}")]
        public async Task<ActionResult> GetBalance(int walletId)
        {
            var wallet = await _context.Wallets.FindAsync(walletId);
            if (wallet == null) return NotFound("Wallet not found.");
            
            return Ok(wallet.AvailableFunds);
        }


        [HttpPost("newDeposit")]
        [ServiceFilter(typeof(IdempotencyFilter))] // Keeping  filter for security
        public async Task<ActionResult> NewDeposit([FromBody] TransactionRequest request)
        {
            var wallet = await _context.Wallets.FindAsync(request.WalletId);
            if (wallet == null) return NotFound("Wallet not found.");
            var transaction = new TransactionEvent {
                WalletId = wallet.Id,
                Amount = request.Amount,
                Type = "Deposit",
                IdempotencyKey = HttpContext.Request.Headers["X-Idempotency-Key"],
                Timestamp = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            wallet.AvailableFunds += transaction.Amount;

            await _context.SaveChangesAsync();

            // SignalR Notification
            await _hubContext.Clients.Group(wallet.AccountNumber).SendAsync("TransactionProcessed", new
            {
                WalletId = wallet.Id,
                AccountNumber = wallet.AccountNumber,
                Amount = request.Amount,
                NewBalance = wallet.AvailableFunds,
                Timestamp = transaction.Timestamp
            });

            return Ok(new { Message = "Deposit successful.", NewBalance = wallet.AvailableFunds });
        }

        // 3. Docker Health Check
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
        }

        [HttpPost("process")]
        [ServiceFilter(typeof(IdempotencyFilter))]
        public async Task<IActionResult> ProcessTransaction([FromBody] TransactionRequest request)
        {
            // 1. Validate Input
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var wallet = await _context.Wallets.FindAsync(request.WalletId);
            if (wallet == null) return NotFound("Wallet not found.");

            // 2. Call Python Risk Service with Circuit Breaker
            RiskServiceResponse? riskResult = null;
            if (_circuitOpen && DateTime.UtcNow - _lastFailureTime < _breakerTimeout)
            {
                // Circuit is open, fallback
                riskResult = new RiskServiceResponse { Approved = false, Reason = "Risk service is down (circuit breaker active)." };
            }
            else
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    var response = await client.PostAsJsonAsync("http://risk-service:8000/api/risk/evaluate", new { accountId = wallet.AccountNumber, amount = request.Amount });
                    response.EnsureSuccessStatusCode();
                    riskResult = await response.Content.ReadFromJsonAsync<RiskServiceResponse>();
                    _circuitOpen = false; // Close circuit on success
                }
                catch
                {
                    _lastFailureTime = DateTime.UtcNow;
                    _circuitOpen = true;
                    riskResult = new RiskServiceResponse { Approved = false, Reason = "Risk service temporarily unavailable." };
                }
            }
            
            if (!riskResult.Approved)
            {
                return BadRequest(new { Message = "Transaction rejected by risk service.", Reason = riskResult.Reason });
            }

            // 3. Append Transaction Event
            var transaction = new TransactionEvent
            {
                WalletId = wallet.Id,
                Amount = request.Amount,
                Type = "Debit",
                IdempotencyKey = HttpContext.Request.Headers["X-Idempotency-Key"],
                Timestamp = DateTime.UtcNow
            };
            _context.Transactions.Add(transaction);

            // 4. Calculate final balance
            wallet.AvailableFunds -= request.Amount;
            await _context.SaveChangesAsync();

            // 5. Push notification via SignalR
            await _hubContext.Clients.Group(wallet.AccountNumber).SendAsync("TransactionProcessed", new
            {
                WalletId = wallet.Id,
                AccountNumber = wallet.AccountNumber,
                Amount = request.Amount,
                NewBalance = wallet.AvailableFunds,
                Timestamp = transaction.Timestamp
            });

            return Ok(new { Message = "Transaction processed successfully.", NewBalance = wallet.AvailableFunds });
        }
    }

    
}