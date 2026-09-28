using Core.Domain;
using Core.Dto;

namespace Core.Domain;

public static class DomainBatchConverter
{
    public static (List<BookCopy> Entities, List<string> ValidationErrors) ToEntities(
        ImportResult<BookCopyDto> importResult)
    {
        var entities = new List<BookCopy>();
        var errors = new List<string>(importResult.Errors);

        foreach (var dto in importResult.Items)
        {
            try
            {
                var entity = BookCopy.FromDto(dto);
                entities.Add(entity);
            }
            catch (Exception ex)
            {
                errors.Add($"Помилка створення сутності для ID '{dto.Id}': {ex.Message}");
            }
        }

        return (entities, errors);
    }
}