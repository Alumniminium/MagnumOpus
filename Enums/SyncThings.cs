namespace MagnumOpus.Enums
{
    [Flags]
    public enum SyncThings : ushort
    {
        None = 0,
        Position = 1,
        Health = 2,
        MaxHealth = 4,
        Mana = 8,
        MaxMana = 16,
        Level = 32,
        Experience = 64,
        Jump = 128,
        Walk = 256,

        All = 0b1111111111111111,
    }
}