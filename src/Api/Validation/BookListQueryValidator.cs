using Api.Contracts;
using FluentValidation;

namespace Api.Validation;

public sealed class BookListQueryValidator : AbstractValidator<BookListQuery>
{
    public const int MaxPageSize = 100;

    public BookListQueryValidator()
    {
        RuleFor(q => q.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Номер сторінки починається з 1.");

        RuleFor(q => q.PageSize)
            .InclusiveBetween(1, MaxPageSize)
            .WithMessage($"Розмір сторінки має бути від 1 до {MaxPageSize}.");
    }
}
