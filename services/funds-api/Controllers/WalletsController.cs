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
    public class WalletsController : ControllerBase
    {
        private readonly WalletDbContext _context;


        public WalletsController(WalletDbContext context)
        {
            _context = context;
        }


        //Add new wallet
        [HttpPost]
        public async Task<IActionResult> CreateWallet([FromBody] Wallet wallet)
        {
            if (wallet == null) return BadRequest("Invalid wallet data.");

            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetWallet), new { id = wallet.Id }, wallet);
        }

        //Retrieve wallet data
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWallet(int walletId)
        {
            var wallet = await _context.Wallets.FindAsync(walletId);
            if (wallet == null) return BadRequest("Wallet not found");
            
            return Ok(wallet);
        }
    }

    
}