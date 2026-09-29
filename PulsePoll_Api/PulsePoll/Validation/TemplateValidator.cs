using PulsePoll.Models.Dtos.Templates;

namespace PulsePoll.Validation;

public class TemplateValidator
{
    public void Validate(CreateTemplateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Title is required.");
        }

        if (request.Questions.Count == 0)
        {
            throw new ArgumentException("At least one question is required.");
        }

        for (var i = 0; i < request.Questions.Count; i++)
        {
            var question = request.Questions[i];

            if (string.IsNullOrWhiteSpace(question.Text))
            {
                throw new ArgumentException($"Question {i + 1}: text is required.");
            }

            if (question.Options.Count < 2 || question.Options.Count > 4)
            {
                throw new ArgumentException($"Question {i + 1}: must have between 2 and 4 options.");
            }

            if (question.Options.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException($"Question {i + 1}: options cannot be empty.");
            }

            if (question.CorrectOptionIndex is { } correctIndex &&
                (correctIndex < 0 || correctIndex >= question.Options.Count))
            {
                throw new ArgumentException($"Question {i + 1}: correctOptionIndex is out of range.");
            }
        }
    }
}
