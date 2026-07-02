using ApplicationAccountingSystem.Domain.Designation;
using ApplicationAccountingSystem.Domain.Interfaces;
using ApplicationAccountingSystem.Domain.Model;
using ApplicationAccountingSystem.Infrastructure.Data;
using Avalonia.Media;
using Avalonia.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAccountingSystem.Infrastructure.Repository
{
    public class TicketRepository : ITicketRepository
    {
        private readonly AppDbContext _context;
        public TicketRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Tickets> CreateTicketAsync(Tickets tickets)
        {
            _context.Tickets.Add(tickets);
            await _context.SaveChangesAsync();
            return tickets;
        }
        public async Task<Tickets> GetTicketByIdAsync(Guid ticketId)
        {
            return await _context.Tickets.FindAsync(ticketId);
        }
        public async Task<IEnumerable<Tickets>> GetAllTicketsAsync()
        {
            return await _context.Tickets.ToListAsync();
        }
        public async Task<IEnumerable<Tickets>> GetTicketsByStatusAsync(TicketStatus status)
        {
            return await _context.Tickets.Where(x=>x.Status == status).ToListAsync();
        }
        public async Task<IEnumerable<Tickets>> GetTicketsByPriorityAsync(TicketPriority priority)
        {
            return await _context.Tickets.Where(x => x.Priority == priority).ToListAsync();
        }
        public async Task<IEnumerable<Tickets>> GetTicketsByUserIdAsync(Guid userId)
        {
            return await _context.Tickets.Where(x=>x.CreatedById == userId).ToListAsync();
        }
        public async Task<IEnumerable<Tickets>> GetTicketsAssignedToUserAsync(Guid userId)
        {
            return await _context.Tickets.Where(x => x.AssignedToId == userId).ToListAsync();
        }
        public async Task UpdateTicketAsync(Tickets ticket)
        {
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteTicketAsync(Guid ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
        }
    }
}
