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
    public class SLARepository : ISLARepository
    {
        private readonly AppDbContext _context;
        public SLARepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<SLAPolicy> CreateSLAAsync(SLAPolicy sla)
        {
            _context.SLAPolicies.Add(sla);
            await _context.SaveChangesAsync();
            return sla;
        }
        public async Task<SLAPolicy?> GetSLAByIdAsync(Guid slaId)
        {
            return await _context.SLAPolicies.FindAsync(slaId);
        }
        public async Task<SLAPolicy?> GetSLAPolicyByPriorityAsync(TicketPriority priority)
        {
            return await _context.SLAPolicies.Where(x=>x.Priority == priority).FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<SLAPolicy>> GetAllSLAsAsync()
        {
            return await _context.SLAPolicies.ToListAsync();
        }
        public async Task<IEnumerable<SLAPolicy>> GetActiveSLAPoliciesAsync()
        {
            return await _context.SLAPolicies.Where(x=>x.IsActive==true).ToListAsync();
        }
        public async Task UpdateSLAAsync(SLAPolicy sla)
        {
            _context.SLAPolicies.Update(sla);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteSLAAsync(Guid slaId)
        {
            var sla = await _context.SLAPolicies.FindAsync(slaId);
            if (sla == null) return;
            _context.SLAPolicies.Remove(sla);
            await _context.SaveChangesAsync();
        }

    }
}
