namespace MagnumOpus.Squiggly.Models
{
#pragma warning disable CS8618
#pragma warning disable IDE1006
    public class cq_wanted
    {
        public long id { get; set; }
        public string target_name { get; set; }
        public byte target_lev { get; set; }
        public byte target_pro { get; set; }
        public long target_syn { get; set; }
        public string payer { get; set; }
        public long? bounty { get; set; }
        public long? order_time { get; set; }
        public string hunter { get; set; }
        public long? finish_time { get; set; }
    }
}
