using GEOrchestrator.Business.Extensions;
using GEOrchestrator.Domain.Models.Tasks;
using GEOrchestrator.Domain.Models.Workflows;
using System.Collections.Generic;
using Xunit;

namespace GEOrchestrator.Business.Test.Extensions
{
    public class WorkflowExtensionTests
    {
        private readonly Workflow _workflow;
        public WorkflowExtensionTests()
        {
            _workflow = new Workflow
            {
                Steps = new List<WorkflowStep>
                {
                    new WorkflowStep() {Id = "Step1"}, 
                    new WorkflowStep() {Id = "Step2", Iterate = new Iterate()
                    {
                        Steps = new List<WorkflowStep>
                        {
                            new WorkflowStep() {Id = "Step3"},
                            new WorkflowStep() {Id = "Step4", Branches = new List<List<WorkflowStep>>()
                            {
                                new List<WorkflowStep>()
                                {
                                    new WorkflowStep() {Id = "Step5"},
                                    new WorkflowStep() {Id = "Step6"}
                                },
                                new List<WorkflowStep>()
                                {
                                    new WorkflowStep() {Id = "Step7"},
                                    new WorkflowStep() {Id = "Step8"}
                                }
                            }}
                        }
                    }},
                    new WorkflowStep() {Id = "Step9", Branches = new List<List<WorkflowStep>>()
                    {
                        new List<WorkflowStep>()
                        {
                            new WorkflowStep() {Id = "Step10"},
                            new WorkflowStep() {Id = "Step11"}
                        },
                        new List<WorkflowStep>()
                        {
                            new WorkflowStep() {Id = "Step12"},
                            new WorkflowStep() {Id = "Step13"}
                        }
                    }},
                    new WorkflowStep() {Id = "Step14"},
                    new WorkflowStep() {Id = "Step15", Iterate = new Iterate()
                    {
                        Steps = new List<WorkflowStep>
                        {
                            new WorkflowStep() {Id = "Step16", Iterate = new Iterate() //0
                            {
                                Steps = new List<WorkflowStep>
                                {
                                    new WorkflowStep() {Id = "Step17"}, //1
                                    new WorkflowStep() {Id = "Step18"} //1
                                }
                            }},
                            new WorkflowStep() {Id = "Step19"} //0
                        }
                    }}
                }
            };
        }

        [Theory]
        [InlineData("Step1", "Step2")]
        [InlineData("Step3", "Step4")]
        [InlineData("Step6", "Step9")]
        [InlineData("Step8", "Step9")]
        [InlineData("Step12", "Step13")]
        [InlineData("Step16", "Step19")]
        [InlineData("Step18", "Step19")]
        [InlineData("Step15", null)]
        [InlineData("Step19", null)]
        public void NextWorkflowStepTests(string completedStepId, string expectedNextStepId)
        {
            //Act
            var nextStep = _workflow.NextWorkflowStep(completedStepId);

            //Assert
            Assert.Equal(expectedNextStepId, nextStep?.Id);
        }


        [Theory]
        [InlineData("Step4", "Step3")]
        [InlineData("Step18", "Step17")]
        [InlineData("Step19", "Step16")]
        [InlineData("Step12", null)]
        public void PreviousWorkflowStepTests(string stepId, string expectedPreviousStepId)
        {
            //Act
            var nextStep = _workflow.PreviousWorkflowStep(stepId);

            //Assert
            Assert.Equal(expectedPreviousStepId, nextStep?.Id);
        }
    }
}
