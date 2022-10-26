using MagnumOpus.ECS;
using MagnumOpus.Networking;
using MagnumOpus.Simulation.Components;

namespace MagnumOpus.Simulation.Systems
{
    public sealed class ViewportSystem : PixelSystem<PositionComponent, ViewportComponent>
    {
        public ViewportSystem() : base("Viewport System", threads: 1) { }
        protected override bool MatchesFilter(in PixelEntity ntt) => ntt.Type != EntityType.Item && ntt.Type != EntityType.Npc && base.MatchesFilter(in ntt);

        public override void Update(in PixelEntity ntt, ref PositionComponent pos, ref ViewportComponent vwp)
        {
            if (pos.ChangedTick != PixelWorld.Tick)
                return;

            vwp.Viewport.X = pos.Position.X - vwp.Viewport.Width / 2;
            vwp.Viewport.Y = pos.Position.Y - vwp.Viewport.Height / 2;

            vwp.EntitiesVisibleLast.Clear();

            vwp.EntitiesVisibleLast.AddRange(vwp.EntitiesVisible);
            vwp.EntitiesVisible.Clear();

            Game.Grids[(int)pos.Position.Z].GetVisibleEntities(ref vwp);

            if (ntt.Type != EntityType.Player)
                return;

            // despawn entities not visible anymore and spawn new ones

            for (var i = 0; i < vwp.EntitiesVisibleLast.Count; i++)
            {
                var b = vwp.EntitiesVisibleLast[i];
                var found = false;
                if (ntt.Id == b.Id)
                    continue;

                for (var j = 0; j < vwp.EntitiesVisible.Count; j++)
                {
                    found = vwp.EntitiesVisible[j].Id == b.Id;
                    if (found)
                        break;
                }

                if (found)
                    continue;
            }

            for (var i = 0; i < vwp.EntitiesVisible.Count; i++)
            {
                var b = vwp.EntitiesVisible[i];
                var found = false;

                for (var j = 0; j < vwp.EntitiesVisibleLast.Count; j++)
                {
                    found = vwp.EntitiesVisibleLast[j].Id == b.Id;
                    if (found)
                        break;
                }

                if (found)
                    continue;

                NetworkHelper.FullSync(ntt, b);
            }
        }
    }
}