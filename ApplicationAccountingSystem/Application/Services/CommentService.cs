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
        private readonly IUserRepository _userRepository;

        public CommentService(ICommentRepository commentRepository, IUserRepository userRepository)
        {
            _commentRepository = commentRepository;
            _userRepository = userRepository;
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

            return await MapToDtoAsync(createdComment);
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByTicketIdAsync(Guid ticketId)
        {
            var comments = await _commentRepository.GetCommentsByTicketIdAsync(ticketId);

            var result = new List<CommentDto>();

            foreach (var comment in comments)
            {
                result.Add(await MapToDtoAsync(comment));
            }

            return result;
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByUserIdAsync(Guid userId)
        {
            var comments = await _commentRepository.GetCommentsByUserIdAsync(userId);

            var result = new List<CommentDto>();

            foreach (var comment in comments)
            {
                result.Add(await MapToDtoAsync(comment));
            }

            return result;
        }

        public async Task DeleteCommentAsync(Guid commentId)
        {
            await _commentRepository.DeleteCommentAsync(commentId);
        }

        private async Task<CommentDto> MapToDtoAsync(Comment comment)
        {
            var author = await _userRepository.GetUserByIdAsync(comment.UserId);

            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                IsInternal = comment.IsInternal,
                UserId = comment.UserId,
                AuthorName = author?.FullName ?? comment.UserId.ToString(),
                TicketId = comment.TicketId
            };
        }
    }
}
