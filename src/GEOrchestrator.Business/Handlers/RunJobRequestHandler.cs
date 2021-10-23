using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Requests;
using GEOrchestrator.Business.Services;
using GEOrchestrator.Domain.Dtos;
using GEOrchestrator.Domain.Enums;
using GEOrchestrator.Domain.Models.Jobs;
using GEOrchestrator.Domain.Models.Parameters;
using MediatR;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GEOrchestrator.Business.Extensions;

namespace GEOrchestrator.Business.Handlers
{
    public class RunJobRequestHandler : IRequestHandler<RunJobRequest, JobStatusDto>
    {
        private readonly IJobService _jobService;
        private readonly IParameterService _parameterService;
        private readonly IMediator _mediator;

        public RunJobRequestHandler(IJobService jobService, IParameterService parameterService, IMediator mediator)
        {
            _jobService = jobService;
            _parameterService = parameterService;
            _mediator = mediator;
        }

        public async Task<JobStatusDto> Handle(RunJobRequest notification, CancellationToken cancellationToken)
        {
            var job = await _jobService.CreateJobAsync(notification.Workflow);

            foreach (var input in notification.Inputs)
            {
                var values = input.Value.ReadValues();
                var index = 0;
                foreach (var value in values)
                {
                    await _parameterService.SaveParameterAsync(new Parameter
                    {
                        JobId = job.Id,
                        Index = index++,
                        Name = input.Key,
                        Value = value
                    });
                }
            }

            var firstStep = job.Workflow.Steps.First();

            await _mediator.Publish(new RunNextStepEvent(job.Id, 0, firstStep.Id), cancellationToken);

            await _jobService.StartJobAsync(job.Id);
            job.Status = JobStatus.Running;

            return new JobStatusDto
            {
                JobId = job.Id,
                ProcessId = job.WorkflowName,
                Message = job.Message,
                Created = job.Created,
                Finished = job.Finished,
                Started = job.Started,
                Status = job.Status,
                Updated = job.Updated
            };
        }
    }
}
