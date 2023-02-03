using UT4MasterServer.Common.Enums;

namespace UT4MasterServer.Models.DTO.Responses;

public sealed class StatisticDTO
{
	public string Name { get; set; } = string.Empty;
	public long Value { get; set; }
	public string Window { get; set; } = string.Empty;
	public OwnerType OwnerType { get; set; }
}
