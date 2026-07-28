using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ModLoader;

namespace InfernumMode.GlobalInstances
{
    public class ProjectileSpawnManagementSystem
    {
        private static Action<Projectile> preSyncAction;

        public static void PrepareProjectileForSpawning(Action<Projectile> a) => preSyncAction = a;

        internal static void Load()
        {
            // Signature đúng của 1.3.5.3 / tML 0.11.x
            IL.Terraria.Projectile.NewProjectile_float_float_float_float_int_int_float_int_float_float += PreSyncProjectileStuff;
        }

        internal static void Unload()
        {
            IL.Terraria.Projectile.NewProjectile_float_float_float_float_int_int_float_int_float_float -= PreSyncProjectileStuff;
        }

        private static void PreSyncProjectileStuff(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            // Đi tới chỗ gán stepSpeed = 1f (sau khi projectile đã được tạo và gán hầu hết field)
            int projectileILIndex = 0;
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchStfld<Projectile>("stepSpeed")))
            {
                InfernumMode.Instance.Logger.Warn("Projectile Initialization Manager: Could not find the step speed check variable.");
                return;
            }

            int placeToSetAction = cursor.Index;

            // Lùi lại tìm local chứa instance Projectile vừa tạo
            if (!cursor.TryGotoPrev(i => i.MatchLdloc(out projectileILIndex)))
            {
                InfernumMode.Instance.Logger.Warn("Projectile Initialization Manager: Could not find the spawned projectile's local IL index.");
                return;
            }

            // Chèn code ngay trước stfld stepSpeed
            cursor.Goto(placeToSetAction);
            cursor.Emit(OpCodes.Ldloc, projectileILIndex);
            cursor.EmitDelegate<Action<Projectile>>(projectile =>
            {
                // Gọi action (nếu có) rồi hủy luôn để không bị “dính” sang lần spawn sau
                preSyncAction?.Invoke(projectile);
                preSyncAction = null;
            });
        }
    }
}