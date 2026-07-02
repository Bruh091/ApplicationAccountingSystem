using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationAccountingSystem.Application.DTOs;
using ApplicationAccountingSystem.Application.Interface;
using ApplicationAccountingSystem.Domain.Interfaces;
using ApplicationAccountingSystem.Domain.Model;

namespace ApplicationAccountingSystem.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<CommentDto> AddCommentAsync(CreateCommentDto dto)
        {
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Content = dto.Content,
                IsInternal = dto.IsInternal,
                UserId = dto.UserId,
                TicketId = dto.TicketId,
                CreatedAt = DateTime.UtcNow
            };

            var createdComment = await _commentRepository.CreateCommentAsync(comment);

            return MapToDto(createdComment);
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByTicketIdAsync(Guid ticketId)
        {
            var comments = await _commentRepository.GetCommentsByTicketIdAsync(ticketId);

            return comments.Select(MapToDto);
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByUserIdAsync(Guid userId)
        {
            var comments = await _commentRepository.GetCommentsByUserIdAsync(userId);

            return comments.Select(MapToDto);
        }

        public async Task DeleteCommentAsync(Guid commentId)
        {
            await _commentRepository.DeleteCommentAsync(commentId);
        }

        private static CommentDto MapToDto(Comment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                IsInternal = comment.IsInternal,
                UserId = comment.UserId,
                TicketId = comment.TicketId
            };
        }
    }
}