using ArchiveB1;
using b1;
using Google.Protobuf;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace bmw_web.Services;

public sealed class AchievementPlanner
{
    private readonly ILogger<AchievementPlanner> _logger;

    public AchievementPlanner(ILogger<AchievementPlanner> logger)
    {
        _logger = logger;
    }

    private static readonly IReadOnlyDictionary<int, string> AchievementNameMap =
        new Dictionary<int, string>
        {
            [81001] = "Home is Behind",
            [81002] = "Hammer and Hew",
            [81003] = "Warring with Wolves",
            [81004] = "Absorb and Cultivate",
            [81005] = "Brew of Bravery",
            [81006] = "Slithering Snake",
            [81007] = "Handy and Hardy",
            [81008] = "Enduring Echoes",
            [81009] = "Temple of Taint",
            [81010] = "Blazing Black Wind",
            [81011] = "Creative Concoction",
            [81012] = "Cursed Clan",
            [81013] = "Cage of Claws",
            [81014] = "Sound in Stone",
            [81015] = "Death in Despair",
            [81016] = "The Stone's Secret",
            [81017] = "Oṃ Maṇi Padme Hūm",
            [81018] = "Shifting Sands",
            [81019] = "Gleams of Gold",
            [81020] = "The Tiger Family",
            [81021] = "Buried in the Sand",
            [81022] = "Precious Pills",
            [81023] = "A Great Gust",
            [81024] = "Thousand-Mile Quest",
            [81025] = "Voice Vanquished",
            [81026] = "Karma of Kang-Jin",
            [81027] = "Boundless Bitterness",
            [81028] = "Shell and Scales",
            [81029] = "Secret in the Scroll",
            [81030] = "Pound and Perfect",
            [81031] = "Guardian of Gear",
            [81032] = "Happy Harvest",
            [81033] = "Secret in Purple Cloud",
            [81034] = "Marvelous Melon",
            [81035] = "Corrupted Captains",
            [81036] = "Devoted Disciples",
            [81037] = "Matches with the Macaque",
            [81038] = "Nifty Nonsense",
            [81039] = "Mud on His Face",
            [81040] = "Gnashing Grudge",
            [81041] = "Passion Passes",
            [81042] = "The Loong Pattern",
            [81043] = "Lust and Dust",
            [81044] = "The Wayward Ways",
            [81045] = "A Family Finished",
            [81046] = "The Soaring Slash",
            [81047] = "The Cockerel Crowed",
            [81048] = "Behold the Betrayal",
            [81049] = "Always Accompanied",
            [81050] = "The Furnace Boy",
            [81051] = "Urge Unfulfilled",
            [81052] = "Seeds to Sow",
            [81053] = "Souls in the Stalks",
            [81054] = "A Willing Warrior",
            [81055] = "Scenic Seeker",
            [81056] = "Three Teams of Two",
            [81057] = "Portraits Perfected",
            [81058] = "Frost and Flame",
            [81059] = "Flaming Fury",
            [81060] = "Treasure Trove",
            [81061] = "Mei of Memory",
            [81062] = "Meet the Match",
            [81063] = "Master of Magic",
            [81064] = "Brews and Barrels",
            [81065] = "The Cloud Claimed",
            [81066] = "The Clamor of Frogs",
            [81067] = "A Curious Collection",
            [81068] = "The Five Skandhas",
            [81069] = "Medicine Meal",
            [81070] = "Treaded Tracks",
            [81071] = "Misfit with Merit",
            [81072] = "A Duel of Destiny",
            [81073] = "Page Preserver",
            [81074] = "Six Senses Secured",
            [81075] = "Full of Forms",
            [81076] = "Gourds Gathered",
            [81077] = "Brewer's Bounty",
            [81078] = "With Full Spirit",
            [81079] = "Mantled with Might",
            [81080] = "Staffs and Spears",
            [81081] = "Final Fulfillment",
        };

    private static readonly IReadOnlyDictionary<int, AchievementKnowledge> AchievementKnowledgeMap =
        new Dictionary<int, AchievementKnowledge>
        {
            [81052] = new AchievementKnowledge
            {
                TargetSource = TargetSource.AchievementRequirements,
                DisplayTitleOverride = "Seeds to Sow - Deliver all seed types to Chen Loong",
                RouteHintOverride =
                    "Turn every seed in to Chen Loong in Zodiac Village. Plant seeds are RNG drops with roughly 30-minute respawns, while boss seeds are fixed drops.",
                Targets =
                [
                    new TargetKnowledge(
                        6001,
                        "Jade Lotus Seed",
                        "Harvest Jade Lotus in Chapter 1 -> Black Wind Cave -> Cave Interior, or the lakes at Chapter 3 -> Valley of Ecstasy -> Towers of Karma / Brook of Bliss."
                    ),
                    new TargetKnowledge(
                        6002,
                        "Nine-Capped Lingzhi Seed",
                        "Dropped by Nine-Capped Lingzhi Guai at Chapter 5 -> Field of Fire -> Ashen Pass III."
                    ),
                    new TargetKnowledge(
                        6003,
                        "Monkey-Head Fungus Seed",
                        "Dropped by Fungiwoman at Chapter 4 -> Temple of Yellow Flowers -> Court of Illumination."
                    ),
                    new TargetKnowledge(
                        6004,
                        "Fragrant Jade Flower Seed",
                        "Harvest the pink flower bushes around Chapter 4 -> Village of Lanxi -> Estate of Zhu and nearby Webbed Hollow routes."
                    ),
                    new TargetKnowledge(
                        6005,
                        "Fire Bellflower Seed",
                        "Harvest Fire Bellflowers in Chapter 5 -> Woods of Ember -> Ashen Pass I."
                    ),
                    new TargetKnowledge(
                        6006,
                        "Tree Pearl Seed",
                        "Chapter 3 -> Valley of Ecstasy -> Towers of Karma: one Tree Pearl is directly behind the shrine and another is by the lake on the right."
                    ),
                    new TargetKnowledge(
                        6007,
                        "Celestial Pear Seed",
                        "Harvest the glowing pear trees at Chapter 3 -> Valley of Ecstasy -> Brook of Bliss."
                    ),
                    new TargetKnowledge(
                        6012,
                        "Licorice Seed",
                        "Harvest Licorice around Chapter 2 -> Kingdom of Sahali -> Sandgate Bound, or near Crouching Tiger Temple."
                    ),
                    new TargetKnowledge(
                        6015,
                        "Fire Date Seed",
                        "Chapter 5 -> Furnace Valley -> Valley Entrance: go forward from the shrine, drop right after the shield enemy, then follow the lava tunnel to the Fire Date trees."
                    ),
                    new TargetKnowledge(
                        6016,
                        "Millennium Ginseng Seed",
                        "Dropped by Old Ginseng Guai at Chapter 3 -> Valley of Ecstasy -> Towers of Karma."
                    ),
                    new TargetKnowledge(
                        6017,
                        "Gentian Seed",
                        "Harvest Gentian along Chapter 3 -> Valley of Ecstasy -> Forest of Felicity."
                    ),
                    new TargetKnowledge(
                        6018,
                        "Golden Lotus Seed",
                        "Farm the worm-like Golden Lotus enemies in Chapter 3 -> Pagoda Realm -> Warding Temple, or around Turtle Island."
                    ),
                ],
            },
            [81045] = new AchievementKnowledge
            {
                TargetSource = TargetSource.AchievementRequirements,
                DisplayTitleOverride = "A Family Finished - Finish both Zhu Family questlines",
                RouteHintOverride =
                    "If this is still missing, revisit the Chapter 4 Zhu Bajie quest steps across Village of Lanxi, Webbed Hollow, and Temple of Yellow Flowers until both storylines are resolved.",
                Targets =
                [
                    new TargetKnowledge(
                        3001,
                        "Zhu Family Quest Part 1",
                        "Complete the first Zhu family quest chain through the Temple of Yellow Flowers story arc."
                    ),
                    new TargetKnowledge(
                        3002,
                        "Zhu Family Quest Part 2",
                        "Complete the second Zhu family quest. Requires revisiting Chapter 4 areas and resolving the remaining family conflict."
                    ),
                ],
            },
            [81067] = new AchievementKnowledge
            {
                TargetSource = TargetSource.DecodedSaveInventory,
                DisplayTitleOverride = "A Curious Collection - Collect 36 Curios",
                RouteHintOverride =
                    "Curios are spread across every chapter. Prioritize missables first: Cat Eye Beads before Elder Jinchi, Auspicious Lantern before Captain Wise-Voice, and the Loong questline curios after getting Loong Scales. Wind Chime is the bonus extra and is not required for the 36-curio achievement.",
                Targets =
                [
                    new TargetKnowledge(16001, "Fine China Tea Bowl", "In a chest at Chapter 3 -> Pagoda Realm -> Lower Pagoda."),
                    new TargetKnowledge(16002, "Cat Eye Beads", "Dropped by Wandering Wight in Chapter 1 -> Forest of Wolves -> Front Hills. Missable after defeating Elder Jinchi."),
                    new TargetKnowledge(16003, "Agate Jar", "In a chest in Chapter 1 -> Black Wind Cave -> Cave Interior, inside the Black Wind King arena."),
                    new TargetKnowledge(16004, "Tiger Tally", "Dropped by Tiger Vanguard in the Chapter 2 secret area, Kingdom of Sahali."),
                    new TargetKnowledge(16005, "Tridacna Pendant", "Dropped by Black Loong in Chapter 2 -> Fright Cliff after using Loong Scales."),
                    new TargetKnowledge(16006, "Goldflora Hairpin", "Buy from Man-in-Stone after completing his Chapter 2 questline."),
                    new TargetKnowledge(16007, "Glazed Reliquary", "Chest behind Tiger's Acolyte in Chapter 2 -> Windrest Hamlet."),
                    new TargetKnowledge(16009, "Thunderstone", "In a chest inside New Thunderclap Temple in Chapter 3."),
                    new TargetKnowledge(16010, "Frostsprout Twig", "Dropped by Captain Lotus-Vision below Chapter 3 -> Pagoda Realm -> Upper Pagoda."),
                    new TargetKnowledge(16011, "Snow Fox Brush", "Reward for completing the Fox questline in Chapter 3."),
                    new TargetKnowledge(16012, "Maitreya's Orb", "Found in a box near Chapter 3 -> Valley of Ecstasy -> Mindfulness Cliff."),
                    new TargetKnowledge(16013, "Golden Carp", "Dropped by Yellow Loong in Chapter 4 after completing the earlier Loong fights."),
                    new TargetKnowledge(16014, "Jade Moon Rabbit", "Dropped by Zhu Bajie after the second fight in Chapter 4."),
                    new TargetKnowledge(16015, "Tablet of the Three Supreme", "In a container behind Commander Beetle at Chapter 4 -> Temple of Yellow Flower -> Forest of Ferocity."),
                    new TargetKnowledge(16016, "Preservation Orb", "Container in the back garden beyond the Venom Daoist arena in Chapter 4."),
                    new TargetKnowledge(16017, "Gold Sun Crow", "Chest in Chapter 4 secret area -> Purple Cloud Mountain -> Valley of Blooms."),
                    new TargetKnowledge(16018, "Cuo Jin-Yin Belt Hook", "Big gold chest at Chapter 4 -> Webbed Hollow -> Upper Hollow."),
                    new TargetKnowledge(16019, "Gold Button", "Defeat both shield giants at Chapter 5 -> Furnace Valley -> Rakshasa Palace."),
                    new TargetKnowledge(16020, "Flame Orb", "Dropped by the Rusty-Gold Cart after finishing the Five Element Carts questline in Chapter 5."),
                    new TargetKnowledge(16021, "Daoist's Basket of Fire and Water", "Reward for beating Bishui Golden-Eyed Beast in the Chapter 5 secret area."),
                    new TargetKnowledge(16022, "Waterward Orb", "Dropped by Jiao-Loong of Waves in Chapter 6 after unlocking cloud flight."),
                    new TargetKnowledge(16023, "Amber Prayer Beads", "Chest after Father of Stones in Chapter 5 -> Woods of Ember -> Height of Ember."),
                    new TargetKnowledge(16024, "Celestial Registry Tablet", "Container beside the gazebo in Chapter 4 secret area -> Purple Cloud Mountain -> Petalfall Hamlet."),
                    new TargetKnowledge(16025, "Tiger Tendon Belt", "Random drop from Snake Sheriff near Chapter 4 -> Court of Illumination."),
                    new TargetKnowledge(16026, "Celestial Birthstone Fragment", "Dropped by Lang-Baw-Baw in Chapter 6 after unlocking cloud flight."),
                    new TargetKnowledge(16027, "Mani Bead", "Random drop from Frozen Corpse enemies in Chapter 3, for example near Mirrormere."),
                    new TargetKnowledge(16028, "Boshan Censer", "Dropped by Red Loong in Chapter 1 after getting Loong Scales from Chapter 2."),
                    new TargetKnowledge(16029, "Back Scratcher", "Buy from any Chapter 1 shrine after finishing Chapter 2."),
                    new TargetKnowledge(16030, "Auspicious Lantern", "Kill all 9 Lantern Wardens in Pagoda Realm before defeating Captain Wise-Voice. Missable."),
                    new TargetKnowledge(16031, "Gold Spikeplate", "Dropped by the giant enemy at Chapter 3 -> Valley of Ecstasy -> Longevity Road."),
                    new TargetKnowledge(16032, "Beast Buddha", "Random drop from Dual-Blade Monk enemies, easiest at Chapter 3 -> Towers of Karma."),
                    new TargetKnowledge(16033, "Bronze Buddha Pendant", "Random drop from Wolf Guardian enemies near Chapter 3 -> Forest of Felicity."),
                    new TargetKnowledge(16034, "Thunderflame Seal", "Random drop from Thunder-Rhino Master enemies at Chapter 3 -> New Thunderclap Temple -> Temple Entrance."),
                    new TargetKnowledge(16035, "Virtuous Bamboo Engraving", "Random drop from Worm Practitioner enemies at Chapter 4 -> Temple of Yellow Flower -> Temple Entrance."),
                    new TargetKnowledge(16036, "Spine in the Sack", "Random drop from destructible standing spider cocoons in the Webbed Hollow."),
                    new TargetKnowledge(16037, "White Seashell Waist Chain", "Random drop from the bull-like enemy at Chapter 5 -> Furnace Valley -> Rakshasa Palace."),
                ],
            },
            [81078] = new AchievementKnowledge
            {
                TargetSource = TargetSource.DecodedSaveInventory,
                DisplayTitleOverride = "With Full Spirit - Collect all 27 Soaks",
                RouteHintOverride =
                    "This achievement tracks soaks, not drinks. The usual holdouts are the RNG plant soaks, Flame Mediator from Searing-Fire enemies, Graceful Orchid from Chen Loong, and the NG+ soak sold by Shen Monkey.",
                Targets =
                [
                    new TargetKnowledge(2301, "Guanyin's Willow Leaf", "Buy from Shen Monkey in New Game+."),
                    new TargetKnowledge(2302, "Flower Primes", "Buy from Shen Monkey starting in Chapter 6."),
                    new TargetKnowledge(2303, "Turtle Tear", "Defeat Apramana Bat, inspect the skeleton, then collect the dropped green tear on the shore in Chapter 3."),
                    new TargetKnowledge(2304, "Stranded Loong's Whisker", "Hidden container on the island in Chapter 3 -> Snowhill Path -> Mirrormere."),
                    new TargetKnowledge(2305, "Mount Lingtai Seedlings", "Golden turtle container on the main path from Chapter 5 -> Woods of Ember -> Camp of Seasons."),
                    new TargetKnowledge(2306, "Breath of Fire", "Dropped by Cyan Loong on Turtle Island after getting Loong Scales."),
                    new TargetKnowledge(2307, "Celestial Lotus Seeds", "Buy from Shen Monkey starting in Chapter 3."),
                    new TargetKnowledge(2308, "Undying Vine", "Random drop from Verdant Glow enemies in Chapter 4 secret area -> Purple Cloud Mountain -> Valley of Blooms."),
                    new TargetKnowledge(2309, "Tiger Relic", "Hidden in the cellar route opened after the Chapter 2 Tiger Vanguard and Stone Vanguard bosses."),
                    new TargetKnowledge(2310, "Laurel Buds", "Container in Chapter 2 -> Sandgate Village -> Village Entrance, near the big village gate."),
                    new TargetKnowledge(2311, "Sweet Ice", "Golden container at Chapter 3 -> New Thunderclap Temple -> Temple Entrance."),
                    new TargetKnowledge(2312, "Thunderbolt Horn", "Buy from Shen Monkey starting in Chapter 3."),
                    new TargetKnowledge(2313, "Deathstinger", "Dropped by the scorpion enemy near the hidden village reached from The Verdure Bridge."),
                    new TargetKnowledge(2314, "Purple-Veined Peach Pit", "One of the rewards from the five chests at Chapter 4 -> The Verdure Bridge route."),
                    new TargetKnowledge(2315, "Bee Mountain Stone", "Golden turtle container at Chapter 4 -> Temple of the Yellow Flower -> Mountain Trail."),
                    new TargetKnowledge(2316, "Iron Pellet", "Buy from Man-in-Stone after finishing his Chapter 2 questline."),
                    new TargetKnowledge(2317, "Slumbering Beetle Husk", "Golden turtle container in Emerald Hall after beating Yin-Yang Fish."),
                    new TargetKnowledge(2318, "Copper Pill", "Container on the path beyond Tiger Vanguard in Chapter 2 -> Crouching Tiger Temple."),
                    new TargetKnowledge(2319, "Goji Shoots", "Golden container in Chapter 4 -> Webbed Hollow -> Upper Hollow."),
                    new TargetKnowledge(2320, "Fruit of Dao", "Random drop from the two monks just below Chapter 4 -> Court of Illumination shrine."),
                    new TargetKnowledge(2321, "Flame Mediator", "Random drop from Searing-Fire enemies in Chapter 5 -> Field of Fire, including near Cooling Slope."),
                    new TargetKnowledge(2322, "Double-Combed Rooster Blood", "Dropped by Duskveil in the Chapter 4 secret area, Purple Cloud Mountain."),
                    new TargetKnowledge(2323, "Gall Gem", "Given automatically by Shen Monkey in Chapter 1 -> Bamboo Grove -> Marsh of White Mist."),
                    new TargetKnowledge(2324, "Graceful Orchid", "Reward from Chen Loong in Zodiac Village after turning in all 15 seed types."),
                    new TargetKnowledge(2325, "Tender Jade Lotus", "Random harvest from Lotus plants, easiest from Chapter 1 -> Black Wind Cave -> Cave Interior."),
                    new TargetKnowledge(2326, "Steel Ginseng", "Random harvest from Ginseng plants, for example at Chapter 2 -> Fright Cliff -> Squall Hideout."),
                    new TargetKnowledge(2327, "Goat Skull", "Random harvest from Licorice plants in Chapter 2 secret area -> Kingdom of Sahali -> Sandgate Bound."),
                ],
            },
            [81079] = new AchievementKnowledge
            {
                TargetSource = TargetSource.DecodedSaveInventory,
                DisplayTitleOverride = "Mantled with Might - Collect 71 Armor Pieces",
                RouteHintOverride =
                    "Most armor comes from shrine crafting, secret bosses, rare drops, and a few missables. Check uncrafted shrine sets first, then clean up Chapter 4 Venom Daoist rewards, the Chapter 5 secret area, and Chapter 6 endgame rewards.",
                Targets =
                [
                    // Serpentscale set (Ch1)
                    new TargetKnowledge(10302, "Serpentscale Battlerobe", "Chapter 1 — crafted at blacksmith / treasure."),
                    new TargetKnowledge(10303, "Serpentscale Bracers", "Chapter 1 — crafted at blacksmith / treasure."),
                    new TargetKnowledge(10304, "Serpentscale Gaiters", "Chapter 1 — crafted at blacksmith / treasure."),
                    // Pilgrim set (Ch1)
                    new TargetKnowledge(10401, "Pilgrim's Headband", "Chapter 1 — starting / quest reward."),
                    new TargetKnowledge(10402, "Pilgrim's Garb", "Chapter 1 — starting / quest reward."),
                    new TargetKnowledge(10403, "Pilgrim Wristwraps", "Chapter 1 — starting / quest reward."),
                    new TargetKnowledge(10404, "Pilgrim's Legwraps", "Chapter 1 — starting / quest reward."),
                    // Heaven's Equal set (Ch1 ultimate)
                    new TargetKnowledge(10501, "Golden Feng-Tail Crown", "Chapter 1 — complete all secret bosses or NG+ craft."),
                    new TargetKnowledge(10502, "Gold Suozi Armor", "Chapter 1 — complete all secret bosses or NG+ craft."),
                    new TargetKnowledge(10503, "Dian-Cui Loong-Soaring Bracers", "Chapter 1 — complete all secret bosses or NG+ craft."),
                    new TargetKnowledge(10504, "Lotus Silk Cloudtreaders", "Chapter 1 — complete all secret bosses or NG+ craft."),
                    // Ochre set (Ch2)
                    new TargetKnowledge(10602, "Ochre Battlerobe", "Chapter 2 — crafted / boss drops in Yellow Wind Ridge."),
                    new TargetKnowledge(10603, "Ochre Armguard", "Chapter 2 — crafted / boss drops in Yellow Wind Ridge."),
                    new TargetKnowledge(10604, "Ochre Greaves", "Chapter 2 — crafted / boss drops in Yellow Wind Ridge."),
                    // Yaksha Outrage set (Ch2)
                    new TargetKnowledge(10701, "Yaksha Mask of Outrage", "Chapter 2 — crafted at blacksmith."),
                    new TargetKnowledge(10702, "Embroidered Shirt of Outrage", "Chapter 2 — crafted at blacksmith."),
                    new TargetKnowledge(10703, "Fire Yaksha Gauntlets", "Chapter 2 — crafted at blacksmith."),
                    new TargetKnowledge(10704, "Yaksha Greaves of Outrage", "Chapter 2 — crafted at blacksmith."),
                    // Loongscale set (Ch3)
                    new TargetKnowledge(10802, "Loongscale Battlerobe", "Chapter 3 — crafted / drops in Valley of Ecstasy."),
                    new TargetKnowledge(10803, "Loongscale Armguard", "Chapter 3 — crafted / drops in Valley of Ecstasy."),
                    new TargetKnowledge(10804, "Loongscale Greaves", "Chapter 3 — crafted / drops in Valley of Ecstasy."),
                    // Ebongold set (Ch3)
                    new TargetKnowledge(10902, "Ebongold Silk Robe", "Chapter 3 — crafted / Pagoda Realm drops."),
                    new TargetKnowledge(10903, "Ebongold Armguard", "Chapter 3 — crafted / Pagoda Realm drops."),
                    new TargetKnowledge(10904, "Ebongold Gaiters", "Chapter 3 — crafted / Pagoda Realm drops."),
                    // Golden set (Ch3)
                    new TargetKnowledge(11001, "Golden Mask of Fury", "Chapter 3 — hidden boss / Pagoda Realm secret."),
                    new TargetKnowledge(11002, "Golden Embroidered Shirt", "Chapter 3 — hidden boss / Pagoda Realm secret."),
                    new TargetKnowledge(11003, "Golden Armguard", "Chapter 3 — hidden boss / Pagoda Realm secret."),
                    new TargetKnowledge(11004, "Golden Greaves", "Chapter 3 — hidden boss / Pagoda Realm secret."),
                    // Galeguard set (Ch3-4)
                    new TargetKnowledge(11201, "Galeguard Beast Mask", "Chapter 3-4 — crafted / boss drops."),
                    new TargetKnowledge(11202, "Galeguard Beastmaw Armor", "Chapter 3-4 — crafted / boss drops."),
                    new TargetKnowledge(11203, "Galeguard Bracers", "Chapter 3-4 — crafted / boss drops."),
                    new TargetKnowledge(11204, "Galeguard Greaves", "Chapter 3-4 — crafted / boss drops."),
                    // Non-Pure set (Ch4)
                    new TargetKnowledge(11301, "Non-Pure Broken Mask", "Chapter 4 — crafted / Temple of Yellow Flowers."),
                    new TargetKnowledge(11302, "Non-Pure Armor of Coiling Loong", "Chapter 4 — crafted / Temple of Yellow Flowers."),
                    new TargetKnowledge(11303, "Non-Pure Gauntlets", "Chapter 4 — crafted / Temple of Yellow Flowers."),
                    new TargetKnowledge(11304, "Non-Pure Greaves", "Chapter 4 — crafted / Temple of Yellow Flowers."),
                    // Scholar set (Ch4)
                    new TargetKnowledge(11401, "Scholar's Cloud Hat", "Chapter 4 — crafted / side quest reward."),
                    new TargetKnowledge(11402, "Scholar's Gilt Armor", "Chapter 4 — crafted / side quest reward."),
                    new TargetKnowledge(11403, "Scholar's Spiked Bracers", "Chapter 4 — crafted / side quest reward."),
                    new TargetKnowledge(11404, "Scholar's Leg Guards", "Chapter 4 — crafted / side quest reward."),
                    // Bull King set (Ch5)
                    new TargetKnowledge(11601, "Bull King's Mask", "Craft after defeating Bishui Golden-Eyed Beast in the Chapter 5 secret area. Needs Bull King's Iron Horns, so NG+ is required for the full Bull King set."),
                    new TargetKnowledge(11602, "Bull King's Shan Wen Armor", "Chapter 5 — defeat the Mighty King enemy or craft at blacksmith."),
                    new TargetKnowledge(11603, "Bull King's Bracers", "Chapter 5 — crafted at blacksmith."),
                    new TargetKnowledge(11604, "Bull King's Greaves", "Chapter 5 — crafted at blacksmith."),
                    // Bronze set (Ch5)
                    new TargetKnowledge(11701, "Bronze Monkey Mask", "Chapter 5 — crafted / boss drops."),
                    new TargetKnowledge(11702, "Bronze Brocade Battlerobe", "Chapter 5 — crafted / boss drops."),
                    new TargetKnowledge(11703, "Bronze Armguard", "Chapter 5 — crafted / boss drops."),
                    new TargetKnowledge(11704, "Bronze Buskins", "Chapter 5 — crafted / boss drops."),
                    // Iron set (Ch5-6)
                    new TargetKnowledge(11801, "Iron Horned Helm", "Chapter 5-6 — crafted with rare materials from endgame bosses."),
                    new TargetKnowledge(11802, "Iron-Tough Armor", "Chapter 5-6 — crafted with rare materials from endgame bosses."),
                    new TargetKnowledge(11803, "Iron-Tough Gauntlets", "Chapter 5-6 — crafted with rare materials from endgame bosses."),
                    new TargetKnowledge(11804, "Iron-Tough Greaves", "Chapter 5-6 — crafted with rare materials from endgame bosses."),
                    // Centipede set (Ch6)
                    new TargetKnowledge(11901, "Centipede Hat of Transcendence", "Chapter 6 — crafted / Centipede boss drops."),
                    new TargetKnowledge(11912, "Centipede Qiang-Jin Armor", "Chapter 6 — upgraded rank; craft at blacksmith using Centipede materials."),
                    new TargetKnowledge(11903, "Centipede Spiked Armguard", "Chapter 6 — crafted / Centipede boss drops."),
                    new TargetKnowledge(11904, "Centipede Greaves of Transcendence", "Chapter 6 — crafted / Centipede boss drops."),
                    // Heaven's Equal set (Endgame)
                    new TargetKnowledge(12001, "Golden Feng-Tail Crown", "Endgame — final version, unlocked after defeating all chapter bosses or NG+."),
                    new TargetKnowledge(12002, "Gold Suozi Armor", "Endgame — final version, unlocked after defeating all chapter bosses or NG+."),
                    new TargetKnowledge(12003, "Dian-Cui Loong-Soaring Bracers", "Endgame — final version, unlocked after defeating all chapter bosses or NG+."),
                    new TargetKnowledge(12004, "Lotus Silk Cloudtreaders", "Endgame — final version, unlocked after defeating all chapter bosses or NG+."),
                    // ── 17xxx Special accessories ──
                    new TargetKnowledge(17001, "Earth Spirit Cap", "Spirit accessories — from special NPC or quest reward."),
                    new TargetKnowledge(17002, "Long-Nosed Mask", "Spirit accessories — Chapter 4 or side quest reward."),
                    new TargetKnowledge(17003, "Skull of Turtle Treasure", "Spirit accessories — Chapter 3 treasure / quest."),
                    new TargetKnowledge(17004, "Mountain Delicacy Raincoat", "Spirit accessories — food-related side quest reward."),
                    new TargetKnowledge(17005, "Locust Antennae Mask", "Rare drop from the locust enemy at Chapter 4 -> Webbed Hollow -> Upper Hollow. Rest at the shrine and farm the cocoon-spawn enemy until it drops."),
                    new TargetKnowledge(17006, "White Face Mask", "Spirit accessories — Chapter 4 Temple of Yellow Flowers / Silk Nest area."),
                    new TargetKnowledge(17007, "See No Evil", "Spirit accessories — Chapter 3 meditation side quest."),
                    new TargetKnowledge(17008, "Yin-Yang Daoist Robe", "Spirit accessories — Chapter 5 or 6 quest reward."),
                    new TargetKnowledge(17009, "Venomous Armguard", "Spirit accessories — Chapter 4 spider cave or poison-themed boss drop."),
                    new TargetKnowledge(17010, "Guanyin's Prayer Beads", "Spirit accessories — Chapter 3 or endgame Buddhist-themed quest."),
                    new TargetKnowledge(17011, "Vajra Armguard", "Rare drop from the Clay Vajra enemy at Chapter 3 -> New Thunderclap Temple -> Temple Entrance."),
                ],
            },
            [81080] = new AchievementKnowledge
            {
                TargetSource = TargetSource.DecodedSaveInventory,
                DisplayTitleOverride = "Staffs and Spears - Collect all 20 Weapons",
                RouteHintOverride =
                    "Most weapons come from shrine crafting plus a few boss and quest rewards. Compare this checklist against your shrine craft list first; the usual final holdouts are the two NG+ crafts.",
                Targets =
                [
                    new TargetKnowledge(15002, "Jingubang", "Story unlock in Chapter 6 at the Water Curtain Cave."),
                    new TargetKnowledge(15003, "Twin Serpents Staff", "Chapter 1 — boss drop or crafted."),
                    new TargetKnowledge(15004, "Wind Bear Staff", "Chapter 1 — boss drop or crafted."),
                    new TargetKnowledge(15005, "Willow Wood Staff", "Chapter 1 — treasure or vendor."),
                    new TargetKnowledge(15006, "Chitin Staff", "Chapter 2 — insect boss drop or crafted."),
                    new TargetKnowledge(15007, "Visionary Centipede Staff", "Chapter 2 — defeat Hundred-Eyed Daoist or related boss."),
                    new TargetKnowledge(15008, "Cloud-Patterned Stone Staff", "Chapter 2 — treasure or boss drop."),
                    new TargetKnowledge(15009, "Rat Sage Staff", "Chapter 3 — defeat Marten Spirit or crafted."),
                    new TargetKnowledge(15010, "Loongwreathe Staff", "Chapter 3 — dragon boss drop or crafted."),
                    new TargetKnowledge(15011, "Staff of Blazing Karma", "Chapter 3 — defeat flame-themed boss."),
                    new TargetKnowledge(15012, "Spikeshaft Staff", "Chapter 4 — boss drop or treasure."),
                    new TargetKnowledge(15013, "Spider Celestial Staff", "Chapter 4 — defeat Spider bosses in Temple of Yellow Flowers / Silk Nest."),
                    new TargetKnowledge(15014, "Kang-Jin Staff", "Chapter 4 — defeat the scorpion boss or crafted."),
                    new TargetKnowledge(15015, "Golden Loong Staff", "Chapter 5 — defeat dragon-themed boss or crafted."),
                    new TargetKnowledge(15016, "Dark Iron Staff", "New Game+ craft only. Upgrade Staff of Blazing Karma after beating Bishui Golden-Eyed Beast and collecting Bull King's Iron Horns."),
                    new TargetKnowledge(15017, "Stormflash Loong Staff", "New Game+ craft at any shrine. This weapon does not unlock until you enter a new cycle."),
                    new TargetKnowledge(15018, "Adept-Spine Shooting Fuban Staff", "Chapter 5-6 — defeat the centipede boss or crafted."),
                    new TargetKnowledge(15019, "Bishui Beast Staff", "Chapter 5 — defeat the golden-eyed beast boss or crafted."),
                    new TargetKnowledge(15101, "Tri-Point Double-Edged Spear", "Chapter 6 — defeat Erlang Shen / final boss or NG+ reward."),
                    new TargetKnowledge(15102, "Chu-Bai Spear", "Craft after completing the Prisoner / Four Captains questline in Chapter 3."),
                ],
            },
            [81081] = new AchievementKnowledge
            {
                TargetSource = TargetSource.AchievementRequirements,
                DisplayTitleOverride = "Final Fulfillment - Complete All 80 Achievements",
                RouteHintOverride =
                    "Finish every other achievement first. This one unlocks automatically once the rest of the list is complete.",
                Targets = [],
            }
        };

    public async Task<AnalysisReport> AnalyzeAsync(string savePath)
    {
        if (string.IsNullOrWhiteSpace(savePath))
        {
            throw new ArgumentException("Save path cannot be empty.", nameof(savePath));
        }

        if (!File.Exists(savePath))
        {
            throw new FileNotFoundException($"Save file not found at path: {savePath}");
        }

        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Reading save file {SavePath}", savePath);

        var bytes = await File.ReadAllBytesAsync(savePath);
        _logger.LogInformation("Loaded {ByteCount} bytes from save file.", bytes.Length);
        IMessage<ArchiveFile> info = new ArchiveFile();
        info.MergeFrom(bytes);
        if (info is not ArchiveFile archiveFile)
        {
            throw new InvalidOperationException("Invalid archive protobuf payload.");
        }

        var contentBytes = archiveFile.GameArchivesDataBytes.ToByteArray();
        var data = BGW_GameArchiveMgr.DeserializeArchiveDataFromBytes<FUStBEDArchivesData>(true, contentBytes);

        var chapter = data.RoleData?.RoleCs?.Chapter?.CurChapter ?? -1;
        var mapId = data.PersistentECSData?.BPCData?.BPCPlayerRoleData?.MapId ?? -1;
        var maxMapId = data.PersistentECSData?.BPCData?.BPCPlayerRoleData?.MaxMapId ?? -1;
        var ownedIds = CollectOwnedIds(data);
        var activeRebirthPoints = data
            .PersistentECSData
            ?.BPCData
            ?.BPCRebirthPointData
            ?.ActivedRebirthPointList
            ?.Where(x => x.HasValue)
            ?.Select(x => x.Value)
            ?.Distinct()
            ?.OrderBy(x => x)
            ?.ToList() ?? new List<int>();

        var achievements =
            data.RoleData?.RoleCs?.Achievement?.Achievements?.ToList() ?? new List<AchievementOne>();
        _logger.LogInformation(
            "Decoded save context for player {PlayerName}: chapter {Chapter}, map {MapId}, achievements {AchievementCount}, owned ids {OwnedIdCount}.",
            data.RoleData?.RoleCs?.Base?.Name ?? "Unknown",
            chapter,
            mapId,
            achievements.Count,
            ownedIds.Count
        );

        var plans = new List<AchievementPlan>(achievements.Count);
        for (var index = 0; index < achievements.Count; index++)
        {
            var achievement = achievements[index];
            var config = achievement.Config ?? new AchievementConfig();
            var requirementType = config.RequirementType.ToString();
            var requiredCount = config.RequirementCount;
            var completedCount = achievement.CompleteRequirementList?.Count ?? 0;
            var isComplete = achievement.IsComplete;
            var completedRequirementIds = achievement.CompleteRequirementList?.ToList() ?? [];

            var remaining = isComplete
                ? 0
                : requiredCount > 0
                    ? Math.Max(requiredCount - completedCount, 0)
                    : 1;
            var priority = PriorityFor(requirementType);
            var context = new RouteContext(chapter, mapId, maxMapId, activeRebirthPoints.Count);
            var knowledge = GetKnowledge(config.AchievementId, completedRequirementIds, ownedIds);
            var displayTitle =
                knowledge?.DisplayTitleOverride ?? BuildTitle(config.AchievementId, requirementType);
            var routeHint = knowledge?.RouteHintOverride ?? RouteHint(requirementType, context);

            var steps = BuildStepPlan(
                requirementType,
                remaining,
                requiredCount,
                completedCount,
                context
            );
            if (knowledge is not null)
            {
                if (knowledge.Targets.Count > 0)
                {
                    var trackedTotal = knowledge.Targets.Count;
                    var trackedComplete = knowledge.Targets.Count(x => x.IsCollected);
                    steps.Add($"Tracked checklist: {trackedComplete}/{trackedTotal} collected.");
                }

                if (knowledge.MissingTargets.Count > 0)
                {
                    steps.Add("Use the Missing Item Tracker for the exact remaining checklist.");
                }
            }

            plans.Add(
                new AchievementPlan
                {
                    Index = index,
                    AchievementId = config.AchievementId,
                    DisplayTitle = displayTitle,
                    RequirementType = requirementType,
                    RequiredCount = requiredCount,
                    RequiredCountText = requiredCount > 0 ? requiredCount.ToString() : "Trigger",
                    CompletedCount = completedCount,
                    RemainingCount = remaining,
                    IsComplete = isComplete,
                    IsProgressType = config.IsProgress,
                    ResetOnNewGamePlus = config.IsResetOnGameplus,
                    CompletedRequirementIds = completedRequirementIds,
                    CompletedRequirementGuids = achievement.CompleteRequirementGuidList?.ToList() ?? [],
                    PriorityOrder = priority.order,
                    PriorityLabel = priority.label,
                    RouteHint = routeHint,
                    Steps = steps,
                    RequirementTargets = knowledge?.Targets ?? [],
                    MissingTargets = knowledge?.MissingTargets ?? [],
                }
            );
        }

        var platformPlans = plans.Where(x => x.AchievementId >= 81000).ToList();
        var selectedPlans = platformPlans.Count > 0 ? platformPlans : plans;
        var filterMode = platformPlans.Count > 0 ? "platform_only" : "all";
        var completed = selectedPlans.Count(x => x.IsComplete);
        var trackedChecklists = selectedPlans.Count(x => x.RequirementTargets.Count > 0);
        var missingTrackedItems = selectedPlans.Sum(x => x.MissingTargets.Count);

        stopwatch.Stop();
        _logger.LogInformation(
            "Built analysis report in {ElapsedMs} ms using {FilterMode} mode: {Completed}/{Total} achievements complete, {TrackedChecklists} tracked checklists, {MissingTrackedItems} missing tracked items.",
            stopwatch.ElapsedMilliseconds,
            filterMode,
            completed,
            selectedPlans.Count,
            trackedChecklists,
            missingTrackedItems
        );

        return new AnalysisReport
        {
            SavePath = savePath,
            GeneratedAtUtc = DateTime.UtcNow,
            PlayerName = data.RoleData?.RoleCs?.Base?.Name ?? "Unknown",
            PlayerLevel = data.RoleData?.RoleCs?.Base?.Level ?? 0,
            NewGamePlusCount = data.RoleData?.RoleCs?.Actor?.NewGamePlusCount ?? 0,
            CurrentChapterId = chapter,
            CurrentMapId = mapId,
            MaxMapId = maxMapId,
            ActiveRebirthPoints = activeRebirthPoints,
            RawAchievementCount = plans.Count,
            RawCompletedAchievements = plans.Count(x => x.IsComplete),
            FilterMode = filterMode,
            TotalAchievements = selectedPlans.Count,
            CompletedAchievements = completed,
            IncompleteAchievements = selectedPlans.Count - completed,
            Achievements = selectedPlans.OrderBy(x => x.AchievementId).ThenBy(x => x.Index).ToList(),
        };
    }

    private static string BuildTitle(int achievementId, string requirementType)
    {
        if (AchievementNameMap.TryGetValue(achievementId, out var name))
        {
            return name;
        }

        var objective = requirementType switch
        {
            var t when t.Contains("KillUnit", StringComparison.OrdinalIgnoreCase) => "Defeat Target(s)",
            var t when t.Contains("KillGuid", StringComparison.OrdinalIgnoreCase) => "Defeat Elite Target(s)",
            var t when t.Contains("EnterMap", StringComparison.OrdinalIgnoreCase) => "Discover Area(s)",
            var t when t.Contains("FinishTask", StringComparison.OrdinalIgnoreCase) => "Finish Quest Stage(s)",
            var t when t.Contains("GainItem", StringComparison.OrdinalIgnoreCase) => "Collect Item(s)",
            var t when t.Contains("GainEquip", StringComparison.OrdinalIgnoreCase) => "Collect Equipment",
            var t when t.Contains("GainSpell", StringComparison.OrdinalIgnoreCase) => "Acquire Spell(s)",
            var t when t.Contains("BuildArmor", StringComparison.OrdinalIgnoreCase) => "Forge Armor",
            var t when t.Contains("BuildWeapon", StringComparison.OrdinalIgnoreCase) => "Forge Weapon",
            var t when t.Contains("Alchemy", StringComparison.OrdinalIgnoreCase) => "Alchemy Milestone",
            var t when t.Contains("AchievementComplete", StringComparison.OrdinalIgnoreCase) =>
                "Meta Achievement",
            _ => requirementType,
        };

        return $"Achievement {achievementId} - {objective}";
    }

    private static string RouteHint(string requirementType, RouteContext context)
    {
        if (context.CurrentChapterId <= 0)
        {
            return "Keep progressing until shrine travel and side routes open up.";
        }

        if (
            requirementType.Contains("EnterMap", StringComparison.OrdinalIgnoreCase)
            || requirementType.Contains("FinishTask", StringComparison.OrdinalIgnoreCase)
        )
        {
            return $"Start in Chapter {context.CurrentChapterId}, then backtrack through shrine travel for side paths, secret areas, and missed NPC follow-ups.";
        }

        if (requirementType.Contains("Kill", StringComparison.OrdinalIgnoreCase))
        {
            return "Use shrine travel to sweep optional bosses and chiefs you may have skipped in each chapter.";
        }

        if (
            requirementType.Contains("GainItem", StringComparison.OrdinalIgnoreCase)
            || requirementType.Contains("GainSpell", StringComparison.OrdinalIgnoreCase)
            || requirementType.Contains("GainEquip", StringComparison.OrdinalIgnoreCase)
        )
        {
            return "Do a chapter-by-chapter cleanup pass and check shops, secret areas, side quests, and shrine crafting.";
        }

        return $"Start in Chapter {context.CurrentChapterId}, clean up side content, then rescan after each unlock.";
    }

    private static List<string> BuildStepPlan(
        string requirementType,
        int remaining,
        int requiredCount,
        int completedCount,
        RouteContext context
    )
    {
        var steps = new List<string>
        {
            requiredCount > 0
                ? $"Progress: {completedCount}/{requiredCount} done, {remaining} left."
                : $"Status: {(remaining == 0 ? "complete" : "still locked")}."
        };

        if (requirementType.Contains("Kill", StringComparison.OrdinalIgnoreCase))
        {
            steps.Add("Check side paths and secret arenas for undefeated bosses or chiefs.");
            steps.Add($"Start in Chapter {context.CurrentChapterId}, then work backward through earlier chapters.");
            return steps;
        }

        if (requirementType.Contains("EnterMap", StringComparison.OrdinalIgnoreCase))
        {
            steps.Add("Visit side routes, secret areas, and optional detours instead of only following the main path.");
            steps.Add("Use each shrine you unlock to branch out before moving on.");
            return steps;
        }

        if (
            requirementType.Contains("Task", StringComparison.OrdinalIgnoreCase)
            || requirementType.Contains("Quest", StringComparison.OrdinalIgnoreCase)
        )
        {
            steps.Add("Revisit NPC hubs and finish any follow-up dialogue or turn-ins after major bosses.");
            steps.Add("Backtrack earlier chapters for side quests that reopen later.");
            return steps;
        }

        if (requirementType.Contains("GainItem", StringComparison.OrdinalIgnoreCase))
        {
            steps.Add("Clean up unique drops, quest rewards, shops, and secret-area pickups.");
            steps.Add("Check every chapter for one-time collectibles you may have skipped.");
            return steps;
        }

        if (requirementType.Contains("GainEquip", StringComparison.OrdinalIgnoreCase))
        {
            steps.Add("Compare your checklist against shrine crafting, vendors, secret bosses, and rare drops.");
            steps.Add("Finish chapter cleanup in order so newly unlocked crafts are easy to spot.");
            return steps;
        }

        if (requirementType.Contains("GainSpell", StringComparison.OrdinalIgnoreCase))
        {
            steps.Add("Finish spell-granting encounters and any shrine or NPC unlock chains tied to them.");
            steps.Add("After each unlock, rescan so the remaining list stays accurate.");
            return steps;
        }

        if (
            requirementType.Contains("BuildArmor", StringComparison.OrdinalIgnoreCase)
            || requirementType.Contains("BuildWeapon", StringComparison.OrdinalIgnoreCase)
        )
        {
            steps.Add("Gather the missing materials from optional bosses, elites, and side content.");
            steps.Add("Craft each missing piece at the shrine smith once it unlocks.");
            return steps;
        }

        if (requirementType.Contains("Alchemy", StringComparison.OrdinalIgnoreCase))
        {
            steps.Add("Advance the recipe chain, then craft the medicine you still need.");
            steps.Add("Farm missing herbs in earlier chapters where routes are fastest.");
            return steps;
        }

        if (requirementType.Contains("AchievementComplete", StringComparison.OrdinalIgnoreCase))
        {
            steps.Add("This unlocks automatically after every other achievement is done.");
            steps.Add("Finish the remaining achievements above, then rescan once more.");
            return steps;
        }

        steps.Add("Keep clearing side content alongside the main story.");
        steps.Add("Rescan after each milestone so the checklist stays current.");
        return steps;
    }

    private static (int order, string label) PriorityFor(string requirementType)
    {
        if (
            requirementType.Contains("Pass", StringComparison.OrdinalIgnoreCase)
            || requirementType.Contains("EnterMap", StringComparison.OrdinalIgnoreCase)
            || requirementType.Contains("Kill", StringComparison.OrdinalIgnoreCase)
            || requirementType.Contains("FinishTask", StringComparison.OrdinalIgnoreCase)
        )
        {
            return (1, "High");
        }

        if (
            requirementType.Contains("GainItem", StringComparison.OrdinalIgnoreCase)
            || requirementType.Contains("GainSpell", StringComparison.OrdinalIgnoreCase)
            || requirementType.Contains("GainEquip", StringComparison.OrdinalIgnoreCase)
            || requirementType.Contains("Build", StringComparison.OrdinalIgnoreCase)
            || requirementType.Contains("Alchemy", StringComparison.OrdinalIgnoreCase)
        )
        {
            return (2, "Medium");
        }

        if (requirementType.Contains("AchievementComplete", StringComparison.OrdinalIgnoreCase))
        {
            return (4, "Meta");
        }

        return (3, "Normal");
    }

    private static HashSet<int> CollectOwnedIds(FUStBEDArchivesData data)
    {
        var owned = new HashSet<int>();
        var roleCs = data.RoleData?.RoleCs;
        var bag = GetPropertyValue(roleCs, "Bag");
        var equip = GetPropertyValue(roleCs, "Equip");

        AddIdsFromObject(owned, bag, 0);
        AddIdsFromObject(owned, GetPropertyValue(bag, "EquipList"), 0);
        AddIdsFromObject(owned, GetPropertyValue(bag, "CanActivateEquipList"), 0);
        AddIdsFromObject(owned, GetPropertyValue(bag, "Accessorylist"), 0);
        AddIdsFromObject(owned, GetPropertyValue(bag, "WearAccessory"), 0);

        AddIdsFromObject(owned, equip, 0);
        AddIdsFromObject(owned, GetPropertyValue(equip, "Accessorylist"), 0);
        AddIdsFromObject(owned, GetPropertyValue(equip, "WearAccessory"), 0);
        AddIdsFromObject(owned, GetPropertyValue(equip, "WearEquip"), 0);

        AddIdsFromObject(owned, GetPropertyValue(roleCs, "Accessorylist"), 0);
        AddIdsFromObject(owned, GetPropertyValue(roleCs, "WearAccessory"), 0);
        AddIdsFromObject(owned, GetPropertyValue(roleCs, "WearEquip"), 0);

        return owned;
    }

    private static void AddIdsFromObject(HashSet<int> owned, object? value, int depth)
    {
        if (value is null || depth > 5)
        {
            return;
        }

        if (value is string)
        {
            return;
        }

        if (TryReadPositiveInt(value, "ItemId", out var itemId))
        {
            owned.Add(itemId);
        }

        if (TryReadPositiveInt(value, "EquipId", out var equipId))
        {
            owned.Add(equipId);
        }

        if (TryReadPositiveInt(value, "OwningItemId", out var owningItemId))
        {
            owned.Add(owningItemId);
        }

        if (value is IEnumerable enumerable)
        {
            foreach (var entry in enumerable)
            {
                AddIdsFromObject(owned, entry, depth + 1);
            }

            return;
        }

        var type = value.GetType();
        if (type.IsPrimitive || type.IsEnum)
        {
            return;
        }

        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            if (!property.CanRead)
            {
                continue;
            }

            object? child;
            try
            {
                child = property.GetValue(value);
            }
            catch
            {
                continue;
            }

            AddIdsFromObject(owned, child, depth + 1);
        }
    }

    private static bool TryReadPositiveInt(object source, string propertyName, out int value)
    {
        value = 0;
        var property = source.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        if (property is null || !property.CanRead)
        {
            return false;
        }

        object? raw;
        try
        {
            raw = property.GetValue(source);
        }
        catch
        {
            return false;
        }

        switch (raw)
        {
            case int intValue when intValue > 0:
                value = intValue;
                return true;
            case long longValue when longValue > 0 && longValue <= int.MaxValue:
                value = (int)longValue;
                return true;
            case uint uintValue when uintValue > 0 && uintValue <= int.MaxValue:
                value = (int)uintValue;
                return true;
            default:
                return false;
        }
    }

    private static object? GetPropertyValue(object? source, string propertyName)
    {
        if (source is null)
        {
            return null;
        }

        var property = source.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        if (property is null || !property.CanRead)
        {
            return null;
        }

        try
        {
            return property.GetValue(source);
        }
        catch
        {
            return null;
        }
    }

    private static AchievementKnowledgeResult? GetKnowledge(
        int achievementId,
        IReadOnlyCollection<int> completedRequirementIds,
        IReadOnlyCollection<int> ownedIds
    )
    {
        if (!AchievementKnowledgeMap.TryGetValue(achievementId, out var knowledge))
        {
            return null;
        }

        var completed = knowledge.TargetSource == TargetSource.DecodedSaveInventory
            ? new HashSet<int>(ownedIds)
            : new HashSet<int>(completedRequirementIds);
        var targets = knowledge
            .Targets.Select(x => new RequirementTarget
            {
                Id = x.Id,
                Name = x.Name,
                IsCollected = completed.Contains(x.Id),
                HowToGet = x.HowToGet,
            })
            .ToList();
        var missing = targets.Where(x => !x.IsCollected).ToList();

        return new AchievementKnowledgeResult
        {
            DisplayTitleOverride = knowledge.DisplayTitleOverride,
            RouteHintOverride = knowledge.RouteHintOverride,
            Targets = targets,
            MissingTargets = missing,
        };
    }
}

public sealed class AnalyzeRequest
{
    public string SavePath { get; set; } = string.Empty;
}

public sealed record RouteContext(
    int CurrentChapterId,
    int CurrentMapId,
    int MaxMapId,
    int ActiveRebirthPointCount
);

public sealed class AnalysisReport
{
    public required string SavePath { get; init; }
    public required DateTime GeneratedAtUtc { get; init; }
    public required string PlayerName { get; init; }
    public required int PlayerLevel { get; init; }
    public required int NewGamePlusCount { get; init; }
    public required int CurrentChapterId { get; init; }
    public required int CurrentMapId { get; init; }
    public required int MaxMapId { get; init; }
    public required List<int> ActiveRebirthPoints { get; init; }
    public required int RawAchievementCount { get; init; }
    public required int RawCompletedAchievements { get; init; }
    public required string FilterMode { get; init; }
    public required int TotalAchievements { get; init; }
    public required int CompletedAchievements { get; init; }
    public required int IncompleteAchievements { get; init; }
    public required List<AchievementPlan> Achievements { get; init; }
}

public sealed class AchievementPlan
{
    public required int Index { get; init; }
    public required int AchievementId { get; init; }
    public required string DisplayTitle { get; init; }
    public required string RequirementType { get; init; }
    public required int RequiredCount { get; init; }
    public required string RequiredCountText { get; init; }
    public required int CompletedCount { get; init; }
    public required int RemainingCount { get; init; }
    public required bool IsComplete { get; init; }
    public required bool IsProgressType { get; init; }
    public required bool ResetOnNewGamePlus { get; init; }
    public required List<int> CompletedRequirementIds { get; init; }
    public required List<string> CompletedRequirementGuids { get; init; }
    public required int PriorityOrder { get; init; }
    public required string PriorityLabel { get; init; }
    public required string RouteHint { get; init; }
    public required List<string> Steps { get; init; }
    public required List<RequirementTarget> RequirementTargets { get; init; }
    public required List<RequirementTarget> MissingTargets { get; init; }
}

public sealed class RequirementTarget
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required bool IsCollected { get; init; }
    public string? HowToGet { get; init; }
}

sealed class AchievementKnowledge
{
    public TargetSource TargetSource { get; init; } = TargetSource.AchievementRequirements;
    public string? DisplayTitleOverride { get; init; }
    public string? RouteHintOverride { get; init; }
    public required List<TargetKnowledge> Targets { get; init; }
}

enum TargetSource
{
    AchievementRequirements,
    DecodedSaveInventory,
}

sealed class AchievementKnowledgeResult
{
    public string? DisplayTitleOverride { get; init; }
    public string? RouteHintOverride { get; init; }
    public required List<RequirementTarget> Targets { get; init; }
    public required List<RequirementTarget> MissingTargets { get; init; }
}

sealed record TargetKnowledge(int Id, string Name, string HowToGet);
