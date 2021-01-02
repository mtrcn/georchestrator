using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GEOrchestrator.Domain.Enums;
using GEOrchestrator.Domain.Models.Tasks;

namespace GEOrchestrator.Business.Services
{
    public class TaskValidatorService : ITaskValidatorService
    {
        public (bool isValid, List<string> messages) Validate(Task task)
        {
            var isValid = true;
            var validationMessages = new List<string>();

            var namePattern = @"^[a-zA-Z0-9]{3,128}$";
            if (!Regex.Match(task.Name, namePattern).Success)
            {
                validationMessages.Add($"{task.Name}: Name doesn't not comply with requirements: Only consist of alpha-numeric characters and minimum 3 and maximum 128 characters length.");
                isValid = false;
            }

            if (string.IsNullOrEmpty(task.Image))
            {
                validationMessages.Add($"{task.Name}: Image cannot be empty.");
                isValid = false;
            }

            var allowedTaskOutputTypes = new[] {TaskOutputType.Artifact, TaskOutputType.Parameter};
            var allowedTaskInputTypes = new[] {TaskInputType.Artifact, TaskInputType.Parameter};
            var parameterNamePattern = @"^[a-zA-Z0-9\._]+$";

            foreach (var taskOutput in task.Outputs)
            {
                if (!Regex.Match(taskOutput.Name, parameterNamePattern).Success)
                {
                    validationMessages.Add($"{task.Name}-{taskOutput.Name}: Name doesn't not comply with requirements: Only consist of alpha-numeric, '.', and '_' characters.");
                    isValid = false;
                }

                if (!allowedTaskOutputTypes.Contains(taskOutput.Type))
                {
                    validationMessages.Add($"{task.Name}-{taskOutput.Name}: Task output type is not allowed. Allowed types: {string.Join(',', allowedTaskOutputTypes)}");
                    isValid = false;
                }

                if (task.Outputs.Count(o => o.Name == taskOutput.Name) > 1)
                {
                    validationMessages.Add($"{task.Name}-{taskOutput.Name}: Same parameter name cannot be used more than once.");
                    isValid = false;
                }
            }

            foreach (var taskInput in task.Inputs)
            {
                if (!Regex.Match(taskInput.Name, parameterNamePattern).Success)
                {
                    validationMessages.Add($"{task.Name}-{taskInput.Name}: Name doesn't not comply with requirements: Only consist of alpha-numeric, '.', and '_' characters.");
                    isValid = false;
                }

                if (!allowedTaskOutputTypes.Contains(taskInput.Type))
                {
                    validationMessages.Add($"{task.Name}-{taskInput.Name}: Task input type is not allowed. Allowed types: {string.Join(',', allowedTaskInputTypes)}");
                    isValid = false;
                }

                if (task.Inputs.Count(o => o.Name == taskInput.Name) > 1)
                {
                    validationMessages.Add($"{task.Name}-{taskInput.Name}: Same parameter name cannot be used more than once.");
                    isValid = false;
                }
            }

            return (isValid, validationMessages);
        }
    }
}
