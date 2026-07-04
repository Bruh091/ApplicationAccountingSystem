using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationAccountingSystem.Application.DTOs;
using ApplicationAccountingSystem.Application.Interface;
using ApplicationAccountingSystem.Domain.Designation;
using ApplicationAccountingSystem.Domain.Interfaces;
using ApplicationAccountingSystem.Domain.Model;
namespace ApplicationAccountingSystem.Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;

        public TicketService(ITicketRepository ticketRepository, IUserRepository userRepository)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
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

            return await MapToDtoAsync(createdTicket);
        }

        public async Task<TicketDto?> GetTicketByIdAsync(Guid ticketId)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);

            if (ticket == null)
            {
                return null;
            }

            return await MapToDtoAsync(ticket);
        }

        public async Task<IEnumerable<TicketDto>> GetAllTicketsAsync()
        {
            var tickets = await _ticketRepository.GetAllTicketsAsync();

            var result = new List<TicketDto>();

            foreach (var ticket in tickets)
            {
                result.Add(await MapToDtoAsync(ticket));
            }

            return result;
        }

        public async Task<IEnumerable<TicketDto>> GetTicketsByUserIdAsync(Guid userId)
        {
            var tickets = await _ticketRepository.GetTicketsByUserIdAsync(userId);

            var result = new List<TicketDto>();

            foreach (var ticket in tickets)
            {
                result.Add(await MapToDtoAsync(ticket));
            }

            return result;
        }

        private async Task<TicketDto> MapToDtoAsync(Tickets ticket)
        {
            var creator = await _userRepository.GetUserByIdAsync(ticket.CreatedById);
            var assignee = ticket.AssignedToId.HasValue
                ? await _userRepository.GetUserByIdAsync(ticket.AssignedToId.Value)
                : null;

            return new TicketDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedAt = ticket.CreatedAt,
                CreatedById = ticket.CreatedById,
                AssignedToId = ticket.AssignedToId,
                CreatorName = creator?.FullName ?? ticket.CreatedById.ToString(),
                AssigneeName = assignee?.FullName
            };
        }
        public async Task<TicketDto?> AssignTicketAsync(Guid ticketId, Guid agentId)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null) { return null; }
            ticket.AssignTo(agentId);
            await _ticketRepository.UpdateTicketAsync(ticket);
            return await MapToDtoAsync(ticket);
        }
        public async Task<TicketDto?> ChangeStatusAsync(Guid ticketId, TicketStatus status)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null) { return null; }
            ticket.ChangeStatus(status);
            await _ticketRepository.UpdateTicketAsync(ticket);
            return await MapToDtoAsync(ticket);
        }
        public async Task<TicketDto?> ChangePriorityAsync(Guid ticketId, TicketPriority priority)
        {
            var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
            if (ticket == null) { return null; }
            ticket.ChangePriority(priority);
            await _ticketRepository.UpdateTicketAsync(ticket);
            return await MapToDtoAsync(ticket);
        }
    }
}
