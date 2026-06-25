using System;
using ApplicationAccountingSystem.Designation;
using ApplicationAccountingSystem.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAccountingSystem.Interfaces
{
    public interface ISLARepository
    {
        Task<SLAPolicy> CreateSLAAsync(SLAPolicy sla);

        Task<SLAPolicy?> GetSLAByIdAsync(Guid slaId);
        
        Task<SLAPolicy?> GetSLAPolicyByPriorityAsync(TicketPriority priority);

        Task<IEnumerable<SLAPolicy>> GetAllSLAsAsync();

        Task<IEnumerable<SLAPolicy>> GetActiveSLAPoliciesAsync();
        Task UpdateSLAAsync(SLAPolicy sla);

        Task DeleteSLAAsync(Guid slaId);
    }
}