using System.Threading.Tasks;
using GEOrchestrator.Business.Exceptions;
using GEOrchestrator.Business.Factories;
using GEOrchestrator.Business.Repositories;

namespace GEOrchestrator.Business.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskValidatorService _taskValidatorService;
        private readonly ITaskRepository _taskRepository;

        public TaskService(
            ITaskValidatorService taskValidatorService,
            ITaskRepositoryFactory taskRepositoryFactory
        )
        {
            _taskValidatorService = taskValidatorService;
            _taskRepository = taskRepositoryFactory.Create();
        }

        public async Task<Domain.Models.Tasks.Task> GetTaskByNameAsync(string name)
        {
            return await _taskRepository.GetByNameAsync(name);
        }

        public async Task SaveAsync(Domain.Models.Tasks.Task task)
        {
            var validation = _taskValidatorService.Validate(task);
            if (!validation.isValid)
            {
                throw new TaskValidationException(string.Join('\n', validation.messages));
            }

            await _taskRepository.SaveAsync(task);
        }
    }
}
