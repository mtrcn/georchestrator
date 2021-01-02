using System.Threading.Tasks;
using GEOrchestrator.Business.Exceptions;
using GEOrchestrator.Business.Factories;
using GEOrchestrator.Business.Repositories.Workflow;
using GEOrchestrator.Domain.Models.Workflows;

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

        public async Task Register(Workflow workflow)
        {
            var validation = await _workflowValidatorService.ValidateAsync(workflow);
            if (!validation.isValid)
            {
                throw new WorkflowValidationException(string.Join('\n', validation.messages));
            }

            workflow.Version = 1;

            try
            {
                var lastVersion = await _workflowRepository.GetByNameAsync(workflow.Name);
                workflow.Version = lastVersion.Version + 1;
            }
            catch(WorkflowNotFoundException) { }

            await _workflowRepository.RegisterAsync(workflow);
        }

        public async Task<Workflow> GetWorkflowByName(string workflowName)
        {
            var workflow = await _workflowRepository.GetByNameAsync(workflowName);
            return workflow;
        }
    }
}
