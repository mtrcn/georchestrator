using GEOrchestrator.Domain.Dtos;
using GEOrchestrator.Domain.Models.Workflows;
using MediatR;
using System.Collections.Generic;

namespace GEOrchestrator.Business.Requests
{
    public class RunJobRequest : IRequest<JobStatusDto>
    {
        public RunJobRequest(Workflow workflow, Dictionary<string, string> inputs)
        {
            Workflow = workflow;
            Inputs = inputs;
        }
        public Workflow Workflow { get; }
        public Dictionary<string, string> Inputs { get; }
    }
}
