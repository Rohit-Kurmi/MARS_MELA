namespace MARS_MELA_PROJECT.Models
{
    public class TradeFairUpdateVM
    {
        public traidfairupdatesearch Search { get; set; } = new();
        public UpdateTradeFairDTO Fair { get; set; } = new();
    }
}
