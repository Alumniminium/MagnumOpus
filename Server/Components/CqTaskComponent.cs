using NttECS.ECS;
namespace MagnumOpus.Components
{
    [Component(SaveEnabled: true)]
    /// <summary>
    /// Conquer Online task/quest component managing player interaction with quest NPCs and dialogue
    /// systems. Contains NPC reference for task giver, option array for dialogue choices (max 16), 
    /// and option count for active choices. Used by CqActionProcessor for quest logic execution
    /// and MsgTaskDialog for NPC dialogue interface. Essential for quest systems, NPC interactions,
    /// and story progression mechanics.
    /// </summary>
    public struct CqTaskComponent
    {
        public NTT Npc;
        public int[] Options = new int[16];
        public byte OptionCount;

        public CqTaskComponent() => Options = new int[16];
        public CqTaskComponent(int npcId) => Npc = NttWorld.GetEntity(npcId);
    }
}