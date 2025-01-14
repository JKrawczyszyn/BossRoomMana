using System;
using Unity.BossRoom.Gameplay.GameplayObjects;
using Unity.BossRoom.Gameplay.GameplayObjects.Character;
using UnityEngine;

namespace Unity.BossRoom.Gameplay.Actions
{
    /// <summary>
    /// Area-of-effect attack Action. The attack is centered on a point provided by the client.
    /// </summary>
    [CreateAssetMenu(menuName = "BossRoom/Actions/AOE Action")]
    public class AOEAction : Action
    {
        /// <summary>
        /// Cheat prevention: to ensure that players don't perform AoEs outside of their attack range,
        /// we ensure that the target is less than Range meters away from the player, plus this "fudge
        /// factor" to accomodate miscellaneous minor movement.
        /// </summary>
        const float k_MaxDistanceDivergence = 1;

        bool m_DidAoE;


        public override bool OnStart(ServerCharacter serverCharacter)
        {
            float distanceAway = Vector3.Distance(serverCharacter.physicsWrapper.Transform.position, Data.Position);
            if (distanceAway > Config.Range + k_MaxDistanceDivergence)
            {
                // Due to latency, it's possible for the client side click check to be out of date with the server driven position. Doing a final check server side to make sure.
                return ActionConclusion.Stop;
            }

            // broadcasting to all players including myself.
            // We don't know our actual targets for this attack until it triggers, so the client can't use the TargetIds list (and we clear it out for clarity).
            // This means we are responsible for triggering reaction-anims ourselves, which we do in PerformAoe()
            Data.TargetIds = new ulong[0];
            serverCharacter.serverAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim);
            serverCharacter.clientCharacter.RecvDoActionClientRPC(Data);
            return ActionConclusion.Continue;
        }

        public override void Reset()
        {
            base.Reset();
            m_DidAoE = false;
        }

        public override bool OnUpdate(ServerCharacter clientCharacter)
        {
            if (TimeRunning >= Config.ExecTimeSeconds && !m_DidAoE)
            {
                // actually perform the AoE attack
                m_DidAoE = true;
                PerformAoE(clientCharacter);
            }

            return ActionConclusion.Continue;
        }

        private void PerformAoE(ServerCharacter parent)
        {
            var colliders = ActionUtils.GetCollidersInSphere(false, true, m_Data.Position, Config.Radius);
            foreach (Collider collider in colliders)
            {
                var enemy = collider.GetComponent<IDamageable>();
                if (enemy != null)
                {
                    // actually deal the damage
                    enemy.ReceiveStat(parent, -Config.Amount, Config.StatType);
                }
            }
        }

        public override bool OnStartClient(ClientCharacter clientCharacter)
        {
            base.OnStartClient(clientCharacter);
            GameObject.Instantiate(Config.Spawns[0], Data.Position, Quaternion.identity);
            return ActionConclusion.Stop;
        }

        public override bool OnUpdateClient(ClientCharacter clientCharacter)
        {
            throw new Exception("This should not execute");
        }
    }
}
