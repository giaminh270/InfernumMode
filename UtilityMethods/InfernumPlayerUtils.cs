using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace InfernumMode
{
	public static partial class InfernumUtils
	{
		public static bool InProfaned(this Player player) => player.Infernum().ZoneProfaned;
	}
}
