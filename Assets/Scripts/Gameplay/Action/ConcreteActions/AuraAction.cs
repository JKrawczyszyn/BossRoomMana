using System;
using Unity.BossRoom.Gameplay.GameplayObjects;
using Unity.BossRoom.Gameplay.GameplayObjects.Character;
using UnityEngine;

namespace Unity.BossRoom.Gameplay.Actions
{
    /// <summary>
    /// Action that performs an AoE aura effect.
    /// </summary>
    [CreateAssetMenu(menuName = "BossRoom/Actions/Aura Action")]
    public class AuraAction : Action
    {
        bool m_DidAura;

        public override bool OnStart(ServerCharacter serverCharacter)
        {
            if (IsAnyAuraRunning(serverCharacter))
            {
                return ActionConclusion.Stop;
            }

            Data.TargetIds = Array.Empty<ulong>();
            serverCharacter.serverAnimationHandler.NetworkAnimator.SetTrigger(Config.Anim);
            serverCharacter.clientCharacter.RecvDoActionClientRPC(Data);
            return ActionConclusion.Continue;
        }

        private bool IsAnyAuraRunning(ServerCharacter serverCharacter)
        {
            // check if we're already running an aura, only one aura at a time can be active
            foreach (Action action in serverCharacter.ActionPlayer.RunningActions)
            {
                if (action != this && action is AuraAction)
                {
                    return true;
                }
            }

            return false;
        }

        public override void Reset()
        {
            base.Reset();
            m_DidAura = false;
        }

        public override bool OnUpdate(ServerCharacter clientCharacter)
        {
            if (TimeRunning >= Config.ExecTimeSeconds && !m_DidAura)
            {
                // actually perform the Aura effect
                m_DidAura = true;
                PerformAura(clientCharacter);
            }

            return ActionConclusion.Continue;
        }

        private void PerformAura(ServerCharacter parent)
        {
            bool wantPcs = Config.IsFriendly ^ parent.IsNpc;
            // find all characters within the radius
            Collider[] colliders = ActionUtils.GetCollidersInSphere(wantPcs, !wantPcs, parent.transform.position, Config.Radius);
            foreach (Collider collider in colliders)
            {
                var neighbor = collider.GetComponent<IDamageable>();
                if (neighbor != null)
                {
                    // actually deal/heal the damage/mana
                    neighbor.ReceiveStat(parent, -Config.Amount, Config.StatType);
                }
            }
        }
    }
}
