using System;

namespace MinecraftCurseForge.NET
{
	public record CurseForgeRewardsTransaction(DateTime Timestamp, int CentiPoints, CurseForgeRewardsTransactionBreakdownItem[] Breakdown);
}