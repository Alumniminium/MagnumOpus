using System.ComponentModel.DataAnnotations;

namespace MagnumOpus.Squiggly.Models
{
#pragma warning disable CS8618
#pragma warning disable IDE1006
    public class cq_levexp
    {
        [Key]
        public long level { get; set; }
        public ulong exp { get; set; }
        public int up_lev_time { get; set; }
    }
}
