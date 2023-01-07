using UT4MasterServer.Enums;

namespace UT4MasterServer.DTOs;

public sealed class StatisticDto
{
	public string Name { get; set; } = string.Empty;
	public int Value { get; set; }
	public string Window { get; set; } = string.Empty;
	public OwnerType OwnerType { get; set; }
}
