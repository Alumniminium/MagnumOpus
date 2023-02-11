using MagnumOpus.Components;
using MagnumOpus.ECS;
using MagnumOpus.Squiggly;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class MagicAttackRoutingSystem : NttSystem<MagicAttackRequestComponent, SpellBookComponent, PositionComponent>
    {
        public MagicAttackRoutingSystem() : base("Attack Router", threads: Environment.ProcessorCount) { }

        public override void Update(in NTT ntt, ref MagicAttackRequestComponent atk, ref SpellBookComponent sbc, ref PositionComponent pos)
        {
            if (!sbc.Spells.TryGetValue((ushort)atk.SkillId, out var spell))
            {
                ntt.Remove<MagicAttackRequestComponent>();
                return;
            }

            if (!Collections.MagicType.TryGetValue(atk.SkillId * 10 + spell.lvl, out var magicType))
            {
                ntt.Remove<MagicAttackRequestComponent>();
                return;
            }

            switch (magicType.ActionSort)
            {
                case 2: // heal self
                    var tcc = new TargetCollectionComponent(ntt.Id, magicType);
                    var target = NttWorld.GetEntityByNetId(atk.TargetId);
                    tcc.Targets.Add(target);
                    ntt.Set(ref tcc);
                    break;
                case 11: // Roar
                case 5: // Circle
                    var circle = new CircleTargetComponent(ntt.Id, atk.X, atk.Y, magicType);
                    ntt.Set(ref circle);
                    break;
                case 4: // Sector
                    var sector = new SectorTargetComponent(ntt.Id, atk.X, atk.Y, magicType);
                    ntt.Set(ref sector);
                    break;
                case 14: // Line
                    var line = new LineTargetComponent(ntt.Id, atk.X, atk.Y, magicType);
                    ntt.Set(ref line);
                    break;
                default:
                    break;
            }

            ntt.Remove<MagicAttackRequestComponent>();
        }
    }
}