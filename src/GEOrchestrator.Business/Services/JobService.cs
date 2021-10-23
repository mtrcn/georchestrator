using GEOrchestrator.Business.Extensions;
using GEOrchestrator.Business.Factories;
using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Enums;
using GEOrchestrator.Domain.Models.Jobs;
using GEOrchestrator.Domain.Models.Workflows;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Dtos;

namespace GEOrchestrator.Business.Services
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly IParameterRepository _parameterRepository;
        private readonly IArtifactRepository _artifactRepository;
        private readonly IObjectRepository _objectRepository;

        public JobService(IJobRepositoryFactory jobRepositoryFactory, IParameterRepositoryFactory parameterRepositoryFactory, IArtifactRepositoryFactory artifactRepositoryFactory, IObjectRepositoryFactory objectRepositoryFactory)
        {
            _jobRepository = jobRepositoryFactory.Create();
            _parameterRepository = parameterRepositoryFactory.Create();
            _artifactRepository = artifactRepositoryFactory.Create();
            _objectRepository = objectRepositoryFactory.Create();
        }

        public async Task<Job> CreateJobAsync(Workflow workflow)
        {
            var job = new Job
            {
                Id = Guid.NewGuid().ToString(),
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
                Workflow = workflow,
                WorkflowName = workflow.Name,
                WorkflowVersion = workflow.Version,
                Status = JobStatus.Accepted
            };
            await _jobRepository.CreateAsync(job);
            return job;
        }

        public async Task<Job> GetJobAsync(string jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            return job;
        }

        public async Task<JobStatusDto> GetJobStatusAsync(string jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
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

        public async Task<Dictionary<string, JToken>> GetJobOutputAsync(string jobId)
        {
            var result = new Dictionary<string, JToken>();
            var job = await _jobRepository.GetByIdAsync(jobId);

            if (job == null)
                return null;

            foreach (var workflowOutputParameter in job.Workflow.Outputs.Parameters)
            {
                var workflowOutputs = await _parameterRepository.GetByReference(job.Id, workflowOutputParameter.Value);
                if (workflowOutputs.Count > 0)
                {
                    result.Add(workflowOutputParameter.Name, JArray.FromObject(workflowOutputs.Select(o => o.Value)));
                }
                else
                {
                    result.Add(workflowOutputParameter.Name, workflowOutputs.FirstOrDefault()?.Value);
                }
            }
            foreach (var workflowOutputArtifact in job.Workflow.Outputs.Artifacts)
            {
                var stepParameterName = workflowOutputArtifact.Value.ParseStepReferenceValue();
                var workflowOutputs = await _artifactRepository.GetAsync(job.Id, stepParameterName.stepId, stepParameterName.parameterName);
                if (workflowOutputs.Count > 0)
                {
                    foreach (var workflowOutput in workflowOutputs)
                    {
                        var signedUrlDownload = await _objectRepository.GetSignedUrlForDownloadAsync(workflowOutput.StoragePath);
                        result.Add($"{workflowOutputArtifact.Name}-{workflowOutput.Index}", JToken.FromObject(new { href = signedUrlDownload }));
                    }
                }
                else
                {
                    var signedUrlDownload = await _objectRepository.GetSignedUrlForDownloadAsync(workflowOutputs.FirstOrDefault()?.StoragePath);
                    result.Add(workflowOutputArtifact.Name, JToken.FromObject(new { href = signedUrlDownload }));
                }
            }
            return result;
        }

        public async Task SaveJobStatusAsync(string jobId, string status, string message)
        {
            await _jobRepository.UpdateStatusAsync(jobId, status, message, DateTime.UtcNow);
        }

        public async Task StartJobAsync(string jobId)
        {
            await _jobRepository.UpdateStartedAsync(jobId, DateTime.UtcNow);
        }

        public async Task FinishJobAsync(string jobId)
        {
            await _jobRepository.UpdateFinishedAsync(jobId, DateTime.UtcNow);
        }
    }
}
