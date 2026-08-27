namespace ProductService.Application.Dtos;

public record PagedResultDto<T>(IEnumerable<T> Items, int TotalCount, int Page, int PageSize);
