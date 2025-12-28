namespace UT4MasterServer.Models.DTO.Responses;

public sealed class PagedResponse<T>
{
	public long Count { get; set; }
	public List<T> Data { get; set; } = [];
}
