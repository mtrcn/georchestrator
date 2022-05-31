using System.Collections.Generic;
using GEOrchestrator.Domain.Models.Tasks;

namespace GEOrchestrator.Business.Services
{
    public interface ITaskValidatorService
    {
        (bool isValid, List<string> messages) Validate(Task task);
    }
}