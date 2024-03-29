﻿using GEOrchestrator.Business.Events;
using GEOrchestrator.Business.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GEOrchestrator.Domain.Dtos;

namespace GEOrchestrator.WorkflowManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StepExecutionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StepExecutionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("{stepExecutionId}/activities")]
        public async Task<IActionResult> ActivityAsync(string stepExecutionId, [FromBody] StepExecutionActivityDto stepExecutionActivityDto)
        {
            await _mediator.Publish(new StepExecutionEvent(stepExecutionId, stepExecutionActivityDto.Type, stepExecutionActivityDto.Payload));
            return NoContent();
        }

        [HttpGet("{stepExecutionId}/inputs")]
        public async Task<IActionResult> InputsAsync(string stepExecutionId)
        {
            var response = await _mediator.Send(new StepExecutionInputsRequest(stepExecutionId));
            return Ok(response);
        }

        [HttpPost("{stepExecutionId}/outputs")]
        public async Task<IActionResult> SaveArtifactOutputsAsync(string stepExecutionId, [FromBody] SendOutputActivityDto sendOutputActivityDto)
        {
            var response = await _mediator.Send(new SendOutputRequest(stepExecutionId, sendOutputActivityDto));
            if (string.IsNullOrEmpty(response))
                return NoContent();
            return Ok(response);
        }
    }
}
