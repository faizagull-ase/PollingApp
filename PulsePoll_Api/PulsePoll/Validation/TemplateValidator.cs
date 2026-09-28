using PulsePoll.Models.Dtos.Templates;

namespace PulsePoll.Validation;

// title required; 1+ questions; 2-4 options per question; correctOptionIndex in range
public class TemplateValidator
{
    public IReadOnlyList<string> Validate(CreateTemplateRequest request)
    {
        var errors = new List<string>();
        // No missing Title validation 
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors.Add("Title is required.");
        }
        // 1 question is required
        if (request.Questions is null || request.Questions.Count == 0)
        {
            errors.Add("At least one question is required.");
            return errors;
        }
        // if no text is given for questions
        for (var i = 0; i < request.Questions.Count; i++)
        {
            var question = request.Questions[i];

            if (string.IsNullOrWhiteSpace(question.Text))
            {
                errors.Add($"Question {i + 1}: text is required.");
            }
            // At least 2 options are required for each question 
            var optionCount = question.Options?.Count ?? 0;
            if (optionCount < 2)
            {
                errors.Add($"Question {i + 1}: must have at least 2 options.");
            }
            // Maximum questions allowed are 4 
            else if (optionCount > 4)
            {
                errors.Add($"Question {i + 1}: must have at most 4 options.");
            }
            // Correct Option is out of index 
            if (question.CorrectOptionIndex is int correctIndex &&
                (correctIndex < 0 || correctIndex >= optionCount))
            {
                errors.Add($"Question {i + 1}: correctOptionIndex is out of range for its options.");
            }
        }

        return errors;
    }
}
