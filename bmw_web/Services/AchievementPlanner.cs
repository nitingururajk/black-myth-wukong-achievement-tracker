using ArchiveB1;
using b1;
using Google.Protobuf;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace bmw_web.Services;

public sealed class AchievementPlanner
{
    private static readonly string[] KnownOwnedIdPropertyNames = ["ItemId", "EquipId", "OwningItemId"];

    private static readonly string[] KnownOwnedRootPropertyNames =
    [
        "Bag",
        "Equip",
        "Accessorylist",
        "WearAccessory",
        "WearEquip",
    ];

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
            [81031] = "The Soaring Slash",
            [81032] = "Happy Harvest",
            [81033] = "Marvelous Melon",
            [81034] = "Lust and Dust",
            [81035] = "Corrupted Captains",
            [81036] = "Devoted Disciples",
            [81037] = "Matches with the Macaque",
            [81038] = "Nifty Nonsense",
            [81039] = "Mud on His Face",
            [81040] = "Gnashing Grudge",
            [81041] = "Passion Passes",
            [81042] = "The Loong Pattern",
            [81043] = "Secret in Purple Cloud",
            [81044] = "The Wayward Ways",
            [81045] = "A Family Finished",
            [81046] = "The Cockerel Crowed",
            [81047] = "Misfit with Merit",
            [81048] = "Behold the Betrayal",
            [81049] = "Always Accompanied",
            [81050] = "The Furnace Boy",
            [81051] = "Urge Unfulfilled",
            [81052] = "Seeds to Sow",
            [81053] = "Souls in the Stalks",
            [81054] = "A Willing Warrior",
            [81055] = "Scenic Seeker",
            [81056] = "Three Teams of Two",
            [81057] = "With Full Spirit",
            [81058] = "Frost and Flame",
            [81059] = "Flaming Fury",
            [81060] = "Treasure Trove",
            [81061] = "Mei of Memory",
            [81062] = "Meet the Match",
            [81063] = "Full of Forms",
            [81064] = "Brews and Barrels",
            [81065] = "The Cloud Claimed",
            [81066] = "The Clamor of Frogs",
            [81067] = "A Curious Collection",
            [81068] = "The Five Skandhas",
            [81069] = "Medicine Meal",
            [81070] = "Treaded Tracks",
            [81071] = "Guardian of Gear",
            [81072] = "A Duel of Destiny",
            [81073] = "Portraits Perfected",
            [81074] = "Six Senses Secured",
            [81075] = "Master of Magic",
            [81076] = "Gourds Gathered",
            [81077] = "Page Preserver",
            [81078] = "Brewer's Bounty",
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
                DisplayTitleOverride = "A Family Finished - Unite the Scorpion Family",
                RouteHintOverride =
                    "This achievement tracks two scorpion-family subgoals in Chapter 4. Finish both the Scorpionlord encounter and the Four Scorpion Princes chain before moving on if the game has not already marked it complete.",
                Targets =
                [
                    new TargetKnowledge(
                        3001,
                        "Scorpionlord",
                        "Defeat Scorpionlord in Chapter 4. If he does not appear, revisit the Yellow Flower routes and make sure you have not locked the encounter out."
                    ),
                    new TargetKnowledge(
                        3002,
                        "Four Chapter 4 Scorpion Enemies",
                        "Defeat the four smaller Chapter 4 scorpion-family enemies so the save records the second scorpion-family requirement."
                    ),
                ],
            },
            [81055] = new AchievementKnowledge
            {
                TargetSource = TargetSource.AchievementRequirements,
                DisplayTitleOverride = "Scenic Seeker - Find all Meditation Spots",
                RouteHintOverride =
                    "Meditation spots are one-time map pickups. Sweep each chapter in shrine-travel order: Chapter 1 has 3, Chapter 2 has 6, Chapter 3 has 5, Chapter 4 has 6, and Chapter 5 has 4.",
                Targets = BuildScenicSeekerTargets(),
            },
            [81057] = new AchievementKnowledge
            {
                TargetSource = TargetSource.AchievementRequirements,
                DisplayTitleOverride = "With Full Spirit - Collect all Spirits",
                RouteHintOverride =
                    "The save tracks this trophy through spirit-skill ownership IDs. This checklist uses the runtime spirit skill names tied to each spirit so every collected entry is still visible in the planner.",
                Targets = BuildWithFullSpiritTargets(),
            },
            [81060] = new AchievementKnowledge
            {
                TargetSource = TargetSource.AchievementRequirements,
                DisplayTitleOverride = "Treasure Trove - Collect all Vessels",
                RouteHintOverride =
                    "Only four vessels count here. Clean up the Chapter 1 secret area, Chapter 2 secret area, Chapter 4 secret area, and the Chapter 5 ending reward.",
                Targets = BuildTreasureTroveTargets(),
            },
            [81063] = new AchievementKnowledge
            {
                TargetSource = TargetSource.AchievementRequirements,
                DisplayTitleOverride = "Full of Forms - Unlock all Transformations",
                RouteHintOverride =
                    "This trophy only checks transformations. Sweep the chapter rewards first, then clean up the Purple Cloud Mountain reward, Yellow Loong, the Chapter 5 horse questline, and the Chapter 6 / secret-ending forms.",
                Targets = BuildFullOfFormsTargets(),
            },
            [81064] = new AchievementKnowledge
            {
                TargetSource = TargetSource.DecodedSaveInventory,
                DisplayTitleOverride = "Brews and Barrels - Collect all Drinks",
                RouteHintOverride =
                    "The save tracks eight non-default drinks. The starting Coconut Wine is automatic; collect the Chapter 2-5 exploration drinks and buy Pinebrew / A Thousand Days Inebriation from Shen Monkey when they unlock.",
                Targets = BuildDrinkTargets(),
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
            [81068] = new AchievementKnowledge
            {
                TargetSource = TargetSource.AchievementRequirements,
                DisplayTitleOverride = "The Five Skandhas - Gather every Skandha",
                RouteHintOverride =
                    "Find the five Skandha collectibles across Chapters 1, 2, 4, and 6, then take them to Xu Dog so the final Five Skandhas Pill is crafted and the trophy registers.",
                Targets =
                [
                    new TargetKnowledge(
                        1169,
                        "Five Skandhas Pill",
                        "Collect every Skandha piece and return them to Xu Dog so the final pill is created and the achievement can complete."
                    ),
                ],
            },
            [81069] = new AchievementKnowledge
            {
                TargetSource = TargetSource.AchievementRequirements,
                DisplayTitleOverride = "Medicine Meal - Collect all Celestial Medicines",
                RouteHintOverride =
                    "This tracker uses the achievement's own celestial-medicine IDs. The three Celestial pills require five pickups each, and the Five Skandhas Pill comes from Xu Dog after the Skandha cleanup.",
                Targets = BuildMedicineMealTargets(),
            },
            [81073] = new AchievementKnowledge
            {
                TargetSource = TargetSource.AchievementRequirements,
                DisplayTitleOverride = "Portraits Perfected - Fill every journal category",
                RouteHintOverride =
                    "Portraits Perfected completes when every journal tab is full. The game tracks four category buckets here instead of every single portrait row.",
                Targets = BuildPortraitsPerfectedTargets(),
            },
            [81075] = new AchievementKnowledge
            {
                TargetSource = TargetSource.AchievementRequirements,
                DisplayTitleOverride = "Master of Magic - Learn every spell",
                RouteHintOverride =
                    "This trophy checks the seven base spells, not the transformations. Clean up the Chapter 1-4 spell rewards first, then finish the Chapter 3 strand spells and the endgame Spell Binder unlock.",
                Targets = BuildMasterOfMagicTargets(),
            },
            [81078] = new AchievementKnowledge
            {
                TargetSource = TargetSource.AchievementRequirements,
                DisplayTitleOverride = "Brewer's Bounty - Collect all 27 Soaks",
                RouteHintOverride =
                    "Brewer's Bounty checks the 27 required soaks only. Drinks are tracked by Brews and Barrels, while gourds are tracked by Gourds Gathered. Guanyin's Willow Leaf requires New Game+.",
                Targets = BuildSoakTargets(),
            },
            [81076] = new AchievementKnowledge
            {
                TargetSource = TargetSource.DecodedSaveInventory,
                DisplayTitleOverride = "Gourds Gathered - Collect all Gourds",
                RouteHintOverride =
                    "The save tracks nine collectible gourd series in addition to the automatic Old / Supreme Gourd line. Stained Jade is missable, and Qing-Tian requires New Game+.",
                Targets = BuildGourdTargets(),
            },
            [81077] = new AchievementKnowledge
            {
                TargetSource = TargetSource.AchievementRequirements,
                DisplayTitleOverride = "Page Preserver - Collect all Medicine Formulas",
                RouteHintOverride =
                    "Formula scrolls come from merchants, chests, quest rewards, and a few late-game pickups. Compare this checklist against shrine vendors and Xu Dog first, then sweep Chapters 3-5 for the holdouts.",
                Targets = BuildPagePreserverTargets(),
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
                    new TargetKnowledge(10302, "Serpentscale Battlerobe", "Craft at a shrine after defeating Whiteclad Noble in Chapter 1."),
                    new TargetKnowledge(10303, "Serpentscale Bracers", "Craft at a shrine after defeating Whiteclad Noble in Chapter 1."),
                    new TargetKnowledge(10304, "Serpentscale Gaiters", "Craft at a shrine after defeating Whiteclad Noble in Chapter 1."),
                    // Pilgrim set (Ch1)
                    new TargetKnowledge(10401, "Pilgrim's Headband", "Craft at a shrine after reaching Guanyin Temple in Chapter 1."),
                    new TargetKnowledge(10402, "Pilgrim's Garb", "Craft at a shrine after reaching Guanyin Temple in Chapter 1."),
                    new TargetKnowledge(10403, "Pilgrim Wristwraps", "Craft at a shrine after reaching Guanyin Temple in Chapter 1."),
                    new TargetKnowledge(10404, "Pilgrim's Legwraps", "Craft at a shrine after reaching Guanyin Temple in Chapter 1."),
                    // Original Wukong set (hidden Chapter 6 chest; separate from the story set)
                    new TargetKnowledge(10501, "Golden Feng-Tail Crown (Original)", "Chapter 6 -> Foothills -> Verdant Path: use the cloud to reach the hidden golden chest on the mountain near the opening shrine."),
                    new TargetKnowledge(10502, "Gold Suozi Armor (Original)", "Found in the same hidden Chapter 6 Verdant Path chest as the other three Original Wukong pieces."),
                    new TargetKnowledge(10503, "Dian-Cui Loong-Soaring Bracers (Original)", "Found in the same hidden Chapter 6 Verdant Path chest as the other three Original Wukong pieces."),
                    new TargetKnowledge(10504, "Lotus Silk Cloudtreaders (Original)", "Found in the same hidden Chapter 6 Verdant Path chest as the other three Original Wukong pieces."),
                    // Ochre set (Ch2)
                    new TargetKnowledge(10602, "Ochre Battlerobe", "Craft at a shrine after reaching the first shrine in Chapter 3."),
                    new TargetKnowledge(10603, "Ochre Armguard", "Craft at a shrine after reaching the first shrine in Chapter 3."),
                    new TargetKnowledge(10604, "Ochre Greaves", "Craft at a shrine after reaching the first shrine in Chapter 3."),
                    // Yaksha Outrage set (Ch2)
                    new TargetKnowledge(10701, "Yaksha Mask of Outrage", "Craft at a shrine after reaching the first shrine in Chapter 6."),
                    new TargetKnowledge(10702, "Embroidered Shirt of Outrage", "Craft at a shrine after reaching the first shrine in Chapter 6."),
                    new TargetKnowledge(10703, "Fire Yaksha Gauntlets", "Craft at a shrine after reaching the first shrine in Chapter 6."),
                    new TargetKnowledge(10704, "Yaksha Greaves of Outrage", "Craft at a shrine after reaching the first shrine in Chapter 6."),
                    // Loongscale set (Ch3)
                    new TargetKnowledge(10802, "Loongscale Battlerobe", "Craft after defeating Kang-Jin Star on Turtle Island in Chapter 3."),
                    new TargetKnowledge(10803, "Loongscale Armguard", "Craft after defeating Kang-Jin Star on Turtle Island in Chapter 3."),
                    new TargetKnowledge(10804, "Loongscale Greaves", "Craft after defeating Kang-Jin Star on Turtle Island in Chapter 3."),
                    // Ebongold set (Ch3)
                    new TargetKnowledge(10902, "Ebongold Silk Robe", "Craft at a shrine after reaching the first shrine in Chapter 2."),
                    new TargetKnowledge(10903, "Ebongold Armguard", "Craft at a shrine after reaching the first shrine in Chapter 2."),
                    new TargetKnowledge(10904, "Ebongold Gaiters", "Craft at a shrine after reaching the first shrine in Chapter 2."),
                    // Golden set (Ch3)
                    new TargetKnowledge(11001, "Golden Mask of Fury", "Craft at a shrine after reaching the first shrine in Chapter 4."),
                    new TargetKnowledge(11002, "Golden Embroidered Shirt", "Craft at a shrine after reaching the first shrine in Chapter 4."),
                    new TargetKnowledge(11003, "Golden Armguard", "Craft at a shrine after reaching the first shrine in Chapter 4."),
                    new TargetKnowledge(11004, "Golden Greaves", "Craft at a shrine after reaching the first shrine in Chapter 4."),
                    // Galeguard set (Ch3-4)
                    new TargetKnowledge(11201, "Galeguard Beast Mask", "Craft after defeating Stone Vanguard in Chapter 2."),
                    new TargetKnowledge(11202, "Galeguard Beastmaw Armor", "Craft after defeating Stone Vanguard in Chapter 2."),
                    new TargetKnowledge(11203, "Galeguard Bracers", "Craft after defeating Stone Vanguard in Chapter 2."),
                    new TargetKnowledge(11204, "Galeguard Greaves", "Craft after defeating Stone Vanguard in Chapter 2."),
                    // Non-Pure set (Ch4)
                    new TargetKnowledge(11301, "Non-Pure Broken Mask", "Chapter 4 — crafted / Temple of Yellow Flowers."),
                    new TargetKnowledge(11302, "Non-Pure Armor of Coiling Loong", "Chapter 4 — crafted / Temple of Yellow Flowers."),
                    new TargetKnowledge(11303, "Non-Pure Gauntlets", "Chapter 4 — crafted / Temple of Yellow Flowers."),
                    new TargetKnowledge(11304, "Non-Pure Greaves", "Chapter 4 — crafted / Temple of Yellow Flowers."),
                    // Insect set (three Fuban crafts plus Scorpionlord armor)
                    new TargetKnowledge(11401, "Monastic Insect Hat", "Craft after defeating Fuban in the Chapter 2 secret area, Kingdom of Sahali."),
                    new TargetKnowledge(11402, "Venomous Sting Insect Armor", "Craft after defeating Scorpionlord in the Chapter 4 secret area, Purple Cloud Mountain."),
                    new TargetKnowledge(11403, "Insect Spike Bracers", "Craft after defeating Fuban in the Chapter 2 secret area, Kingdom of Sahali."),
                    new TargetKnowledge(11404, "Insect Spike Gaiters", "Craft after defeating Fuban in the Chapter 2 secret area, Kingdom of Sahali."),
                    // Bull King set (Ch5)
                    new TargetKnowledge(11601, "Bull King's Mask", "Craft after defeating Bishui Golden-Eyed Beast in the Chapter 5 secret area. Needs Bull King's Iron Horns, so NG+ is required for the full Bull King set."),
                    new TargetKnowledge(11602, "Bull King's Shan Wen Armor", "Craft after defeating Bishui Golden-Eyed Beast. The full set needs more Iron Horns than one cycle provides, so finish it in New Game+."),
                    new TargetKnowledge(11603, "Bull King's Bracers", "Craft after defeating Bishui Golden-Eyed Beast. The full set needs more Iron Horns than one cycle provides, so finish it in New Game+."),
                    new TargetKnowledge(11604, "Bull King's Greaves", "Craft after defeating Bishui Golden-Eyed Beast. The full set needs more Iron Horns than one cycle provides, so finish it in New Game+."),
                    // Bronze set (Ch5)
                    new TargetKnowledge(11701, "Bronze Monkey Mask", "Craft after defeating Elder Jinchi in the Chapter 1 secret area, Ancient Guanyin Temple."),
                    new TargetKnowledge(11702, "Bronze Brocade Battlerobe", "Craft after defeating Elder Jinchi in the Chapter 1 secret area, Ancient Guanyin Temple."),
                    new TargetKnowledge(11703, "Bronze Armguard", "Craft after defeating Elder Jinchi in the Chapter 1 secret area, Ancient Guanyin Temple."),
                    new TargetKnowledge(11704, "Bronze Buskins", "Craft after defeating Elder Jinchi in the Chapter 1 secret area, Ancient Guanyin Temple."),
                    // Iron set (Ch5-6)
                    new TargetKnowledge(11801, "Iron Horned Helm", "Craft after defeating Yin Tiger in Zodiac Village during Chapter 3."),
                    new TargetKnowledge(11802, "Iron-Tough Armor", "Craft after defeating Yin Tiger in Zodiac Village during Chapter 3."),
                    new TargetKnowledge(11803, "Iron-Tough Gauntlets", "Craft after defeating Yin Tiger in Zodiac Village during Chapter 3."),
                    new TargetKnowledge(11804, "Iron-Tough Greaves", "Craft after defeating Yin Tiger in Zodiac Village during Chapter 3."),
                    // Centipede set (Ch6)
                    new TargetKnowledge(11901, "Centipede Hat of Transcendence", "Craft after reaching the first shrine in Chapter 5."),
                    new TargetKnowledge(11912, "Centipede Qiang-Jin Armor", "Craft after reaching the first shrine in Chapter 5."),
                    new TargetKnowledge(11903, "Centipede Spiked Armguard", "Craft after reaching the first shrine in Chapter 5."),
                    new TargetKnowledge(11904, "Centipede Greaves of Transcendence", "Craft after reaching the first shrine in Chapter 5."),
                    // Heaven's Equal set (Endgame)
                    new TargetKnowledge(12001, "Golden Feng-Tail Crown", "Defeat Feng-Tail General in Chapter 6; this is the mythical story version, separate from the Original set in the hidden chest."),
                    new TargetKnowledge(12002, "Gold Suozi Armor", "Defeat Gold-Armored Rhino in Chapter 6; this is the mythical story version, separate from the Original set in the hidden chest."),
                    new TargetKnowledge(12003, "Dian-Cui Loong-Soaring Bracers", "Defeat Emerald-Armed Mantis in Chapter 6; this is the mythical story version, separate from the Original set in the hidden chest."),
                    new TargetKnowledge(12004, "Lotus Silk Cloudtreaders", "Defeat Cloudtreading Deer in Chapter 6; this is the mythical story version, separate from the Original set in the hidden chest."),
                    // Standalone pieces
                    new TargetKnowledge(17001, "Earth Spirit Cap", "Defeat Nine-Capped Lingzhi Guai near Chapter 5 -> Field of Fire -> Ashen Pass III."),
                    new TargetKnowledge(17002, "Snout Mask", "Defeat Yellow-Robed Squire during the Chapter 2 Drunken Boar questline."),
                    new TargetKnowledge(17003, "Skull of Turtle Treasure", "Rare drop from Turtle Treasure enemies in Chapter 3; farm the one near Longevity Road and rest between attempts."),
                    new TargetKnowledge(17004, "Ginseng Cape", "Defeat Old Ginseng Guai behind Chapter 3 -> Valley of Ecstasy -> Towers of Karma."),
                    new TargetKnowledge(17005, "Locust Antennae Mask", "Rare drop from the locust enemy at Chapter 4 -> Webbed Hollow -> Upper Hollow. Rest at the shrine and farm the cocoon-spawn enemy until it drops."),
                    new TargetKnowledge(17006, "Grey Wolf Mask", "Defeat Lingxuzi in Chapter 1."),
                    new TargetKnowledge(17007, "See No Evil", "Rare drop from Blind Monk enemies in Chapter 3 -> New Thunderclap Temple."),
                    new TargetKnowledge(17008, "Yin-Yang Daoist Robe", "Automatic reward for defeating Keeper of Flaming Mountains / Yin-Yang Fish in Chapter 5."),
                    new TargetKnowledge(17009, "Venomous Armguard", "During the first Venom Daoist fight in Chapter 4, break four of his extra arms before defeating him. Missable for the cycle."),
                    new TargetKnowledge(17010, "Guanyin's Prayer Beads", "Open the chest in the Chapter 1 Ancient Guanyin Temple secret area."),
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
                    new TargetKnowledge(15002, "Jingubang", "Unmissable Chapter 6 story reward at Water Curtain Cave after collecting Wukong's four armor pieces."),
                    new TargetKnowledge(15003, "Twin Serpents Staff", "Craft it after defeating Whiteclad Noble in Chapter 1."),
                    new TargetKnowledge(15004, "Wind Bear Staff", "Craft it at a Keeper's Shrine after advancing the Chapter 1 Black Wind Mountain story."),
                    new TargetKnowledge(15005, "Willow Wood Staff", "The Destined One's starting weapon; keep it in the collection as later branches are crafted."),
                    new TargetKnowledge(15006, "Chitin Staff", "Craft it after defeating the Second Sister at the start of Chapter 4."),
                    new TargetKnowledge(15007, "Visionary Centipede Staff", "Upgrade the Chitin Staff after completing Chapter 4."),
                    new TargetKnowledge(15008, "Cloud-Patterned Stone Staff", "Collect all six Buddha's Eyeballs and defeat Shigandang in Chapter 2, then craft it."),
                    new TargetKnowledge(15009, "Rat Sage Staff", "Craft it after defeating Yellow Wind Sage at the end of Chapter 2."),
                    new TargetKnowledge(15010, "Loongwreathe Staff", "Use the Chapter 2 Loong Scales to defeat Red Loong in Chapter 1, then upgrade the Twin Serpents Staff."),
                    new TargetKnowledge(15011, "Staff of Blazing Karma", "In Chapter 5, collect Samadhi Fire Crystals from Flint Chief, Flint Vanguard, and Mother of Flamlings, then craft it."),
                    new TargetKnowledge(15012, "Spikeshaft Staff", "Craft it after completing Chapter 3."),
                    new TargetKnowledge(15013, "Spider Celestial Staff", "Upgrade the Chitin Staff after defeating Violet Spider in Chapter 4."),
                    new TargetKnowledge(15014, "Kang-Jin Staff", "Craft it after defeating Kang-Jin Star on Turtle Island in Chapter 3."),
                    new TargetKnowledge(15015, "Golden Loong Staff", "Use Loong Scales to defeat Cyan Loong on Turtle Island in Chapter 3, then upgrade the Loongwreathe Staff."),
                    new TargetKnowledge(15016, "Dark Iron Staff", "New Game+ craft only. Upgrade Staff of Blazing Karma after beating Bishui Golden-Eyed Beast and collecting Bull King's Iron Horns."),
                    new TargetKnowledge(15017, "Stormflash Loong Staff", "New Game+ craft at any shrine. This weapon does not unlock until you enter a new cycle."),
                    new TargetKnowledge(15018, "Adept-Spine Shooting Fuban Staff", "New Game+ craft requiring four Sky-Piercing Horns; defeat Fuban again in the Chapter 2 secret area for the final required horn."),
                    new TargetKnowledge(15019, "Bishui Beast Staff", "Defeat Bishui Golden-Eyed Beast in the Chapter 5 secret area, then upgrade the Rat Sage Staff."),
                    new TargetKnowledge(15101, "Tri-Point Double-Edged Spear", "Defeat Erlang in Mount Mei and complete the secret-ending sequence."),
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

    private static List<TargetKnowledge> BuildDrinkTargets()
    {
        return
        [
            new TargetKnowledge(2009, "Bluebridge Romance", "Chapter 3 -> Bitter Lake -> North Shore: follow the coast back toward the temple and collect it from the lake beside the temple."),
            new TargetKnowledge(2010, "Lambbrew", "Chapter 2 -> Sandgate Village: before the Village Entrance shrine, climb the rat-archer hill and collect it from the altar at the top."),
            new TargetKnowledge(2011, "Worryfree Brew", "Chapter 4 -> Webbed Hollow -> Verdure Bridge: reach the village beyond the lantern path, break the cocoons beside the purple altar, and inspect the tea kettle."),
            new TargetKnowledge(2012, "Loong Balm", "Chapter 5 -> Furnace Valley -> Emerald Hall: after the Yin-Yang Fish fight, collect it beside the throne."),
            new TargetKnowledge(2019, "Jade Essence", "Chapter 3 -> Valley of Ecstasy -> Towers of Karma: look behind the stone pillar near the shrine."),
            new TargetKnowledge(2020, "A Thousand Days Inebriation", "Buy it from Shen Monkey once Chapter 6 has begun."),
            new TargetKnowledge(2022, "Pinebrew", "Buy it from Shen Monkey once Chapter 4 has begun."),
            new TargetKnowledge(2023, "Sunset of the Nine Skies", "Chapter 4 -> Temple of Yellow Flowers -> Court of Illumination: take the path opposite the shrine, turn right at the split, and inspect the hut."),
        ];
    }

    private static List<TargetKnowledge> BuildGourdTargets()
    {
        return
        [
            new TargetKnowledge(18007, "Xiang River Goddess Gourd", "Chapter 4 -> Webbed Hollow -> Verdure Bridge: open the chests in the room reached after the lantern-throwing cutscene."),
            new TargetKnowledge(18009, "Trailblazer's Scarlet Gourd", "Redeem the preorder reward at a shrine, or exhaust Ma Tianba's dialogue on the upper balcony near Chapter 3 -> New Thunderclap Temple -> Temple Entrance."),
            new TargetKnowledge(18011, "Qing-Tian Gourd", "Buy it from Shen Monkey in New Game+ after completing the journal."),
            new TargetKnowledge(18012, "Plaguebane Gourd", "Chapter 2: complete all three Old-Rattle Drum sites, defeat Mad Tiger in the well, then open the chest behind his arena."),
            new TargetKnowledge(18013, "Stained Jade Gourd", "Defeat Scorpionlord in Purple Cloud Mountain before Duskveil. This reward is missable for the current cycle."),
            new TargetKnowledge(18014, "Immortal Blessing Gourd", "Buy it from Shen Monkey from Chapter 5 onward after obtaining Buddha's Left Hand and Buddha's Right Hand."),
            new TargetKnowledge(18015, "Multi-Glazed Gourd", "Buy it from Shen Monkey once Chapter 6 has begun."),
            new TargetKnowledge(18016, "Jade Lotus Gourd", "Buy it from Shen Monkey once Chapter 3 has begun. Its fully upgraded form is called Jade Guanyin Gourd."),
            new TargetKnowledge(18017, "Fiery Gourd", "Chapter 3 -> Valley of Ecstasy -> Mindfulness Cliff: drop from the wooden boards and follow the lower path to the chest."),
        ];
    }

    private static List<TargetKnowledge> BuildSoakTargets()
    {
        return
        [
            new TargetKnowledge(2301, "Soak: Guanyin's Willow Leaf", "Buy from Shen Monkey in New Game+.", 81078),
            new TargetKnowledge(2302, "Soak: Flower Primes", "Buy from Shen Monkey after reaching Chapter 6.", 81078),
            new TargetKnowledge(2303, "Soak: Turtle Tear", "Chapter 3 collectible from the Bitter Lake turtle route after the North Shore sequence.", 81078),
            new TargetKnowledge(2304, "Soak: Stranded Loong's Whisker", "Hidden container on the island in Chapter 3 -> Snowhill Path -> Mirrormere.", 81078),
            new TargetKnowledge(2305, "Soak: Mount Lingtai Seedlings", "Golden container on the main path from Chapter 5 -> Woods of Ember -> Camp of Seasons.", 81078),
            new TargetKnowledge(2306, "Soak: Breath of Fire", "Reward chest after defeating Cyan Loong on the turtle island in Chapter 3 -> Bitter Lake.", 81078),
            new TargetKnowledge(2307, "Soak: Celestial Lotus Seeds", "Bought from Shen Monkey starting in Chapter 3.", 81078),
            new TargetKnowledge(2308, "Soak: Undying Vine", "Purple Cloud Mountain -> Valley of Blooms: random drop from Lushleaf enemies.", 81078),
            new TargetKnowledge(2309, "Soak: Tiger Relic", "Hidden in the cellar route opened after the Chapter 2 Tiger Vanguard and Stone Vanguard bosses.", 81078),
            new TargetKnowledge(2310, "Soak: Laurel Buds", "Container in Chapter 2 -> Sandgate Village -> Village Entrance, near the big village gate.", 81078),
            new TargetKnowledge(2311, "Soak: Sweet Ice", "Chapter 3 collectible from the New Thunderclap Temple route near the Temple Entrance area.", 81078),
            new TargetKnowledge(2312, "Soak: Thunderbolt Horn", "Bought from Shen Monkey starting in Chapter 3.", 81078),
            new TargetKnowledge(2313, "Soak: Deathstinger", "Dropped by the scorpion enemy near the hidden village reached from The Verdure Bridge.", 81078),
            new TargetKnowledge(2314, "Soak: Purple-Veined Peach Pit", "One of the rewards from the five chests at Chapter 4 -> The Verdure Bridge route.", 81078),
            new TargetKnowledge(2315, "Soak: Bee Mountain Stone", "Golden turtle container at Chapter 4 -> Temple of the Yellow Flower -> Mountain Trail.", 81078),
            new TargetKnowledge(2316, "Soak: Iron Pellet", "Purchased after the Man-in-Stone side quest.", 81078),
            new TargetKnowledge(2317, "Soak: Slumbering Beetle Husk", "Chapter 5 -> Furnace Valley -> Emerald Hall: loot the chest in the throne room side area.", 81078),
            new TargetKnowledge(2318, "Soak: Copper Pill", "Container on the path beyond Tiger Vanguard in Chapter 2 -> Crouching Tiger Temple.", 81078),
            new TargetKnowledge(2319, "Soak: Goji Shoots", "Golden container in Chapter 4 -> Webbed Hollow -> Upper Hollow.", 81078),
            new TargetKnowledge(2320, "Soak: Fruit of Dao", "Purple Cloud Mountain -> Valley of Blooms: random drop from Staff Daoist / nearby Daoist enemies.", 81078),
            new TargetKnowledge(2321, "Soak: Flame Mediator", "Chapter 5 random drop from the fire-aligned enemies around Field of Fire, especially Cooling Slope.", 81078),
            new TargetKnowledge(2322, "Soak: Double-Combed Rooster Blood", "Dropped by Duskveil in the Chapter 4 secret area, Purple Cloud Mountain.", 81078),
            new TargetKnowledge(2323, "Soak: Gall Gem", "Defeat the enemy near Shen Monkey in Chapter 1 -> Bamboo Grove -> Marsh of White Mist.", 81078),
            new TargetKnowledge(2324, "Soak: Graceful Orchid", "Reward from Chen Loong in Zodiac Village after the 12 achievement-tracked seed items have filled all 15 garden entries.", 81078),
            new TargetKnowledge(2325, "Soak: Tender Jade Lotus", "Random harvest from Lotus plants, easiest from Chapter 1 -> Black Wind Cave -> Cave Interior.", 81078),
            new TargetKnowledge(2326, "Soak: Steel Ginseng", "Random harvest from Ginseng plants, for example at Chapter 2 -> Fright Cliff -> Squall Hideout.", 81078),
            new TargetKnowledge(2327, "Soak: Goat Skull", "Random harvest from Licorice plants in Chapter 2, for example around Yellow Wind Ridge / Fright Cliff.", 81078),
        ];
    }

    private static List<TargetKnowledge> BuildScenicSeekerTargets()
    {
        return
        [
            new TargetKnowledge(1006, "The Arbor, Forest of Wolves", "Chapter 1 meditation spot in the Forest of Wolves route."),
            new TargetKnowledge(1007, "The Cavern, Bamboo Grove", "Chapter 1 meditation spot in the Bamboo Grove cave route."),
            new TargetKnowledge(1004, "The Cliff, Black Wind Cave", "Chapter 1 meditation spot near the Black Wind Cave cliff path."),
            new TargetKnowledge(2001, "The Ravine, Rock Clash Platform", "Chapter 2 meditation spot near Rock Clash Platform."),
            new TargetKnowledge(2002, "The Altar, Sandgate Village", "Chapter 2 meditation spot in Sandgate Village."),
            new TargetKnowledge(2003, "The Grotto, Yellow Wind Formation", "Chapter 2 meditation spot in Yellow Wind Formation."),
            new TargetKnowledge(2004, "The Sculpture, Crouching Tiger Temple", "Chapter 2 meditation spot by the Crouching Tiger Temple sculpture."),
            new TargetKnowledge(2005, "The Deadwood, Rockrest Flat", "Chapter 2 meditation spot near the dead tree at Rockrest Flat."),
            new TargetKnowledge(2006, "The Rock, Sandgate Bound", "Chapter 2 meditation spot at Sandgate Bound."),
            new TargetKnowledge(3001, "The Shade, Mirrormere", "Chapter 3 meditation spot at Mirrormere."),
            new TargetKnowledge(3002, "The Bottom, Pagoda Realm", "Chapter 3 meditation spot in Pagoda Realm."),
            new TargetKnowledge(3003, "The Statue, Precept Corridor", "Chapter 3 meditation spot at Precept Corridor."),
            new TargetKnowledge(3004, "The Track, Mindfulness Cliff", "Chapter 3 meditation spot near Mindfulness Cliff."),
            new TargetKnowledge(3005, "The Hall, New Thunderclap Temple", "Chapter 3 meditation spot inside New Thunderclap Temple."),
            new TargetKnowledge(4001, "The Carvings, Pool of Shattered Jade", "Chapter 4 meditation spot at the Pool of Shattered Jade."),
            new TargetKnowledge(4002, "The Tree, Middle Hollow", "Chapter 4 meditation spot in Middle Hollow."),
            new TargetKnowledge(4003, "Cave Depths, Lower Hollow", "Chapter 4 meditation spot deep in Lower Hollow."),
            new TargetKnowledge(4004, "The Height, Forest of Ferocity", "Chapter 4 meditation spot in the Forest of Ferocity route."),
            new TargetKnowledge(4005, "The Pines, Temple of Yellow Flowers", "Chapter 4 meditation spot around the Temple of Yellow Flowers."),
            new TargetKnowledge(4006, "The Ledge, Purple Cloud Mountain", "Chapter 4 meditation spot on the Purple Cloud Mountain ledge."),
            new TargetKnowledge(5001, "The Buddha, Emerald Hall", "Chapter 5 meditation spot in Emerald Hall."),
            new TargetKnowledge(5002, "The Relief, Camp of Seasons", "Chapter 5 meditation spot on the Camp of Seasons route."),
            new TargetKnowledge(5003, "The Crag, Ashen Pass III", "Chapter 5 meditation spot near Ashen Pass III."),
            new TargetKnowledge(5004, "The Screen, Purge Pit", "Chapter 5 meditation spot at Purge Pit."),
        ];
    }

    private static List<TargetKnowledge> BuildWithFullSpiritTargets()
    {
        return
        [
            SpiritTarget(8011, "Guangmou", "Chapter 1 -> Bamboo Grove -> Snake Trail: cross the river into the bamboo arena, defeat Guangmou, and absorb the flame."),
            SpiritTarget(8012, "Baw-Li-Guhh-Lang", "Chapter 1 -> Bamboo Grove -> Snake Trail: follow the lower river to the frog arena, defeat it, and absorb the flame."),
            SpiritTarget(8013, "Wandering Wight", "Chapter 1 -> Forest of Wolves -> Front Hills. Collect this before defeating Elder Jinchi or it disappears until New Game+."),
            SpiritTarget(8061, "Wolf Assassin", "Chapter 1 -> Black Wind Cave -> Outside the Cave: drop into the chest side area after the archer bridge and defeat the blue-glowing wolf."),
            SpiritTarget(8014, "Rat Governor", "Defeat and absorb the blue-flame Rat Governor while clearing Chapter 2's Yellow Wind Ridge side routes."),
            SpiritTarget(8015, "Gore-Eye Daoist", "Defeat and absorb Gore-Eye Daoist near the Chapter 2 Fright Cliff / Rockrest Flat route."),
            SpiritTarget(8017, "Tiger's Acolyte", "Defeat Tiger's Acolyte on the bridge beyond Chapter 2 -> Windrest Hamlet and absorb the flame."),
            SpiritTarget(8062, "Mad Tiger", "Complete the three Old-Rattle Drum sites, enter the Chapter 2 village well, defeat Mad Tiger, and absorb the flame."),
            SpiritTarget(8063, "Second Rat Prince", "At the Sandgate Village dual-boss fight, defeat King of Flowing Sands first, then the Second Rat Prince so his Spirit drops."),
            SpiritTarget(8064, "Rat Imperial Guard", "Defeat and absorb the blue-flame Rat Imperial Guard in Chapter 2."),
            SpiritTarget(8065, "Spearbone", "Chapter 2 -> Fright Cliff -> Rockrest Flat: take the uphill path beside the shrine and defeat the blue-glowing shield wielder."),
            SpiritTarget(8066, "Civet Sergeant", "Defeat and absorb the blue-flame Civet Sergeant while clearing Chapter 2."),
            SpiritTarget(8067, "Swift Bat", "Chapter 2 -> Sandgate Village -> Valley of Despair: cross the broken pillar in the cave and squeeze through the wall to the blue-flame bat."),
            SpiritTarget(8068, "Poisestone", "Chapter 2 -> Fright Cliff -> Squall Hideout: follow the cave path toward Rockrest Flat and defeat the blue-glowing Poisestone."),
            SpiritTarget(8069, "Rat Archer", "Open Sandgate Village's main gate from behind after Earth Wolf; defeat the blue-glowing archer that falls from the gate."),
            SpiritTarget(8070, "Earth Wolf", "Chapter 2 -> Sandgate Village -> Village Entrance: enter the enclosed village meadow and defeat Earth Wolf."),
            SpiritTarget(8020, "Non-Void", "Complete the Chapter 3 Snow Fox quest at New Thunderclap Temple, defeat Non-Void, and absorb the flame."),
            SpiritTarget(8022, "Apramana Bat", "Defeat Apramana Bat at Bitter Lake's north shore before Zhu Bajie leaves; the Spirit is awarded after finishing Chapter 3."),
            SpiritTarget(8024, "Non-Pure", "Defeat Non-Pure in New Thunderclap Temple during Chapter 3 and absorb the flame."),
            SpiritTarget(8025, "Non-White", "Defeat the story encounter at Chapter 3 -> Valley of Ecstasy -> Mindfulness Cliff and absorb the flame."),
            SpiritTarget(8026, "Non-Able", "Chapter 3 -> Valley of Ecstasy -> Brook of Bliss: climb the right slope, circle around, and defeat Non-Able."),
            SpiritTarget(8071, "Falcon Hermit", "Chapter 3 -> Pagoda Realm -> Snow-Veiled Trail: climb the long left slope to the blue-glowing bird at the summit."),
            SpiritTarget(8072, "Red-Haired Yaksha", "Defeat the blue-flame enemy on the linear route after Chapter 3 -> Bitter Lake -> Precept Corridor."),
            SpiritTarget(8073, "Blade Monk", "Chapter 3 -> Pagoda Realm -> Outside the Wheel: drop one level at the far cliff and follow it to the blue-flame monk."),
            SpiritTarget(8074, "Clay Vajra", "Defeat and absorb a blue-flame Clay Vajra in Chapter 3 -> New Thunderclap Temple."),
            SpiritTarget(8075, "Enslaved Yaksha", "After defeating Captain Lotus-Vision, open the prison gates in Upper Pagoda and defeat the blue-flame Enslaved Yaksha."),
            SpiritTarget(8076, "Mountain Patroller", "Chapter 3 -> Snowhill Path -> Frost-Clad Path: climb the left stairs in the first building area and clear the enemies at the path's end."),
            SpiritTarget(8077, "Crow Diviner", "Defeat and absorb the blue-flame Crow Diviner while clearing Chapter 3's Valley of Ecstasy routes."),
            SpiritTarget(8027, "Commander Beetle", "Defeat and absorb Commander Beetle in Chapter 4 -> Temple of Yellow Flowers."),
            SpiritTarget(8028, "Puppet Tick", "Defeat and absorb the blue-flame Puppet Tick while sweeping Chapter 4's Webbed Hollow."),
            SpiritTarget(8029, "Verdant Glow", "Purple Cloud Mountain -> Valley of Blooms: drop into the water left of the shrine and defeat the large blue-flame tree enemy."),
            SpiritTarget(8030, "Scorpion Prince", "Chapter 4 -> Verdure Bridge: reach the village beyond the lantern path and defeat the blue-glowing scorpion near its entrance."),
            SpiritTarget(8031, "Centipede Guai", "Defeat Centipede Guai on the Chapter 4 story route and absorb the Spirit flame."),
            SpiritTarget(8078, "Beetle Captain", "Chapter 4 -> Webbed Hollow -> Upper Hollow: after the second green-arrow archer, enter the cave on the right and defeat the blue-flame beetle."),
            SpiritTarget(8079, "Dragonfly Guai", "Defeat and absorb the blue-flame Dragonfly Guai while clearing Chapter 4's Webbed Hollow routes."),
            SpiritTarget(8081, "Puppet Spider", "Defeat and absorb the blue-flame Puppet Spider while clearing Chapter 4's Webbed Hollow routes."),
            SpiritTarget(8083, "Snake Sheriff", "Chapter 4 -> Temple of Yellow Flowers -> Temple Entrance: pass through the first gate and defeat the blue-glowing long-necked enemy."),
            SpiritTarget(8084, "Snake Herbalist", "Purple Cloud Mountain -> Valley of Blooms: cross the bridge, turn left, and defeat the blue-flame herbalist on the path."),
            SpiritTarget(8085, "Lantern Holder", "After defeating Second Sister, return to Chapter 4 -> Estate of the Zhu and defeat the blue-glowing lantern enemy in her arena."),
            SpiritTarget(8032, "Top Takes Bottom, Bottom Takes Top", "After their Bishui Cave fight, return beyond Emerald Hall and interact with the steel ball embedded in the rock."),
            SpiritTarget(8033, "Father of Stones", "Defeat Father of Stones along the Chapter 5 Woods of Ember route and absorb the Spirit."),
            SpiritTarget(8034, "Earth Rakshasa", "Chapter 5 -> Furnace Valley -> Valley Entrance: take the left side path before Cloudy Mist and defeat the shield enemy guarding a chest."),
            SpiritTarget(8035, "Turtle Treasure", "Defeat and absorb the blue-flame Turtle Treasure in Chapter 5."),
            SpiritTarget(8036, "Flint Chief", "Defeat and absorb Flint Chief in Chapter 5; its Samadhi Fire Crystal is also needed for Staff of Blazing Karma."),
            SpiritTarget(8037, "Flint Vanguard", "Defeat Flint Vanguard near Chapter 5 -> Field of Fire -> Fallen Furnace Crater and absorb the flame."),
            SpiritTarget(8086, "Charface", "Defeat and absorb the blue-flame Charface while clearing Chapter 5."),
            SpiritTarget(8087, "Bull Governor", "Defeat and absorb the blue-flame Bull Governor while clearing Chapter 5."),
            SpiritTarget(8088, "Cloudy Mist, Misty Cloud", "Defeat the required paired boss on Chapter 5 -> Furnace Valley -> Valley Entrance; absorb the shared Spirit."),
            SpiritTarget(8038, "Elder Armourworm", "Give Proto-Armourworm to Chen Loong, feed it three Rice Cocoons with rests between, then collect its Spirit in Zodiac Village."),
            SpiritTarget(8039, "Mother of Flamlings", "Offer four Flame Ore at the Chapter 5 summoning spot, defeat Mother of Flamlings, and absorb the Spirit."),
            SpiritTarget(8040, "Old Ginseng Guai", "Chapter 3 -> Valley of Ecstasy -> Towers of Karma: harvest the large ginseng plant behind the shrine and defeat the boss."),
            SpiritTarget(8041, "Fungiwoman", "Chapter 4 -> Court of Illumination: pull the mushroom on the right-hand path beyond the shrine and defeat the hidden boss."),
            SpiritTarget(8042, "Fungiman", "Chapter 3 -> Pagoda Realm -> Upper Pagoda: cross the beam, enter the first left gate, and pull the mushroom from the ground."),
            SpiritTarget(8092, "Nine-Capped Lingzhi Guai", "Chapter 5 -> Field of Fire -> Ashen Pass III: interact with the large lingzhi plant and defeat the hidden boss."),
        ];
    }

    private static List<TargetKnowledge> BuildTreasureTroveTargets()
    {
        return
        [
            new TargetKnowledge(19001, "Fireproof Mantle", "Chapter 1 vessel from the Ancient Guanyin Temple secret area."),
            new TargetKnowledge(19002, "Wind Tamer", "Chapter 2 vessel from the Kingdom of Sahali secret area."),
            new TargetKnowledge(19004, "Weaver's Needle", "Chapter 4 vessel from Purple Cloud Mountain."),
            new TargetKnowledge(19005, "Plantain Fan", "Chapter 5 vessel reward after finishing the main story arc there."),
        ];
    }

    private static List<TargetKnowledge> BuildFullOfFormsTargets()
    {
        return
        [
            new TargetKnowledge(5001, "Red Tides", "Defeat Guangzhi at Chapter 1 -> Forest of Wolves -> Outside the Forest and take his weapon."),
            new TargetKnowledge(5004, "Azure Dust", "Finish Man-in-Stone's Chapter 2 quest, defeat him, rest, then buy the Azure Dust item from his shop."),
            new TargetKnowledge(5006, "Ashen Slumber", "Set up the Two-Headed Rat dialogue in Chapter 2, then finish the Third Prince prison route in Chapter 3. Sequence-sensitive."),
            new TargetKnowledge(5008, "Ebon Flow", "Unlock Zodiac Village in Chapter 3, challenge Yin Tiger, and defeat him."),
            new TargetKnowledge(5014, "Hoarfrost", "Defeat Monk from the Sea near Chapter 3 -> New Thunderclap Temple -> Temple Entrance."),
            new TargetKnowledge(5016, "Umbral Abyss", "Automatic Chapter 3 reward after the final Macaque Chief encounter."),
            new TargetKnowledge(5017, "Violet Hail", "Complete Daoist Mi's quest in Purple Cloud Mountain before defeating Duskveil. Missable for the cycle."),
            new TargetKnowledge(5018, "Golden Lining", "Defeat Red, Black, and Cyan Loong, then defeat Yellow Loong in Chapter 4."),
            new TargetKnowledge(5019, "Dark Thunder", "Complete every Ma Tianba meeting from Chapters 1-5, then return to the Chapter 5 cart after finishing the chapter. Missable."),
            new TargetKnowledge(5024, "Azure Dome", "Defeat Erlang and the Four Heavenly Kings on the secret-ending route."),
        ];
    }

    private static List<TargetKnowledge> BuildMedicineMealTargets()
    {
        return
        [
            new TargetKnowledge(1003, "Celestial Jade Lotus Pill", "Collect all 5 Health upgrade pills."),
            new TargetKnowledge(1004, "Celestial Taiyi Pill", "Collect all 5 Mana upgrade pills."),
            new TargetKnowledge(1005, "Celestial Nonary Pill", "Collect all 5 Stamina upgrade pills."),
            new TargetKnowledge(1169, "Five Skandhas Pill", "Crafted by Xu Dog after the full Skandha cleanup."),
        ];
    }

    private static List<TargetKnowledge> BuildPortraitsPerfectedTargets()
    {
        return
        [
            new TargetKnowledge(7401, "Characters", "Fill the Characters tab in the journal."),
            new TargetKnowledge(7402, "Yaoguais", "Fill the Yaoguais tab in the journal."),
            new TargetKnowledge(7403, "Chiefs", "Fill the Chiefs tab in the journal."),
            new TargetKnowledge(7404, "Kings", "Fill the Kings tab in the journal."),
        ];
    }

    private static List<TargetKnowledge> BuildMasterOfMagicTargets()
    {
        return
        [
            new TargetKnowledge(5101, "Immobilize", "Automatic early Chapter 1 story spell during the Bullguard encounter."),
            new TargetKnowledge(5102, "Ring of Fire", "Automatic Chapter 3 story spell after the Macaque Chief encounter at Warding Temple."),
            new TargetKnowledge(5103, "Spell Binder", "Complete the Chapter 3 Treasure Hunter quest and defeat Green-Capped Martialist at Melon Field."),
            new TargetKnowledge(5201, "Cloud Step", "Automatic Chapter 1 story spell after defeating Black Wind King."),
            new TargetKnowledge(5202, "Rock Solid", "Automatic Chapter 2 story spell after defeating Tiger Vanguard."),
            new TargetKnowledge(5301, "A Pluck of Many", "Automatic Chapter 2 story spell after reaching Windseal Gate."),
            new TargetKnowledge(5302, "Life-Saving Strand", "Granted only after entering New Game+."),
        ];
    }

    private static List<TargetKnowledge> BuildPagePreserverTargets()
    {
        return
        [
            new TargetKnowledge(1107, "Body-Fleeting Powder", "Chapter 3 -> New Thunderclap Temple -> Temple Entrance: cross the upper wooden bridge near Ma Tianba and open the golden container."),
            new TargetKnowledge(1110, "Septenary Heartfire Pill", "Chapter 5 -> Bishui Cave -> Purge Pit: jump-heavy-attack onto the ledge behind the shrine and open the chest."),
            new TargetKnowledge(1111, "Life-Saving Pill", "Chapter 4 -> Purple Cloud Mountain -> Cloudnest Peak: open the small container to the right of the shrine."),
            new TargetKnowledge(1113, "Ascension Powder", "Automatic reward for defeating Supreme Inspector at the start of Chapter 6."),
            new TargetKnowledge(1115, "Soul Remigration Pill", "Buy the formula from Xu Dog in New Game+."),
            new TargetKnowledge(1118, "Essence Decoction", "Buy the formula from Xu Dog from the start of Chapter 4."),
            new TargetKnowledge(1121, "Tonifying Decoction", "Buy the formula from Xu Dog from the start of Chapter 3."),
            new TargetKnowledge(1130, "Longevity Decoction", "Buy the formula from Xu Dog from the start of Chapter 2."),
            new TargetKnowledge(1134, "Fortifying Medicament", "Receive it from Xu Dog while fetching Chen Loong's Special-Made Bone-Strengthening Pellet in Chapter 3."),
            new TargetKnowledge(1136, "Mirage Pill", "Chapter 4 -> Webbed Hollow -> Verdure Bridge: cross the vine bridge, drop at the cliff, pass the cocoons, and reach the village chest."),
            new TargetKnowledge(1142, "Loong Aura Amplification Pellets", "Buy the formula from Xu Dog from the start of Chapter 5."),
            new TargetKnowledge(1144, "Evil Repelling Medicament", "Defeat Lang-Li-Guhh-Baw in the ravine below Chapter 2 -> Sandgate Village -> Village Entrance."),
            new TargetKnowledge(1166, "Enhanced Ginseng Pellets", "Chapter 4 -> Webbed Hollow -> Verdure Bridge: cross the vine bridge, stay left, and open the chest in the side room."),
            new TargetKnowledge(1168, "Enhanced Tiger Subduing Pellets", "Chapter 3 -> Bitter Lake -> Precept Corridor: follow the downhill story path and collect the page."),
        ];
    }

    private static TargetKnowledge SpiritTarget(int id, string name, string howToGet)
    {
        return new TargetKnowledge(id, $"Spirit: {name}", howToGet);
    }

    public AnalysisReport AnalyzeUploadedSave(string saveFileName, byte[] saveBytes)
    {
        return AnalyzeCore(saveBytes, NormalizeSaveFileName(saveFileName));
    }

    private AnalysisReport AnalyzeCore(byte[] saveBytes, string saveFileName)
    {
        if (saveBytes.Length == 0)
        {
            throw new ArgumentException("Save file is empty.", nameof(saveBytes));
        }

        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Reading save file {SaveFileName}", saveFileName);
        _logger.LogInformation("Loaded {ByteCount} bytes from save file.", saveBytes.Length);
        IMessage<ArchiveFile> info = new ArchiveFile();
        info.MergeFrom(saveBytes);
        if (info is not ArchiveFile archiveFile)
        {
            throw new InvalidOperationException("Invalid archive protobuf payload.");
        }

        var contentBytes = archiveFile.GameArchivesDataBytes.ToByteArray();
        var data = BGW_GameArchiveMgr.DeserializeArchiveDataFromBytes<FUStBEDArchivesData>(true, contentBytes);

        var chapter = data.RoleData?.RoleCs?.Chapter?.CurChapter ?? -1;
        var mapId = data.PersistentECSData?.BPCData?.BPCPlayerRoleData?.MapId ?? -1;
        var maxMapId = data.PersistentECSData?.BPCData?.BPCPlayerRoleData?.MaxMapId ?? -1;
        var newGamePlusCount = data.RoleData?.RoleCs?.Actor?.NewGamePlusCount ?? 0;
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
        var achievementRequirementIds = BuildAchievementRequirementIds(achievements);
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
            var context = new RouteContext(chapter, mapId, maxMapId, activeRebirthPoints.Count);
            var knowledge = GetKnowledge(
                config.AchievementId,
                completedRequirementIds,
                ownedIds,
                achievementRequirementIds
            );
            var completionFallbackUsed = false;
            if (
                !isComplete
                && config.IsResetOnGameplus
                && newGamePlusCount > 0
                && knowledge?.Targets.Count > 0
                && knowledge.MissingTargets.Count == 0
            )
            {
                isComplete = true;
                completedCount = requiredCount > 0
                    ? Math.Max(completedCount, requiredCount)
                    : Math.Max(completedCount, knowledge.Targets.Count);
                completionFallbackUsed = true;
            }

            var remaining = isComplete
                ? 0
                : requiredCount > 0
                    ? Math.Max(requiredCount - completedCount, 0)
                    : 1;
            var priority = PriorityFor(config.AchievementId, requirementType);
            AchievementGuideCatalog.ById.TryGetValue(config.AchievementId, out var guide);
            var displayTitle =
                guide?.Name
                ?? knowledge?.DisplayTitleOverride
                ?? BuildTitle(config.AchievementId, requirementType);
            var routeHint =
                knowledge?.RouteHintOverride
                ?? guide?.RequirementSummary
                ?? RouteHint(requirementType, context);

            var steps = BuildStepPlan(
                config.AchievementId,
                requirementType,
                remaining,
                requiredCount,
                completedCount,
                context,
                knowledge
            );
            if (completionFallbackUsed)
            {
                steps.Add(
                    "Marked complete from the tracked checklist because this resettable NG+ achievement did not keep its top-level save flag in sync."
                );
            }
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
                    RequirementSummary = guide?.RequirementSummary ?? routeHint,
                    Category = guide?.Category ?? "Other",
                    Chapter = guide?.Chapter ?? "Unknown",
                    IsMissable = guide?.IsMissable ?? false,
                    MissableNote = guide?.MissableNote,
                    RequiresNewGamePlus = guide?.RequiresNewGamePlus ?? false,
                    Prerequisites = guide?.Prerequisites.ToList() ?? [],
                    GuideSteps = guide?.GuideSteps.ToList() ?? [],
                    GuideChecklist = guide?.GuideChecklist.ToList() ?? [],
                    IsPresentInSave = true,
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

        var decodedPlatformPlans = plans
            .Where(x => x.AchievementId is >= 81001 and <= 81081)
            .GroupBy(x => x.AchievementId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(plan => plan.IsComplete)
                    .ThenByDescending(plan => plan.CompletedCount)
                    .First()
            );
        var selectedPlans = new List<AchievementPlan>(AchievementGuideCatalog.All.Count);
        foreach (var guide in AchievementGuideCatalog.All)
        {
            if (decodedPlatformPlans.TryGetValue(guide.Id, out var decodedPlan))
            {
                selectedPlans.Add(decodedPlan);
                continue;
            }

            AchievementKnowledgeResult? knowledge = null;
            if (
                AchievementKnowledgeMap.TryGetValue(guide.Id, out var knowledgeDefinition)
                && knowledgeDefinition.TargetSource == TargetSource.DecodedSaveInventory
            )
            {
                knowledge = GetKnowledge(guide.Id, [], ownedIds, achievementRequirementIds);
            }
            var trackedCount = knowledge?.Targets.Count ?? 0;
            var trackedComplete = knowledge?.Targets.Count(target => target.IsCollected) ?? 0;
            var priority = PriorityFor(guide.Id, "NotPresentInSave");
            selectedPlans.Add(
                new AchievementPlan
                {
                    Index = guide.Id - 81001,
                    AchievementId = guide.Id,
                    DisplayTitle = guide.Name,
                    RequirementSummary = guide.RequirementSummary,
                    Category = guide.Category,
                    Chapter = guide.Chapter,
                    IsMissable = guide.IsMissable,
                    MissableNote = guide.MissableNote,
                    RequiresNewGamePlus = guide.RequiresNewGamePlus,
                    Prerequisites = guide.Prerequisites.ToList(),
                    GuideSteps = guide.GuideSteps.ToList(),
                    GuideChecklist = guide.GuideChecklist.ToList(),
                    IsPresentInSave = false,
                    RequirementType = "NotPresentInSave",
                    RequiredCount = trackedCount,
                    RequiredCountText = trackedCount > 0 ? trackedCount.ToString() : "Trigger",
                    CompletedCount = trackedComplete,
                    RemainingCount = trackedCount > 0 ? Math.Max(trackedCount - trackedComplete, 0) : 1,
                    IsComplete = false,
                    IsProgressType = trackedCount > 0,
                    ResetOnNewGamePlus = false,
                    CompletedRequirementIds = [],
                    CompletedRequirementGuids = [],
                    PriorityOrder = priority.order,
                    PriorityLabel = priority.label,
                    RouteHint = knowledge?.RouteHintOverride ?? guide.RequirementSummary,
                    Steps =
                    [
                        "This save does not expose a top-level progress row for this achievement yet. The guide remains available, and any decoded item ownership is shown below.",
                    ],
                    RequirementTargets = knowledge?.Targets ?? [],
                    MissingTargets = knowledge?.MissingTargets ?? [],
                }
            );
        }

        const string filterMode = "canonical_81";
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
            SaveFileName = saveFileName,
            GeneratedAtUtc = DateTime.UtcNow,
            PlayerName = data.RoleData?.RoleCs?.Base?.Name ?? "Unknown",
            PlayerLevel = data.RoleData?.RoleCs?.Base?.Level ?? 0,
            NewGamePlusCount = newGamePlusCount,
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

    private static string NormalizeSaveFileName(string pathOrFileName)
    {
        var saveFileName = Path.GetFileName(pathOrFileName.Trim().Replace('\\', '/'));
        return string.IsNullOrWhiteSpace(saveFileName) ? "uploaded-save.sav" : saveFileName;
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
        int achievementId,
        string requirementType,
        int remaining,
        int requiredCount,
        int completedCount,
        RouteContext context,
        AchievementKnowledgeResult? knowledge
    )
    {
        var steps = new List<string>
        {
            requiredCount > 0
                ? $"Progress: {completedCount}/{requiredCount} done, {remaining} left."
                : $"Status: {(remaining == 0 ? "complete" : "still locked")}."
        };

        if (achievementId == 81045)
        {
            var missingTargetIds = knowledge?.MissingTargets.Select(x => x.Id).ToHashSet() ?? [];
            if (missingTargetIds.Count == 0)
            {
                steps.Add(
                    "Both Chapter 4 scorpion-family requirements are already present in the save."
                );
                steps.Add("If this was earned on an earlier cycle, an NG+ rescan should now stay marked complete.");
                return steps;
            }

            if (missingTargetIds.Contains(3001))
            {
                steps.Add(
                    "Revisit the Temple of Yellow Flower routes and defeat Scorpionlord before the encounter locks out."
                );
            }

            if (missingTargetIds.Contains(3002))
            {
                steps.Add(
                    "Defeat the four smaller Chapter 4 scorpion-family enemies so the second requirement records."
                );
            }

            return steps;
        }

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

    private static (int order, string label) PriorityFor(int achievementId, string requirementType)
    {
        if (achievementId == 81045)
        {
            return (1, "High");
        }

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
        foreach (var rootPropertyName in KnownOwnedRootPropertyNames)
        {
            AddIdsFromKnownNode(owned, GetPropertyValue(roleCs, rootPropertyName), 0);
        }

        return owned;
    }

    private static void AddIdsFromKnownNode(HashSet<int> owned, object? value, int depth)
    {
        if (value is null || depth > 5)
        {
            return;
        }

        if (value is string)
        {
            return;
        }

        foreach (var propertyName in KnownOwnedIdPropertyNames)
        {
            if (TryReadPositiveInt(value, propertyName, out var ownedId))
            {
                owned.Add(ownedId);
            }
        }

        if (value is IEnumerable enumerable)
        {
            foreach (var entry in enumerable)
            {
                AddIdsFromKnownNode(owned, entry, depth + 1);
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

            AddIdsFromKnownNode(owned, child, depth + 1);
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
        IReadOnlyCollection<int> ownedIds,
        IReadOnlyDictionary<int, IReadOnlySet<int>> achievementRequirementIds
    )
    {
        if (!AchievementKnowledgeMap.TryGetValue(achievementId, out var knowledge))
        {
            return null;
        }

        var targets = knowledge.TargetSource switch
        {
            TargetSource.DecodedSaveInventory => BuildTargetsFromCollectedIds(
                knowledge.Targets,
                BuildCollectedTargetIds(completedRequirementIds, ownedIds)
            ),
            TargetSource.LinkedAchievementRequirements => BuildTargetsFromAchievementRequirements(
                knowledge.Targets,
                achievementRequirementIds,
                ownedIds
            ),
            _ => BuildTargetsFromCollectedIds(knowledge.Targets, new HashSet<int>(completedRequirementIds))
        };
        var missing = targets.Where(x => !x.IsCollected).ToList();

        return new AchievementKnowledgeResult
        {
            DisplayTitleOverride = knowledge.DisplayTitleOverride,
            RouteHintOverride = knowledge.RouteHintOverride,
            Targets = targets,
            MissingTargets = missing,
        };
    }

    private static HashSet<int> BuildCollectedTargetIds(
        IReadOnlyCollection<int> completedRequirementIds,
        IReadOnlyCollection<int> ownedIds
    )
    {
        var completed = new HashSet<int>(ownedIds);
        completed.UnionWith(completedRequirementIds);
        return completed;
    }

    private static Dictionary<int, IReadOnlySet<int>> BuildAchievementRequirementIds(
        IEnumerable<AchievementOne> achievements
    )
    {
        var requirementIds = new Dictionary<int, IReadOnlySet<int>>();
        foreach (var achievement in achievements)
        {
            var config = achievement.Config;
            if (config is null)
            {
                continue;
            }

            requirementIds[config.AchievementId] =
                new HashSet<int>(achievement.CompleteRequirementList?.ToList() ?? []);
        }

        return requirementIds;
    }

    private static List<RequirementTarget> BuildTargetsFromCollectedIds(
        IEnumerable<TargetKnowledge> targets,
        IReadOnlySet<int> collectedIds
    )
    {
        return targets
            .Select(x => new RequirementTarget
            {
                Id = x.Id,
                Name = x.Name,
                IsCollected = collectedIds.Contains(x.Id),
                HowToGet = x.HowToGet,
            })
            .ToList();
    }

    private static List<RequirementTarget> BuildTargetsFromAchievementRequirements(
        IEnumerable<TargetKnowledge> targets,
        IReadOnlyDictionary<int, IReadOnlySet<int>> achievementRequirementIds,
        IReadOnlyCollection<int> ownedIds
    )
    {
        return targets
            .Select(x => new RequirementTarget
            {
                Id = x.Id,
                Name = x.Name,
                IsCollected = ownedIds.Contains(x.Id)
                    || (
                        x.SourceAchievementId is int sourceAchievementId
                        && achievementRequirementIds.TryGetValue(
                            sourceAchievementId,
                            out var collectedIds
                        )
                        && collectedIds.Contains(x.Id)
                    ),
                HowToGet = x.HowToGet,
            })
            .ToList();
    }
}

public sealed record RouteContext(
    int CurrentChapterId,
    int CurrentMapId,
    int MaxMapId,
    int ActiveRebirthPointCount
);

public sealed class AnalysisReport
{
    public required string SaveFileName { get; init; }
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
    public required string RequirementSummary { get; init; }
    public required string Category { get; init; }
    public required string Chapter { get; init; }
    public required bool IsMissable { get; init; }
    public string? MissableNote { get; init; }
    public required bool RequiresNewGamePlus { get; init; }
    public required List<string> Prerequisites { get; init; }
    public required List<string> GuideSteps { get; init; }
    public required List<string> GuideChecklist { get; init; }
    public required bool IsPresentInSave { get; init; }
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
    LinkedAchievementRequirements,
}

sealed class AchievementKnowledgeResult
{
    public string? DisplayTitleOverride { get; init; }
    public string? RouteHintOverride { get; init; }
    public required List<RequirementTarget> Targets { get; init; }
    public required List<RequirementTarget> MissingTargets { get; init; }
}

sealed record TargetKnowledge(int Id, string Name, string HowToGet, int? SourceAchievementId = null);
