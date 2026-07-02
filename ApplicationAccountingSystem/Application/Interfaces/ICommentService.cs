using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationAccountingSystem.Application.DTOs;

namespace ApplicationAccountingSystem.Application.Interface
{
    public interface ICommentService
    {
        Task<CommentDto> AddCommentAsync(CreateCommentDto dto);

        Task<IEnumerable<CommentDto>> GetCommentsByTicketIdAsync(Guid ticketId);

        Task<IEnumerable<CommentDto>> GetCommentsByUserIdAsync(Guid userId);

        Task DeleteCommentAsync(Guid commentId);
    }
}