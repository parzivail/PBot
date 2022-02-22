using System;

namespace MinecraftCurseForge.NET
{
	[Flags]
	public enum CurseForgeRewardsTransactionType
	{
		Orders = 1,
		Awards = 2,
		Transfers = 4
	}
}