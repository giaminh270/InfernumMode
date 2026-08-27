using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ModLoader;
using static CalamityMod.ILEditing.ILChanges;

namespace InfernumMode.GlobalInstances
{
    public class ProjectileSpawnManagementSystem
    {
        private static Action<Projectile> preSyncAction;

        public static void PrepareProjectileForSpawning(Action<Projectile> a) => preSyncAction = a;

        internal static void Load()
        {
            IL.Terraria.Projectile.NewProjectile_float_float_float_float_int_int_float_int_float_float += PreSyncProjectileStuff;
        }

        internal static void Unload()
        {
            IL.Terraria.Projectile.NewProjectile_float_float_float_float_int_int_float_int_float_float -= PreSyncProjectileStuff;
        }

        private static void PreSyncProjectileStuff(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            // Go after the projectile instantiation phase and find the local index of the spawned projectile.
            int projectileILIndex = 0;
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchStfld<Projectile>("stepSpeed")))
            {
                InfernumMode.Instance.Logger.Warn("Projectile Initialization Manager: Could not find the step speed check variable.");
                return;
            }

            int placeToSetAction = cursor.Index;
            if (!cursor.TryGotoPrev(i => i.MatchLdloc(out projectileILIndex)))
            {
                InfernumMode.Instance.Logger.Warn("Projectile Initialization Manager: Could not find the spawned projectile's local IL index.");
                return;
            }

            cursor.Goto(placeToSetAction);
            cursor.Emit(OpCodes.Ldloc, projectileILIndex);
            cursor.EmitDelegate<Action<Projectile>>(projectile =>
            {
                // Invoke the pre-sync action and then destroy it, to ensure that the action doesn't bleed into successive, unrelated spawn calls.
                preSyncAction?.Invoke(projectile);
                preSyncAction = null;
            });
        }
    }
}