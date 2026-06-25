using System;
using ApplicationAccountingSystem.Domain.Designation;
using ApplicationAccountingSystem.Domain.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAccountingSystem.Domain.Interfaces
{
    public interface ITicketRepository
    {
        Task<Ticket> CreateTicketAsync(Ticket ticket);

        Task<Ticket?> GetTicketByIdAsync(Guid ticketId);

        Task<IEnumerable<Ticket>> GetAllTicketsAsync();

        Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(TicketStatus status);

        Task<IEnumerable<Ticket>> GetTicketsByPriorityAsync(TicketPriority priority);

        Task<IEnumerable<Ticket>> GetTicketsByUserIdAsync(Guid userId);

        Task<IEnumerable<Ticket>> GetTicketsAssignedToUserAsync(Guid userId);

        Task UpdateTicketAsync(Ticket ticket);

        Task DeleteTicketAsync(Guid ticketId);
    }
}