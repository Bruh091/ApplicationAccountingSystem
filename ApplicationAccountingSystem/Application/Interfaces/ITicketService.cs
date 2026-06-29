using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationAccountingSystem.Application.DTOs;

namespace ApplicationAccountingSystem.Application.Interface
{
    public interface ITicketService
    {
        Task<TicketDto> CreateTicketAsync(CreateTicketDto dto);
        Task<TicketDto?> GetTicketByIdAsync(Guid ticketId);
        Task<IEnumerable<TicketDto>> GetAllTicketsAsync();
        Task<IEnumerable<TicketDto>> GetTicketsByUserIdAsync(Guid userId);
    }
}