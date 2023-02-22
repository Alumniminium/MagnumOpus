using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using MagnumOpus.Helpers;

namespace MagnumOpus.ECS
{
    public class NttList
    {
        public readonly int MaxEntities = 500_000;
        public int EntityCount => MaxEntities - AvailableArrayIndicies.Count;

        private readonly NTT[] Entities;
        private readonly Dictionary<int, int> NetIdToEntityIndex = new();
        private readonly ConcurrentQueue<int> AvailableArrayIndicies;
        public readonly HashSet<NTT> Players = new();

        private readonly ConcurrentQueue<NTT> ToBeRemoved = new();
        public readonly ConcurrentQueue<NTT> ChangedThisTick = new();

        public NttList()
        {
            Entities = new NTT[MaxEntities];
            AvailableArrayIndicies = new(Enumerable.Range(1, MaxEntities - 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref NTT CreateEntity(EntityType type)
        {
            lock (Entities)
            {
                if (AvailableArrayIndicies.TryDequeue(out var arrayIndex))
                {
                    var netId = IdGenerator.Get(type);
                    Entities[arrayIndex] = new NTT(arrayIndex, netId, type);
                    NetIdToEntityIndex.Add(netId, arrayIndex);
                    PrometheusPush.NTTCount.Inc();
                    return ref Entities[arrayIndex];
                }
            }
            throw new IndexOutOfRangeException("Failed to pop an array index");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref NTT CreateEntityWithNetId(EntityType type, int netId = 0)
        {
            lock (Entities)
            {
                if (AvailableArrayIndicies.TryDequeue(out var arrayIndex))
                {
                    Entities[arrayIndex] = new NTT(arrayIndex, netId, type);
                    NetIdToEntityIndex.Add(netId, arrayIndex);
                    PrometheusPush.NTTCount.Inc();
                    return ref Entities[arrayIndex];
                }
            }
            throw new IndexOutOfRangeException("Failed to pop an array index");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref NTT GetEntity(int nttId) => ref Entities[nttId];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref NTT GetEntityByNetId(int netId)
        {
            if (!NetIdToEntityIndex.TryGetValue(netId, out var index))
                return ref Entities[0];

            return ref Entities[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EntityExists(int nttId) => Entities[nttId].Id == nttId;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InformChangesFor(in NTT ntt) => ChangedThisTick.Enqueue(ntt);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Destroy(in NTT ntt) => ToBeRemoved.Enqueue(ntt);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyInternal(in NTT ntt)
        {
            lock (Entities)
            {
                AvailableArrayIndicies.Enqueue(ntt.Id);
                Players.Remove(ntt);
                ntt.Recycle();
                NetIdToEntityIndex.Remove(ntt.NetId);
                Entities[ntt.Id] = default;
                ChangedThisTick.Enqueue(ntt);
            }
            PrometheusPush.NTTCount.Dec();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateNTTs()
        {
            while (ToBeRemoved.TryDequeue(out var ntt))
                DestroyInternal(in ntt);
        }
    }
}