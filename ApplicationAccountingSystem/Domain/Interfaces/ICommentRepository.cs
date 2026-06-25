using System;
using ApplicationAccountingSystem.Domain.Designation;
using ApplicationAccountingSystem.Domain.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ApplicationAccountingSystem.Domain.Interfaces
{
    public interface ICommentRepository
    {
        Task<Comment> CreateCommentAsync(Comment comment);

        Task<Comment?> GetCommentByIdAsync(Guid commentId);

        Task<IEnumerable<Comment>> GetCommentsByTicketIdAsync(Guid ticketId);
        Task<IEnumerable<Comment>> GetCommentsByUserIdAsync(Guid userId);

        Task UpdateCommentAsync(Comment comment);

        Task DeleteCommentAsync(Guid commentId);
    }
}