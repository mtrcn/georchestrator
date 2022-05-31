using GEOrchestrator.Domain.Models.Executions;
using GEOrchestrator.Domain.Models.Workflows;
using System.Collections.Generic;
using System.Linq;

namespace GEOrchestrator.Business.Extensions
{
    public static class WorkflowExtension
    {
        public static WorkflowStep NextWorkflowStep(this Workflow workflow, string stepId)
        {
            var tree = BuildTree(workflow.Steps, new ExecutionNode());
            var stepExecutionNodeNode = Search(tree, stepId);
            var nextExecutionNode = FindNextWorkflowStep(tree, stepExecutionNodeNode);
            return nextExecutionNode?.Node;
        }

        public static WorkflowStep FindWorkflowStep(this Workflow workflow, string stepId)
        {
            var tree = BuildTree(workflow.Steps, new ExecutionNode());
            var stepExecutionNodeNode = Search(tree, stepId);
            return stepExecutionNodeNode.Node;
        }

        public static List<WorkflowStep> GetChildren(this Workflow workflow, string stepId)
        {
            var tree = BuildTree(workflow.Steps, new ExecutionNode());
            var stepExecutionNodeNode = Search(tree, stepId);
            return stepExecutionNodeNode.Children.Select(c => c.Node).ToList();
        }

        public static WorkflowStep PreviousWorkflowStep(this Workflow workflow, string stepId)
        {
            var tree = BuildTree(workflow.Steps, new ExecutionNode());
            var stepExecutionNodeNode = Search(tree, stepId);
            return stepExecutionNodeNode.PreviousStep;
        }

        private static ExecutionNode FindNextWorkflowStep(ExecutionNode root, ExecutionNode executionNode)
        {

            if (executionNode.NextStep == null && executionNode.Parent != null)
            {
                var parentExecutionNode = Search(root, executionNode.Parent.Id);
                return FindNextWorkflowStep(root, parentExecutionNode);
            }
            return executionNode.NextStep != null ? Search(root, executionNode.NextStep.Id) : null;
        }

        private static ExecutionNode BuildTree(List<WorkflowStep> children, ExecutionNode node)
        {
            foreach (var child in children)
            {
                var previous = children.IndexOf(child) == 0 ? null : children[children.IndexOf(child) - 1];
                var next = children.Count - 1 <= children.IndexOf(child) ? null : children[children.IndexOf(child) + 1];
                if (child.Iterate != null && child.Iterate.Steps.Count > 0)
                {
                    node.Children.Add(BuildTree(child.Iterate.Steps, new ExecutionNode
                    {
                        Parent = node.Node,
                        PreviousStep = previous,
                        NextStep = next,
                        Node = child
                    }));
                }
                else if (child.Branches != null && child.Branches.Count > 0)
                {
                    foreach (var branch in child.Branches)
                    {
                        node.Children.Add(BuildTree(branch, new ExecutionNode
                        {
                            Parent = node.Node,
                            PreviousStep = previous,
                            NextStep = next,
                            Node = child
                        }));
                    }
                }
                else
                {
                    node.Children.Add(new ExecutionNode { Node = child, Parent = node.Node, Children = null, PreviousStep = previous, NextStep = next });
                }
            }

            return node;
        }

        private static ExecutionNode Search(ExecutionNode node, string completedStepId)
        {
            if (node.Children == null)
                return null;

            ExecutionNode result = null;

            foreach (var executionNode in node.Children)
            {
                result = executionNode.Node?.Id == completedStepId ? executionNode : Search(executionNode, completedStepId);
                if (result != null)
                    break;
            }

            return result;
        }
    }
}
