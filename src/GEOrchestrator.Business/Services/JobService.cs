using GEOrchestrator.Business.Extensions;
using GEOrchestrator.Business.Factories;
using GEOrchestrator.Business.Repositories;
using GEOrchestrator.Domain.Dtos;
using GEOrchestrator.Domain.Enums;
using GEOrchestrator.Domain.Models.Jobs;
using GEOrchestrator.Domain.Models.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

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

        public async Task<Dictionary<string, JsonNode>> GetJobOutputAsync(string jobId)
        {
            var result = new Dictionary<string, JsonNode>();
            var job = await _jobRepository.GetByIdAsync(jobId);

            if (job == null)
                return null;

            foreach (var workflowOutputParameter in job.Workflow.Outputs.Parameters)
            {
                var workflowOutputs = await _parameterRepository.GetByReference(job.Id, workflowOutputParameter.Value);
                if (workflowOutputs.Count > 0)
                {
                    var values = workflowOutputs.Select(o => o.Value).ToList();
                    result.Add(workflowOutputParameter.Name, JsonSerializer.SerializeToNode(values));
                }
                else
                {
                    var value = workflowOutputs.FirstOrDefault()?.Value;
                    result.Add(workflowOutputParameter.Name, value != null ? JsonValue.Create(value) : null);
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
                        var obj = new { href = signedUrlDownload };
                        result.Add($"{workflowOutputArtifact.Name}-{workflowOutput.Index}", JsonSerializer.SerializeToNode(obj));
                    }
                }
                else
                {
                    var storagePath = workflowOutputs.FirstOrDefault()?.StoragePath;
                    var signedUrlDownload = storagePath != null ? await _objectRepository.GetSignedUrlForDownloadAsync(storagePath) : null;
                    var obj = new { href = signedUrlDownload };
                    result.Add(workflowOutputArtifact.Name, JsonSerializer.SerializeToNode(obj));
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
