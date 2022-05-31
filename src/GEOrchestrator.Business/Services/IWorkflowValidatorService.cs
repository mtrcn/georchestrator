using System.Collections.Generic;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Models.Workflows;

namespace GEOrchestrator.Business.Services
{
    public interface IWorkflowValidatorService
    {
        Task<(bool isValid, List<string> messages)> ValidateAsync(Workflow workflow);
    }
}