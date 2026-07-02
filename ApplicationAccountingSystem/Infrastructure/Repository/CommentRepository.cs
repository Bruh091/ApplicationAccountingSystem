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
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _context;
        public CommentRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }
        public async Task<Comment?> GetCommentByIdAsync(Guid commentId)
        {
            return await _context.Comments.FindAsync(commentId);
        }
        public async Task<IEnumerable<Comment>> GetCommentsByTicketIdAsync(Guid ticketId)
        {
            return await _context.Comments.Where(x => x.TicketId == ticketId).ToListAsync();
        }
        public async Task<IEnumerable<Comment>> GetCommentsByUserIdAsync(Guid userId)
        {
            return await _context.Comments.Where(x=>x.UserId == userId).ToListAsync();
        }
        public async Task UpdateCommentAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteCommentAsync(Guid commentId)
        {
            var command = await _context.Comments.FindAsync($"{commentId}");
            if (command == null) return;
            _context.Comments.Remove(command);
            await _context.SaveChangesAsync();
        }
    }
    
}
    
