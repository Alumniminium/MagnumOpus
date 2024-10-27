using HerstLib.IO;
using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Enums;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Systems
{
    public sealed class MagicAttackRoutingSystem : NttSystem<MagicAttackRequestComponent, SpellBookComponent, PositionComponent>
    {
        public MagicAttackRoutingSystem() : base("Attack Router", threads: 2) { }

        public override void Update(in NTT ntt, ref MagicAttackRequestComponent atk, ref SpellBookComponent sbc, ref PositionComponent pos)
        {
            if (!sbc.Spells.TryGetValue((ushort)atk.SkillId, out var spell))
            {
                ntt.Remove<MagicAttackRequestComponent>();
                if (IsLogging)
                    FConsole.WriteLine("{ntt} tried to use skill {atk.SkillId} but doesn't have it", ntt, atk.SkillId);
                return;
            }

            if (!Collections.MagicType.TryGetValue((atk.SkillId * 10) + spell.lvl, out var magicType))
            {
                ntt.Remove<MagicAttackRequestComponent>();
                if (IsLogging)
                    FConsole.WriteLine("{ntt} tried to use skill {atk.SkillId} but it doesn't exist", ntt, atk.SkillId);
                return;
            }

            switch (magicType.ActionSort)
            {
                case 2: // heal self
                    var tcc = new TargetCollectionComponent(magicType);
                    var target = NttWorld.GetEntity(atk.TargetId);
                    tcc.Targets.Add(target);
                    ntt.Set(ref tcc);
                    break;
                case 11: // Roar
                case 5: // Circle
                    var circle = new TargetingComponent(atk.X, atk.Y, magicType, TargetingType.Circle);
                    ntt.Set(ref circle);
                    break;
                case 4: // Sector
                    var sector = new TargetingComponent(atk.X, atk.Y, magicType, TargetingType.Sector);
                    ntt.Set(ref sector);
                    break;
                case 14: // Line
                    var line = new TargetingComponent(atk.X, atk.Y, magicType, TargetingType.Line);
                    ntt.Set(ref line);
                    break;
                default:
                    break;
            }

            ntt.Remove<MagicAttackRequestComponent>();
        }
    }
}