using MagnumOpus.IO;
using MagnumOpus.Components;
using MagnumOpus.Enums;
using MagnumOpus.Squiggly;
using NttECS.ECS;

namespace MagnumOpus.Systems
{
    public sealed class MagicAttackRoutingSystem : NttSystem<MagicAttackRequestComponent, SpellBookComponent, PositionComponent>
    {
        public MagicAttackRoutingSystem() : base("Attack Router", threads: 1, log: false) { }

        // Routes magic attack requests to appropriate targeting systems based on spell action type.
        // Validates spell availability in spellbook and magic type database, then creates targeting
        // components for different spell patterns: healing (self-target), area effects (circle/roar),
        // directional attacks (sector), and line attacks. Each spell type uses specialized targeting.
        public override void Update(in NTT ntt, ref MagicAttackRequestComponent magicAttackRequest, ref SpellBookComponent spellBook, ref PositionComponent position)
        {
            // === VALIDATE SPELL IN SPELLBOOK ===
            if (!spellBook.Spells.TryGetValue((ushort)magicAttackRequest.SkillId, out var spellData))
            {
                if (IsLogging)
                    FConsole.WriteLine("{ntt} tried to use skill {skill} but doesn't have it", ntt, magicAttackRequest.SkillId);

                ntt.Remove<MagicAttackRequestComponent>();
                return;
            }

            // === VALIDATE SPELL IN MAGIC TYPE DATABASE ===
            var magicTypeId = (magicAttackRequest.SkillId * 10) + spellData.lvl;
            if (!Collections.MagicType.TryGetValue(magicTypeId, out var magicTypeEntry))
            {
                if (IsLogging)
                    FConsole.WriteLine("{ntt} tried to use skill {skill} but magic type doesn't exist", ntt, magicAttackRequest.SkillId);

                ntt.Remove<MagicAttackRequestComponent>();
                return;
            }

            // === ROUTE TO APPROPRIATE TARGETING SYSTEM ===
            switch (magicTypeEntry.ActionSort)
            {
                case 2: // Heal self - direct target
                    var targetCollection = new TargetCollectionComponent(magicTypeEntry);
                    var targetEntity = NttWorld.GetEntity(magicAttackRequest.TargetId);
                    targetCollection.Targets.Add(targetEntity);
                    ntt.Set(ref targetCollection);
                    break;

                case 11: // Roar - area effect around caster
                case 5:  // Circle - area effect at target location
                    var circleTargeting = new TargetingComponent(magicAttackRequest.X, magicAttackRequest.Y, magicTypeEntry, TargetingType.Circle);
                    ntt.Set(ref circleTargeting);
                    break;

                case 4: // Sector - cone/wedge attack
                    var sectorTargeting = new TargetingComponent(magicAttackRequest.X, magicAttackRequest.Y, magicTypeEntry, TargetingType.Sector);
                    ntt.Set(ref sectorTargeting);
                    break;

                case 14: // Line - linear attack
                    var lineTargeting = new TargetingComponent(magicAttackRequest.X, magicAttackRequest.Y, magicTypeEntry, TargetingType.Line);
                    ntt.Set(ref lineTargeting);
                    break;

                default:
                    // Unknown action sort - no targeting needed
                    break;
            }

            // Clean up the magic attack request
            ntt.Remove<MagicAttackRequestComponent>();
        }
    }
}