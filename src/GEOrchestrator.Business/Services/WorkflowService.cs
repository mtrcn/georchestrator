using GEOrchestrator.Business.Factories;
using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Models.Workflows;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GEOrchestrator.Business.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowValidatorService _workflowValidatorService;
        private readonly IWorkflowRepository _workflowRepository;

        public WorkflowService(
            IWorkflowValidatorService workflowValidatorService, 
            IWorkflowRepositoryFactory workflowRepositoryFactory
        )
        {
            _workflowValidatorService = workflowValidatorService;
            _workflowRepository = workflowRepositoryFactory.Create();
        }

        public async Task<List<string>> Register(Workflow workflow)
        {
            var (isValid, messages) = await _workflowValidatorService.ValidateAsync(workflow);
            if (!isValid)
            {
                return messages;
            }

            workflow.Version = 1;

            var lastVersion = await _workflowRepository.GetByNameAsync(workflow.Name);
            if (lastVersion != null)
            {
                workflow.Version = lastVersion.Version + 1;
            }

            await _workflowRepository.RegisterAsync(workflow);

            return messages;
        }

        public async Task<Workflow> GetWorkflowByName(string workflowName)
        {
            var workflow = await _workflowRepository.GetByNameAsync(workflowName);
            return workflow;
        }

        public async Task<List<Workflow>> GetAllAsync()
        {
            var workflows = await _workflowRepository.GetAllAsync();
            return workflows;
        }
    }
}
