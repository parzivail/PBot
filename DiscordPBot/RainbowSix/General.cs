using Newtonsoft.Json;

namespace DiscordPBot.RainbowSix
{
    public class General
    {
        [JsonProperty("assists")]
        public int Assists { get; set; }
        [JsonProperty("barricades_deployed")]
        public int BarricadesDeployed { get; set; }
        [JsonProperty("blind_kills")]
        public int BlindKills { get; set; }
        [JsonProperty("bullets_fired")]
        public int BulletsFired { get; set; }
        [JsonProperty("bullets_hit")]
        public int BulletsHit { get; set; }
        [JsonProperty("dbnos")]
        public int Dbnos { get; set; }
        [JsonProperty("deaths")]
        public int Deaths { get; set; }
        [JsonProperty("distance_travelled")]
        public int DistanceTravelled { get; set; }
        [JsonProperty("gadgets_destroyed")]
        public int GadgetsDestroyed { get; set; }
        [JsonProperty("headshots")]
        public int Headshots { get; set; }
        [JsonProperty("kd")]
        public float Kd { get; set; }
        [JsonProperty("kills")]
        public int Kills { get; set; }
        [JsonProperty("losses")]
        public int Losses { get; set; }
        [JsonProperty("melee_kills")]
        public int MeleeKills { get; set; }
        [JsonProperty("penetration_kills")]
        public int PenetrationKills { get; set; }
        [JsonProperty("playtime")]
        public int Playtime { get; set; }
        [JsonProperty("rappel_breaches")]
        public int RappelBreaches { get; set; }
        [JsonProperty("reinforcements_deployed")]
        public int ReinforcementsDeployed { get; set; }
        [JsonProperty("revives")]
        public int Revives { get; set; }
        [JsonProperty("suicides")]
        public int Suicides { get; set; }
        [JsonProperty("wins")]
        public int Wins { get; set; }
        [JsonProperty("wl")]
        public float Wl { get; set; }
    }
}