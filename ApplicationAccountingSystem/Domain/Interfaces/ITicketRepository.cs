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
        Task<Tickets> CreateTicketAsync(Tickets ticket);

        Task<Tickets?> GetTicketByIdAsync(Guid ticketId);

        Task<IEnumerable<Tickets>> GetAllTicketsAsync();

        Task<IEnumerable<Tickets>> GetTicketsByStatusAsync(TicketStatus status);

        Task<IEnumerable<Tickets>> GetTicketsByPriorityAsync(TicketPriority priority);

        Task<IEnumerable<Tickets>> GetTicketsByUserIdAsync(Guid userId);

        Task<IEnumerable<Tickets>> GetTicketsAssignedToUserAsync(Guid userId);

        Task UpdateTicketAsync(Tickets ticket);

        Task DeleteTicketAsync(Guid ticketId);
    }
}