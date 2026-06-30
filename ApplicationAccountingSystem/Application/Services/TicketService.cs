using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationAccountingSystem.Application.DTOs;
using ApplicationAccountingSystem.Application.Interface;
using ApplicationAccountingSystem.Domain.Designation;
using ApplicationAccountingSystem.Domain.Interfaces;
using ApplicationAccountingSystem.Domain.Model;
using ApplicationAccountingSystem.Domain.Model;
namespace ApplicationAccountingSystem.Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public async Task<TicketDto> CreateTicketAsync(CreateTicketDto dto)
        {
            var ticket = new Tickets
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                Status = TicketStatus.New,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedById = dto.CreatedById
            };

            var createdTicket = await _ticketRepository.CreateTicketAsync(ticket);

            return MapToDto(createdTicket);
        }

        public async Task<TicketDto?> GetTicketByIdAsync(Guid ticketId)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);

            if (ticket == null)
            {
                return null;
            }

            return MapToDto(ticket);
        }

        public async Task<IEnumerable<TicketDto>> GetAllTicketsAsync()
        {
            var tickets = await _ticketRepository.GetAllTicketsAsync();

            return tickets.Select(MapToDto);
        }

        public async Task<IEnumerable<TicketDto>> GetTicketsByUserIdAsync(Guid userId)
        {
            var tickets = await _ticketRepository.GetTicketsByUserIdAsync(userId);

            return tickets.Select(MapToDto);
        }

        private static TicketDto MapToDto(Tickets ticket)
        {
            return new TicketDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedAt = ticket.CreatedAt,
                CreatedById = ticket.CreatedById,
                AssignedToId = ticket.AssignedToId
            };
        }
        public async Task<TicketDto?> AssignTicketAsync(Guid ticketId, Guid agentId)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null) { return null; }
            ticket.AssignTo(agentId);
            _ticketRepository.UpdateTicketAsync(ticket);
            return MapToDto(ticket);
        }
        public async Task<TicketDto?> ChangeStatusAsync(Guid ticketId, TicketStatus status)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null) { return null; }
            ticket.ChangeStatus(status);
            _ticketRepository.UpdateTicketAsync(ticket);
            return MapToDto(ticket);
        }
        public async Task<TicketDto?> ChangePriorityAsync(Guid ticketId, TicketPriority priority)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null) { return null; }
            ticket.ChangePriority(priority);
            _ticketRepository.UpdateTicketAsync(ticket);
            return MapToDto(ticket);
        }
    }
}