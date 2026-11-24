using Api.Contracts;
using Core.Domain;
using FluentValidation;

namespace Api.Validation;

public sealed class SaveBookRequestValidator : AbstractValidator<SaveBookRequest>
{
    public SaveBookRequestValidator()
    {
        RuleFor(r => r.Isbn)
            .Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage("ISBN обов'язковий.")
            .Matches(@"^[0-9][0-9\-]{8,15}[0-9Xx]$").When(r => !string.IsNullOrWhiteSpace(r.Isbn), ApplyConditionTo.CurrentValidator)
            .WithMessage("ISBN має містити 10 або 13 цифр, дозволені дефіси.");

        RuleFor(r => r.Title)
            .Must(v => !string.IsNullOrWhiteSpace(v) && v.Trim().Length <= Book.MaxTitleLength)
            .WithMessage($"Назва обов'язкова, до {Book.MaxTitleLength} символів.");

        RuleFor(r => r.Author)
            .Must(v => !string.IsNullOrWhiteSpace(v) && v.Trim().Length <= Book.MaxAuthorLength)
            .WithMessage($"Автор обов'язковий, до {Book.MaxAuthorLength} символів.");

        RuleFor(r => r.Year)
            .InclusiveBetween(Book.MinYear, DateTime.Today.Year)
            .WithMessage($"Рік видання має бути від {Book.MinYear} до {DateTime.Today.Year}.");

        RuleFor(r => r.Copies)
            .InclusiveBetween(1, Book.MaxCopies)
            .WithMessage($"Кількість примірників має бути від 1 до {Book.MaxCopies}.");
    }
}
