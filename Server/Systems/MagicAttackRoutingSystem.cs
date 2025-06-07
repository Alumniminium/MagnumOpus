using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Squiggly;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    /// <summary>
    /// Routes magic attack requests to appropriate targeting systems based on spell action type.
    /// Validates spell availability and creates targeting components for area effects, healing, and line attacks.
    /// </summary>
    public sealed class MagicAttackRoutingSystem : NttSystem<MagicAttackRequestComponent, SpellBookComponent, PositionComponent>
    {
        /// <summary>
        /// Initializes the MagicAttackRoutingSystem with limited threading for spell routing.
        /// </summary>
        public MagicAttackRoutingSystem() : base("Attack Router", threads: 1) { }

        /// <summary>
        /// Routes magic attack requests to appropriate targeting systems based on spell type and area of effect.
        /// </summary>
        /// <param name="ntt">The entity casting the spell</param>
        /// <param name="magicAttackRequest">Magic attack request component specifying the spell and target</param>
        /// <param name="spellBook">Spell book component containing available spells</param>
        /// <param name="position">Position component for spell origin point</param>
        public override void Update(in NTT ntt, ref MagicAttackRequestComponent magicAttackRequest, ref SpellBookComponent spellBook, ref PositionComponent position)
        {
            if (!spellBook.Spells.TryGetValue((ushort)magicAttackRequest.SkillId, out var spellData))
            {
                ntt.Remove<MagicAttackRequestComponent>();
                if (IsLogging)
                    FConsole.WriteLine("{ntt} tried to use skill {skillId} but doesn't have it", ntt, magicAttackRequest.SkillId);
                return;
            }

            if (!Collections.MagicType.TryGetValue((magicAttackRequest.SkillId * 10) + spellData.lvl, out var magicTypeEntry))
            {
                ntt.Remove<MagicAttackRequestComponent>();
                if (IsLogging)
                    FConsole.WriteLine("{ntt} tried to use skill {skillId} but it doesn't exist", ntt, magicAttackRequest.SkillId);
                return;
            }

            switch (magicTypeEntry.ActionSort)
            {
                case 2: // heal self
                    var targetCollection = new TargetCollectionComponent(magicTypeEntry);
                    var targetEntity = NttWorld.GetEntity(magicAttackRequest.TargetId);
                    targetCollection.Targets.Add(targetEntity);
                    ntt.Set(ref targetCollection);
                    break;
                case 11: // Roar
                case 5: // Circle
                    var circleTargeting = new TargetingComponent(magicAttackRequest.X, magicAttackRequest.Y, magicTypeEntry, TargetingType.Circle);
                    ntt.Set(ref circleTargeting);
                    break;
                case 4: // Sector
                    var sectorTargeting = new TargetingComponent(magicAttackRequest.X, magicAttackRequest.Y, magicTypeEntry, TargetingType.Sector);
                    ntt.Set(ref sectorTargeting);
                    break;
                case 14: // Line
                    var lineTargeting = new TargetingComponent(magicAttackRequest.X, magicAttackRequest.Y, magicTypeEntry, TargetingType.Line);
                    ntt.Set(ref lineTargeting);
                    break;
                default:
                    break;
            }

            ntt.Remove<MagicAttackRequestComponent>();
        }
    }
}