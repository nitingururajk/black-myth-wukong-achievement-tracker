namespace bmw_web.Services;

internal sealed record AchievementGuide(
    int Id,
    string Name,
    string RequirementSummary,
    string Category,
    string Chapter,
    bool IsMissable,
    string? MissableNote,
    bool RequiresNewGamePlus,
    IReadOnlyList<string> Prerequisites,
    IReadOnlyList<string> GuideSteps,
    IReadOnlyList<string> GuideChecklist
);

internal static class AchievementGuideCatalog
{
    public static IReadOnlyList<AchievementGuide> All { get; } = Build();

    public static IReadOnlyDictionary<int, AchievementGuide> ById { get; } =
        All.ToDictionary(guide => guide.Id);

    private static IReadOnlyList<AchievementGuide> Build()
    {
        var guides = new List<AchievementGuide>
        {
            G(
                81001,
                "Home is Behind",
                "Finish the opening battle and reach Black Wind Mountain.",
                "Story",
                "Prologue",
                steps:
                [
                    "Complete the tutorial duel with Erlang and continue through the opening scenes.",
                    "The achievement registers automatically when the Destined One begins the journey.",
                ]
            ),
            G(
                81002,
                "Hammer and Hew",
                "Craft any piece of armor at a Keeper's Shrine.",
                "Crafting",
                "Chapter 1",
                steps:
                [
                    "Advance until shrine armor crafting becomes available.",
                    "Open Craft Armor, choose any affordable unlocked piece, and forge it.",
                ]
            ),
            G(
                81003,
                "Warring with Wolves",
                "Defeat Lingxuzi in Guanyin Temple.",
                "Story Boss",
                "Chapter 1",
                steps:
                [
                    "Follow the Forest of Wolves route to Guanyin Temple.",
                    "Defeat Lingxuzi; this is a required story encounter.",
                ]
            ),
            G(
                81004,
                "Absorb and Cultivate",
                "Absorb your first Yaoguai Spirit with the Blessed Gourd.",
                "Combat",
                "Chapter 1",
                prerequisites: ["Receive the Blessed Gourd guidance from the old master on the Bamboo Grove route."],
                steps:
                [
                    "Defeat a spirit-dropping Yaoguai Chief.",
                    "Interact with the blue flame left behind to absorb its Spirit.",
                ]
            ),
            G(
                81005,
                "Brew of Bravery",
                "Equip any soak in a drink at a Keeper's Shrine.",
                "Crafting",
                "Chapter 1",
                steps:
                [
                    "Obtain a soak, then choose Brew at a Keeper's Shrine.",
                    "Select the drink, place the soak in an open slot, and confirm the brew.",
                ]
            ),
            G(
                81006,
                "Slithering Snake",
                "Defeat Whiteclad Noble in the Marsh of White Mist.",
                "Story Boss",
                "Chapter 1",
                steps:
                [
                    "Continue through Bamboo Grove to Marsh of White Mist.",
                    "Win both phases of the Whiteclad Noble fight.",
                ]
            ),
            G(
                81007,
                "Handy and Hardy",
                "Craft any weapon at a Keeper's Shrine.",
                "Crafting",
                "Chapter 1",
                steps:
                [
                    "Defeat bosses until a staff upgrade unlocks in shrine crafting.",
                    "Gather its materials and Will, then forge the weapon.",
                ]
            ),
            G(
                81008,
                "Enduring Echoes",
                "Ring all three great bells in Chapter 1.",
                "Exploration",
                "Chapter 1",
                prerequisites: ["Explore the side arenas near Guangzhi, Guangmou, and Whiteclad Noble."],
                steps:
                [
                    "Ring the bell behind Guangzhi at Forest of Wolves - Outside the Forest.",
                    "Ring the bell behind Guangmou at Bamboo Grove - Snake Trail.",
                    "Ring the bell beyond Whiteclad Noble at Marsh of White Mist.",
                ],
                checklist: ["Guangzhi bell", "Guangmou bell", "Whiteclad Noble bell"]
            ),
            G(
                81009,
                "Temple of Taint",
                "Defeat Elder Jinchi in the Ancient Guanyin Temple secret area.",
                "Secret Boss",
                "Chapter 1",
                prerequisites: ["Ring all three Chapter 1 bells."],
                steps:
                [
                    "The third bell transports you to Ancient Guanyin Temple.",
                    "Rest at the shrine, loot the area, and defeat Elder Jinchi.",
                ]
            ),
            G(
                81010,
                "Blazing Black Wind",
                "Defeat Black Bear Guai and finish Chapter 1.",
                "Story Boss",
                "Chapter 1",
                steps:
                [
                    "Climb Black Wind Mountain after defeating Black Wind King.",
                    "Use the Fireproof Mantle if needed, then defeat Black Bear Guai.",
                ]
            ),
            G(
                81011,
                "Creative Concoction",
                "Craft any mortal medicine after learning medicine crafting.",
                "Crafting",
                "Chapter 2",
                prerequisites: ["Meet Xu Dog below Crouching Tiger Temple and complete his first request."],
                steps:
                [
                    "Open Make Medicines at a Keeper's Shrine or speak with Xu Dog.",
                    "Craft any available consumable formula.",
                ]
            ),
            G(
                81012,
                "Cursed Clan",
                "Defeat the royal rat family encounters in Sandgate Village.",
                "Boss Chain",
                "Chapter 2",
                steps:
                [
                    "Fight the King of Flowing Sands and the Second Rat Prince at Sandgate Village.",
                    "Continue into the valley and defeat the First Rat Prince.",
                ],
                checklist: ["King of Flowing Sands and Second Rat Prince", "First Rat Prince"]
            ),
            G(
                81013,
                "Cage of Claws",
                "Defeat Red Loong behind the Chapter 1 waterfall.",
                "Secret Boss",
                "Chapter 1",
                prerequisites: ["Obtain Loong Scales from the hidden wall in Chapter 2's First Rat Prince arena."],
                steps:
                [
                    "Travel back to Forest of Wolves - Outside the Forest.",
                    "Follow the waterfall route, use Loong Scales to enter, and defeat Red Loong.",
                ]
            ),
            G(
                81014,
                "Sound in Stone",
                "Complete the Man-in-Stone quest in Fright Cliff.",
                "Side Quest",
                "Chapter 2",
                steps:
                [
                    "Find Man-in-Stone in the ravine near Squall Hideout and exhaust his dialogue.",
                    "Defeat Mother of Stones, return the Stone Essence, then defeat Man-in-Stone.",
                    "Rest and return so his merchant inventory becomes available.",
                ],
                checklist: ["Speak to Man-in-Stone", "Defeat Mother of Stones", "Return Stone Essence and win the duel"]
            ),
            G(
                81015,
                "Death in Despair",
                "Defeat Black Loong behind the Chapter 2 sandfall.",
                "Secret Boss",
                "Chapter 2",
                prerequisites: ["Obtain Loong Scales from the First Rat Prince arena."],
                steps:
                [
                    "Travel to Fright Cliff - Rockrest Flat and find the sandfall.",
                    "Use Loong Scales to open the arena and defeat Black Loong.",
                ]
            ),
            G(
                81016,
                "The Stone's Secret",
                "Defeat Stone Vanguard at Rockrest Flat.",
                "Story Boss",
                "Chapter 2",
                steps:
                [
                    "Explore Fright Cliff and reach the Stone Vanguard arena.",
                    "Defeat Stone Vanguard to receive the key item needed for story progression.",
                ]
            ),
            G(
                81017,
                "Oṃ Maṇi Padme Hūm",
                "Collect six Buddha's Eyeballs and use them to awaken Shigandang.",
                "Exploration",
                "Chapter 2",
                steps:
                [
                    "Collect all six glowing Buddha heads around Squall Hideout and Rockrest Flat.",
                    "Place the Eyeballs at the large stone beside the Stone Vanguard arena.",
                    "Defeat Shigandang when it awakens.",
                ],
                checklist:
                [
                    "Six Buddha's Eyeballs collected",
                    "Eyeballs placed at the arena stone",
                    "Shigandang defeated",
                ]
            ),
            G(
                81018,
                "Shifting Sands",
                "Defeat Fuban in the Kingdom of Sahali secret area.",
                "Secret Boss",
                "Chapter 2",
                prerequisites: ["Complete the Yellow-Robed Squire's drunken boar quest to enter Sahali."],
                steps:
                [
                    "Advance through the Kingdom of Sahali and defeat its Tiger Vanguard.",
                    "Speak with the Yellow Wind Sage, enter the desert arena, and defeat Fuban.",
                ]
            ),
            G(
                81019,
                "Gleams of Gold",
                "Complete the Yellow-Robed Squire quest and enter the Kingdom of Sahali.",
                "Side Quest",
                "Chapter 2",
                steps:
                [
                    "Give the drunken boar a Sobering Stone near Rockrest Flat.",
                    "Speak to him at Crouching Tiger Temple and give him a Jade Lotus.",
                    "Return to his first location, defeat the Yellow-Robed Squire, and follow him through the gate.",
                ],
                checklist: ["Sobering Stone delivered", "Temple conversation and Jade Lotus", "Yellow-Robed Squire defeated"]
            ),
            G(
                81020,
                "The Tiger Family",
                "Defeat all three Tiger Vanguard family encounters in Chapter 2.",
                "Boss Chain",
                "Chapter 2",
                steps:
                [
                    "Defeat Tiger Vanguard at Crouching Tiger Temple.",
                    "Defeat the earlier Tiger Vanguard in the Kingdom of Sahali.",
                    "Use the Old Rattle-Drum route to find and defeat Mad Tiger.",
                ],
                checklist: ["Crouching Tiger Temple's Tiger Vanguard", "Kingdom of Sahali's Tiger Vanguard", "Mad Tiger"]
            ),
            G(
                81021,
                "Buried in the Sand",
                "Complete the Old Rattle-Drum quest and defeat Mad Tiger.",
                "Side Quest",
                "Chapter 2",
                prerequisites: ["Defeat Tiger's Acolyte and obtain the Old Rattle-Drum."],
                steps:
                [
                    "Use the drum at each of the three child-voice locations in Yellow Wind Ridge.",
                    "Follow the child into the well in Sandgate Village and defeat Mad Tiger.",
                ],
                checklist: ["Windrest Hamlet drum spot", "Windseal Gate drum spot", "Sandgate Village drum spot and well"]
            ),
            G(
                81022,
                "Precious Pills",
                "Craft your first Celestial Medicine with Xu Dog.",
                "Crafting",
                "Chapter 2",
                prerequisites: ["Defeat Tiger Vanguard and find Xu Dog in the Crouching Tiger Temple cellar."],
                steps:
                [
                    "Give Xu Dog a Mind Core in the cellar or Zodiac Village.",
                    "Choose a permanent Health, Mana, Stamina, or resistance upgrade and craft it.",
                ]
            ),
            G(
                81023,
                "A Great Gust",
                "Defeat Yellow Wind Sage and finish Chapter 2.",
                "Story Boss",
                "Chapter 2",
                steps:
                [
                    "Open the gate with the key items from Stone Vanguard and Tiger Vanguard.",
                    "Equip Wind Tamer for an easier final phase, then defeat Yellow Wind Sage.",
                ]
            ),
            G(
                81024,
                "Thousand-Mile Quest",
                "Complete the two-headed rat prisoner's quest and obtain Ashen Slumber.",
                "Side Quest",
                "Chapter 3",
                prerequisites: ["Exhaust the two-headed rat NPC dialogue in Chapter 2's Sandgate Village."],
                steps:
                [
                    "Speak with the hidden two-headed rat survivor in Sandgate Village until dialogue repeats.",
                    "In Chapter 3, inspect the prison cell beside the Lower Pagoda starting cell.",
                    "Follow the quest outcome to obtain the Ashen Slumber transformation.",
                ]
            ),
            G(
                81025,
                "Voice Vanquished",
                "Defeat Captain Wise-Voice in Pagoda Realm.",
                "Story Boss",
                "Chapter 3",
                steps:
                [
                    "Climb from Lower Pagoda to Mani Wheel.",
                    "Before the fight, finish the nine Lantern Wardens if you want their missable Curio.",
                    "Defeat Captain Wise-Voice to end the Pagoda curse.",
                ]
            ),
            G(
                81026,
                "Karma of Kang-Jin",
                "Defeat Kang-Jin Star at Turtle Island.",
                "Story Boss",
                "Chapter 3",
                steps:
                [
                    "Continue across the frozen lake and Pagoda route to Turtle Island.",
                    "Defeat Kang-Jin Star to continue toward Bitter Lake.",
                ]
            ),
            G(
                81027,
                "Boundless Bitterness",
                "Defeat Cyan Loong on Turtle Island.",
                "Secret Boss",
                "Chapter 3",
                prerequisites: ["Obtain Loong Scales in Chapter 2."],
                steps:
                [
                    "From Turtle Island shrine, descend to the lone fisherman-like figure at the island edge.",
                    "Approach with Loong Scales and defeat Cyan Loong.",
                ]
            ),
            G(
                81028,
                "Shell and Scales",
                "Defeat Apramana Bat and collect Turtle Tear on the North Shore of Bitter Lake.",
                "Secret Boss",
                "Chapter 3",
                isMissable: true,
                missableNote: "Apramana Bat can disappear after the Chapter 3 story advances too far. Do this on the first North Shore visit with Zhu Bajie.",
                steps:
                [
                    "From North Shore of the Bitter Lake, follow the right-hand coast while Zhu Bajie accompanies you.",
                    "Inspect the giant snake skeleton and defeat Apramana Bat.",
                    "Interact with the giant turtle's tear on the shore; the Turtle Tear soak is also needed later.",
                ],
                checklist: ["Apramana Bat defeated before it disappears", "Turtle Tear collected"]
            ),
            G(
                81029,
                "Secret in the Scroll",
                "Complete Chen Loong's request and unlock Zodiac Village.",
                "Side Quest",
                "Chapter 3",
                prerequisites: ["Reach North Shore of the Bitter Lake and have access to Xu Dog."],
                steps:
                [
                    "Defeat Chen Loong in the water near North Shore of the Bitter Lake.",
                    "Ask Xu Dog for the special medicine and deliver it to Chen Loong.",
                    "Use the Ruyi Scroll he gives you to enter Zodiac Village.",
                ]
            ),
            G(
                81030,
                "Pound and Perfect",
                "Upgrade any armor piece with Yin Tiger.",
                "Crafting",
                "Chapter 3",
                prerequisites: ["Unlock Zodiac Village through Chen Loong's quest."],
                steps:
                [
                    "Speak with Yin Tiger at the village forge.",
                    "Choose Upgrade Armor and improve any eligible piece by one rarity tier.",
                ]
            ),
            G(
                81031,
                "The Soaring Slash",
                "Challenge and defeat Yin Tiger in Zodiac Village.",
                "Optional Boss",
                "Chapter 3",
                prerequisites: ["Unlock Zodiac Village."],
                steps:
                [
                    "Ask Yin Tiger to challenge him; upgrades remain available after the duel.",
                    "Defeat him to unlock Ebon Flow and an extra Curio slot.",
                ]
            ),
            G(
                81032,
                "Happy Harvest",
                "Give Chen Loong a seed and harvest a plant in Zodiac Village.",
                "Collection",
                "Chapter 3",
                prerequisites: ["Unlock Zodiac Village and obtain any seed while harvesting plants."],
                steps:
                [
                    "Deliver a seed to Chen Loong so he plants it.",
                    "Return after the real-time crop cycle and harvest the planted ingredients.",
                ]
            ),
            G(
                81033,
                "Marvelous Melon",
                "Complete the Treasure Hunter quest and defeat Green-Capped Martialist.",
                "Side Quest",
                "Chapter 3",
                steps:
                [
                    "Rescue and exhaust the Treasure Hunter's dialogue near North Shore of the Bitter Lake.",
                    "Find him shivering near Towers of Karma and cast Ring of Fire beside him.",
                    "Reach Melon Field from Brook of Bliss and defeat Green-Capped Martialist.",
                ],
                checklist: ["Bitter Lake meeting", "Ring of Fire at Towers of Karma", "Melon Field duel"]
            ),
            G(
                81034,
                "Lust and Dust",
                "Complete the Snow Fox quest by defeating Non-Void and reporting back.",
                "Side Quest",
                "Chapter 3",
                steps:
                [
                    "Speak to the fox corpse near Forest of Felicity and take the Snow Fox Brush.",
                    "Use the brush near Non-Void in New Thunderclap Temple, then defeat him.",
                    "Return to the fox corpse to finish the quest and receive Snow Fox Brush Curio.",
                ]
            ),
            G(
                81035,
                "Corrupted Captains",
                "Defeat the four Captains and give their spirit remnants to the Pagoda prisoner.",
                "Boss Chain",
                "Chapter 3",
                prerequisites: ["Speak through the locked cell beside Lower Pagoda's starting cell."],
                steps:
                [
                    "Defeat Captain Lotus-Vision below Upper Pagoda.",
                    "Defeat Captain Wise-Voice at Mani Wheel.",
                    "Defeat Captain Kalpa-Wave before New Thunderclap Temple.",
                    "Collect Captain Void-Illusion's remnant from the corpse near Longevity Road.",
                    "Return all four remnants to the prisoner at Lower Pagoda.",
                ],
                checklist: ["Lotus-Vision", "Wise-Voice", "Kalpa-Wave", "Void-Illusion", "Prisoner turn-in"]
            ),
            G(
                81036,
                "Devoted Disciples",
                "Defeat Yellowbrow's four named disciples.",
                "Boss Chain",
                "Chapter 3",
                steps:
                [
                    "Defeat Non-White twice on the Mindfulness Cliff route.",
                    "Defeat Non-Able near Brook of Bliss.",
                    "Defeat Non-Void during the Snow Fox quest.",
                    "Defeat Non-Pure inside New Thunderclap Temple.",
                ],
                checklist: ["Non-White", "Non-Able", "Non-Void", "Non-Pure"]
            ),
            G(
                81037,
                "Matches with the Macaque",
                "Win all three Macaque Chief encounters in Chapter 3.",
                "Story Boss",
                "Chapter 3",
                steps:
                [
                    "Defeat Macaque Chief at Snowhill Path.",
                    "Defeat the second encounter during Chapter 3 progression.",
                    "Win the final encounter during Yellowbrow's story sequence.",
                ],
                checklist: ["Snowhill Path encounter", "Second encounter", "Final Yellowbrow sequence"]
            ),
            G(
                81038,
                "Nifty Nonsense",
                "Defeat Yellowbrow and finish Chapter 3.",
                "Story Boss",
                "Chapter 3",
                steps:
                [
                    "Clear New Thunderclap Temple and enter the main hall.",
                    "Complete every phase of the Yellowbrow story battle.",
                ]
            ),
            G(
                81039,
                "Mud on His Face",
                "Defeat Buddha's Right Hand in Webbed Hollow.",
                "Story Boss",
                "Chapter 4",
                steps:
                [
                    "Follow the Webbed Hollow main route to the suspended hand-shaped creature.",
                    "Defeat both phases to create the bridge deeper into the hollow.",
                ]
            ),
            G(
                81040,
                "Gnashing Grudge",
                "Defeat Zhu Bajie in both Chapter 4 encounters.",
                "Story Boss",
                "Chapter 4",
                steps:
                [
                    "Defeat Zhu Bajie in the first Webbed Hollow arena.",
                    "Pursue him through the mud route and win the second encounter.",
                ],
                checklist: ["First Zhu Bajie fight", "Second mud-arena fight"]
            ),
            G(
                81041,
                "Passion Passes",
                "Defeat Violet Spider in Webbed Hollow.",
                "Story Boss",
                "Chapter 4",
                steps:
                [
                    "Reach the Estate of Zhu and continue to the Violet Spider arena.",
                    "Defeat Violet Spider and escape the webbed cavern sequence.",
                ]
            ),
            G(
                81042,
                "The Loong Pattern",
                "Defeat Yellow Loong after completing the earlier Loong fights.",
                "Secret Boss",
                "Chapter 4",
                prerequisites: ["Obtain Loong Scales and defeat Red Loong, Black Loong, and Cyan Loong."],
                steps:
                [
                    "Travel to Webbed Hollow - Relief of the Fallen Loong.",
                    "Complete the duel with Yellow Loong to receive Golden Lining and Golden Carp.",
                ],
                checklist: ["Red Loong", "Black Loong", "Cyan Loong", "Yellow Loong"]
            ),
            G(
                81043,
                "Secret in Purple Cloud",
                "Defeat Venom Daoist twice and enter Purple Cloud Mountain.",
                "Secret Area",
                "Chapter 4",
                steps:
                [
                    "Defeat Venom Daoist at Pool of Shattered Jade.",
                    "Find and defeat him again near Court of Illumination.",
                    "Pass through the revealed mural to enter Purple Cloud Mountain.",
                ],
                checklist: ["Pool of Shattered Jade encounter", "Court of Illumination encounter", "Purple Cloud Mountain entered"]
            ),
            G(
                81044,
                "The Wayward Ways",
                "Complete Daoist Mi's quest and obtain Violet Hail.",
                "Side Quest",
                "Chapter 4",
                isMissable: true,
                missableNote: "Daoist Mi disappears when Duskveil is defeated. Finish his quest before the secret-area final boss.",
                prerequisites: ["Unlock Purple Cloud Mountain."],
                steps:
                [
                    "Speak to Daoist Mi at Petalfall Hamlet.",
                    "Farm nearby yellow-robed Daoists until one drops Violet Hailstone.",
                    "Return the item and defeat Daoist Mi before fighting Duskveil.",
                ]
            ),
            G(
                81045,
                "A Family Finished",
                "Defeat Scorpionlord and all four smaller scorpion-family enemies in Chapter 4.",
                "Boss Chain",
                "Chapter 4",
                isMissable: true,
                missableNote: "Scorpionlord joins the Duskveil fight and becomes unavailable if Duskveil is engaged first. Defeat him before Duskveil.",
                prerequisites: ["Unlock Purple Cloud Mountain for Scorpionlord."],
                steps:
                [
                    "Find and defeat the four scorpion princes while exploring Webbed Hollow.",
                    "At Purple Cloud Mountain, break Scorpionlord's jars and defeat him before Duskveil.",
                ],
                checklist: ["Four scorpion princes", "Scorpionlord before Duskveil"]
            ),
            G(
                81046,
                "The Cockerel Crowed",
                "Defeat Duskveil in Purple Cloud Mountain.",
                "Secret Boss",
                "Chapter 4",
                prerequisites: ["Unlock Purple Cloud Mountain through Venom Daoist."],
                steps:
                [
                    "Finish Daoist Mi and Scorpionlord first so their missable rewards remain available.",
                    "Climb to Cloudnest Peak and defeat both phases of Duskveil.",
                ]
            ),
            G(
                81047,
                "Misfit with Merit",
                "Remove all four purple talismans and survive the Supreme Inspector encounter.",
                "Side Quest",
                "Chapter 4",
                isMissable: true,
                missableNote: "Finish all four talismans before defeating Hundred-Eyed Daoist Master; the quest becomes unavailable afterward.",
                steps:
                [
                    "Remove three purple talismans while descending through Webbed Hollow.",
                    "Remove the fourth at Temple of Yellow Flowers - Court of Illumination.",
                    "Complete the resulting Supreme Inspector encounter before the chapter boss.",
                ],
                checklist: ["Upper Hollow talisman", "Middle Hollow talisman", "Lower Hollow talisman", "Court of Illumination talisman"]
            ),
            G(
                81048,
                "Behold the Betrayal",
                "Defeat Hundred-Eyed Daoist Master and finish Chapter 4.",
                "Story Boss",
                "Chapter 4",
                steps:
                [
                    "Finish the Purple Talisman quest before entering the final arena.",
                    "Use Weaver's Needle to cancel the boss's golden domain, then defeat Hundred-Eyed Daoist Master.",
                ]
            ),
            G(
                81049,
                "Always Accompanied",
                "Fully upgrade the starting gourd and show it to the old master in Chapter 5.",
                "Upgrade",
                "Chapter 5",
                prerequisites: ["Collect enough Luojia Fragrant Vines to upgrade the Old Gourd to nine uses."],
                steps:
                [
                    "Ask Shen Monkey to upgrade the starting gourd through every tier.",
                    "In Furnace Valley, speak to the old master beside the fire until he upgrades it to Supreme Gourd.",
                ]
            ),
            G(
                81050,
                "The Furnace Boy",
                "Defeat the Keeper of Flaming Mountains and Yin-Yang Fish.",
                "Story Boss",
                "Chapter 5",
                steps:
                [
                    "Reach Emerald Hall through Furnace Valley.",
                    "Complete the Keeper of Flaming Mountains encounter, including the Yin-Yang Fish phase.",
                ]
            ),
            G(
                81051,
                "Urge Unfulfilled",
                "Complete Ma Tianba's five-chapter Horse Guai quest and obtain Dark Thunder.",
                "Side Quest",
                "Chapters 1-5",
                isMissable: true,
                missableNote: "Speak to Ma Tianba in every chapter before completing the Chapter 5 Keeper encounter; skipped dialogue can lock the final reward.",
                steps:
                [
                    "Find Ma Tianba and exhaust his dialogue in Chapters 1, 2, 3, and 4 before moving on.",
                    "In Chapter 5, find the cart holding his whip in Furnace Valley.",
                    "After the Keeper of Flaming Mountains fight, return and pull the whip to receive Dark Thunder.",
                ],
                checklist: ["Chapter 1 conversation", "Chapter 2 conversation", "Chapter 3 conversation", "Chapter 4 conversation", "Chapter 5 cart and whip"]
            ),
            G(
                81052,
                "Seeds to Sow",
                "Deliver all 12 save-tracked seed items to Chen Loong; together they grow 15 harvestable plant types.",
                "Collection",
                "Chapters 1-5",
                prerequisites: ["Unlock Zodiac Village."],
                steps:
                [
                    "Harvest plants whenever you pass them; ordinary seed drops are random and plants respawn on a real-time cycle.",
                    "Defeat the four plant bosses for their guaranteed specialist seeds.",
                    "Give every new seed to Chen Loong. The analyzer's 12-item checklist follows the game's authoritative achievement requirements.",
                ],
                checklist:
                [
                    "Jade Lotus Seed",
                    "Nine-Capped Lingzhi Seed",
                    "Monkey-Head Fungus Seed",
                    "Fragrant Jade Flower Seed",
                    "Fire Bellflower Seed",
                    "Tree Pearl Seed",
                    "Celestial Pear Seed",
                    "Licorice Seed",
                    "Fire Date Seed",
                    "Millennium Ginseng Seed",
                    "Gentian Seed",
                    "Golden Lotus Seed",
                ]
            ),
            G(
                81053,
                "Souls in the Stalks",
                "Defeat every required plant Guai: six named bosses and the four burrowing plant enemy types.",
                "Collection",
                "Chapters 3-5",
                steps:
                [
                    "Defeat the six named plant bosses, then kill at least one of each burrowing plant enemy type.",
                    "Elder Armourworm's separate Spirit also requires giving Proto-Armourworm to Chen Loong and feeding it three Rice Cocoons.",
                ],
                checklist:
                [
                    "Fungiman", "Old Ginseng Guai", "Elder Armourworm", "Fungiwoman", "Nine-Capped Lingzhi Guai", "Mother of Flamlings",
                    "Fungling", "Ginsengling", "Lingzhiling", "Minor Armourworm",
                ]
            ),
            G(
                81054,
                "A Willing Warrior",
                "Complete Pale-Axe Stalwart's Five Element Carts quest and open Bishui Cave.",
                "Side Quest",
                "Chapter 5",
                steps:
                [
                    "Defeat Pale-Axe Stalwart, then exhaust his friendly dialogue.",
                    "Defeat the active elemental carts along the Chapter 5 story route and revisit Pale-Axe after each stage.",
                    "Return to Cooling Slope, defeat Rusty-Gold Cart, and inspect the frozen gate to Bishui Cave.",
                ],
                checklist: ["Pale-Axe Stalwart recruited", "Brown-Iron Cart", "Gray-Bronze Cart", "Crimson-Silver Cart", "Rusty-Gold Cart and Bishui gate"]
            ),
            G(
                81055,
                "Scenic Seeker",
                "Find all 24 Meditation Spots: 3 in Chapter 1, 6 in Chapter 2, 5 in Chapter 3, 6 in Chapter 4, and 4 in Chapter 5.",
                "Collection",
                "Chapters 1-5",
                steps:
                [
                    "Meditation Spots are one-time map interactions, so sweep them in shrine order before leaving each chapter.",
                    "Use the analyzer's 24 save-verified rows to see the exact spot still missing.",
                ],
                checklist:
                [
                    "Chapter 1: The Arbor, The Cavern, The Cliff",
                    "Chapter 2: The Ravine, The Altar, The Grotto, The Sculpture, The Deadwood, The Rock",
                    "Chapter 3: The Shade, The Bottom, The Statue, The Track, The Hall",
                    "Chapter 4: The Carvings, The Tree, Cave Depths, The Height, The Pines, The Ledge",
                    "Chapter 5: The Buddha, The Relief, The Crag, The Screen",
                ]
            ),
            G(
                81056,
                "Three Teams of Two",
                "Defeat the three paired Chapter 5 boss teams.",
                "Boss Chain",
                "Chapter 5",
                steps:
                [
                    "Explore every side arena along Woods of Ember and Furnace Valley rather than following only the story path.",
                    "Defeat both members of each paired encounter.",
                ],
                checklist: ["Quick as Fire and Fast as Wind", "Cloudy Mist and Misty Cloud", "Top Takes Bottom and Bottom Takes Top"]
            ),
            G(
                81057,
                "With Full Spirit",
                "Collect all 54 base-game Yaoguai Spirits tracked by the save.",
                "Collection",
                "Chapters 1-5",
                isMissable: true,
                missableNote: "Wandering Wight disappears after Elder Jinchi, and Second Rat Prince's Spirit needs the correct kill order. Missed Spirits require another cycle.",
                steps:
                [
                    "Defeat every blue-flame elite and absorb the Spirit after the fight.",
                    "If a flame was left before receiving the Blessed Gourd, use Retrieve Spirits at a Keeper's Shrine.",
                    "Check the analyzer's 54 save-verified Spirit rows; their runtime skill names are more reliable than inventory sorting.",
                ],
                checklist: ["Wandering Wight before Elder Jinchi", "Second Rat Prince Spirit via correct boss order", "All 54 save-tracked Spirit skills owned"]
            ),
            G(
                81058,
                "Frost and Flame",
                "Defeat Bishui Golden-Eyed Beast in Bishui Cave.",
                "Secret Boss",
                "Chapter 5",
                prerequisites: ["Complete the Five Element Carts quest and enter Bishui Cave."],
                steps:
                [
                    "Cross the Chapter 5 secret area to the Purge Pit route.",
                    "Defeat Bishui Golden-Eyed Beast; arena transitions can expose alternate elemental phases.",
                ]
            ),
            G(
                81059,
                "Flaming Fury",
                "Defeat Red Boy and Yaksha King to finish Chapter 5.",
                "Story Boss",
                "Chapter 5",
                steps:
                [
                    "Reach Field of Fire after the Keeper encounter.",
                    "Defeat Red Boy, then complete the Yaksha King phase.",
                ],
                checklist: ["Red Boy", "Yaksha King"]
            ),
            G(
                81060,
                "Treasure Trove",
                "Collect the four Vessels awarded by secret areas and Chapter 5 progression.",
                "Collection",
                "Chapters 1-5",
                steps:
                [
                    "Finish each secret area as it becomes available; Vessels are permanent active tools and major boss counters.",
                    "Confirm all four save-verified Vessel rows are owned.",
                ],
                checklist: ["Fireproof Mantle", "Wind Tamer", "Weaver's Needle", "Plantain Fan"]
            ),
            G(
                81061,
                "Mei of Memory",
                "Open the Great Pagoda path to Mount Mei after completing every secret-area prerequisite.",
                "Secret Area",
                "Endgame",
                prerequisites: ["Reach Chapter 6 and keep the Great Pagoda available in Chapter 3."],
                steps:
                [
                    "Complete Ancient Guanyin Temple in Chapter 1 and Kingdom of Sahali in Chapter 2.",
                    "Complete the Treasure Hunter/Melon Field quest in Chapter 3.",
                    "Complete Purple Cloud Mountain in Chapter 4 and Bishui Cave in Chapter 5.",
                    "Return to the Great Pagoda, inspect the completed murals, and follow Maitreya to Mount Mei.",
                ],
                checklist: ["Ancient Guanyin Temple", "Kingdom of Sahali", "Melon Field", "Purple Cloud Mountain", "Bishui Cave", "Great Pagoda portal"]
            ),
            G(
                81062,
                "Meet the Match",
                "Defeat Erlang, the Sacred Divinity, and the Four Heavenly Kings on the secret-ending path.",
                "Secret Boss",
                "Endgame",
                prerequisites: ["Unlock Mount Mei through the Great Pagoda."],
                steps:
                [
                    "Enter Mount Mei and prepare for the game's longest optional boss sequence.",
                    "Defeat Erlang's human-form encounter, then complete the giant-form Heavenly Kings and Erlang Shen battles.",
                ],
                checklist: ["Erlang, the Sacred Divinity", "Four Heavenly Kings", "Erlang Shen"]
            ),
            G(
                81063,
                "Full of Forms",
                "Unlock all 10 base-game Transformations.",
                "Collection",
                "Chapters 1-6",
                isMissable: true,
                missableNote: "Violet Hail is lost if Duskveil is defeated before Daoist Mi's quest; Dark Thunder depends on the five-chapter Horse Guai quest.",
                steps:
                [
                    "Complete transformation-granting boss fights and side quests before each chapter's point of no return.",
                    "Use the analyzer's 10 save-verified transformation rows to identify the exact missing form.",
                ],
                checklist: ["Red Tides", "Azure Dust", "Ashen Slumber", "Ebon Flow", "Hoarfrost", "Umbral Abyss", "Violet Hail", "Golden Lining", "Dark Thunder", "Azure Dome"]
            ),
            G(
                81064,
                "Brews and Barrels",
                "Collect the 8 non-default Drinks required by the PC/Xbox achievement data.",
                "Collection",
                "Chapters 1-6",
                steps:
                [
                    "Buy every new Shen Monkey drink and collect the drinks found in chapter side routes.",
                    "The analyzer follows the eight runtime requirement IDs; the starting Coconut Wine upgrade line is not a separate required row.",
                ],
                checklist: ["Bluebridge Romance", "Lambbrew", "Worryfree Brew", "Loong Balm", "Jade Essence", "A Thousand Days Inebriation", "Pinebrew", "Sunset of the Nine Skies"]
            ),
            G(
                81065,
                "The Cloud Claimed",
                "Defeat Supreme Inspector in Chapter 6 and claim the Somersault Cloud.",
                "Story Boss",
                "Chapter 6",
                steps:
                [
                    "Follow the opening Mount Huaguo route to Foothills - Verdant Path.",
                    "Defeat Supreme Inspector in the second, full encounter to unlock cloud flight.",
                ]
            ),
            G(
                81066,
                "The Clamor of Frogs",
                "Defeat all six frog bosses, one in each chapter.",
                "Boss Chain",
                "Chapters 1-6",
                steps:
                [
                    "Each chapter hides one large frog boss near water or a side ravine.",
                    "Defeat all six on the same save; later frogs inherit abilities from the earlier encounters.",
                ],
                checklist: ["Baw-Li-Guhh-Lang (Chapter 1)", "Lang-Li-Guhh-Baw (Chapter 2)", "Lang-Li-Guhh-Lang (Chapter 3)", "Baw-Li-Guhh-Baw (Chapter 4)", "Baw-Lang-Lang (Chapter 5)", "Lang-Baw-Baw (Chapter 6)"]
            ),
            G(
                81067,
                "A Curious Collection",
                "Collect all 36 base-game Curios required by the save data; the bonus Wind Chime is not required.",
                "Collection",
                "Chapters 1-6",
                isMissable: true,
                missableNote: "Get Cat Eye Beads from Wandering Wight before Elder Jinchi and kill all nine Lantern Wardens before Captain Wise-Voice for Auspicious Lantern.",
                steps:
                [
                    "Open every gold chest, finish optional boss chains, and buy newly unlocked merchant stock.",
                    "Farm the enemy- and plant-drop Curios last, preferably while wearing Golden Carp and Skull of Turtle Treasure.",
                    "Use the analyzer's 36 per-item ownership rows rather than counting the optional Wind Chime.",
                ],
                checklist:
                [
                    "Fine China Tea Bowl", "Cat Eye Beads", "Agate Jar", "Tiger Tally", "Tridacna Pendant", "Goldflora Hairpin",
                    "Glazed Reliquary", "Thunderstone", "Frostsprout Twig", "Snow Fox Brush", "Maitreya's Orb", "Golden Carp",
                    "Jade Moon Rabbit", "Tablet of the Three Supreme", "Preservation Orb", "Gold Sun Crow", "Cuo Jin-Yin Belt Hook", "Gold Button",
                    "Flame Orb", "Daoist's Basket of Fire and Water", "Waterward Orb", "Amber Prayer Beads", "Celestial Registry Tablet", "Tiger Tendon Belt",
                    "Celestial Birthstone Fragment", "Mani Bead", "Boshan Censer", "Back Scratcher", "Auspicious Lantern", "Gold Spikeplate",
                    "Beast Buddha", "Bronze Buddha Pendant", "Thunderflame Seal", "Virtuous Bamboo Engraving", "Spine in the Sack", "White Seashell Waist Chain",
                ]
            ),
            G(
                81068,
                "The Five Skandhas",
                "Collect all five Skandhas and have Xu Dog refine them into the Five Skandhas Pill.",
                "Collection",
                "Chapters 1-6",
                steps:
                [
                    "Find the four early Skandhas near the monkey-like statue encounters while progressing through Chapters 1-4.",
                    "In Chapter 6, collect the last Skandha after defeating Giant Shigandang.",
                    "Take all five to Xu Dog; the save verifies the finished Five Skandhas Pill, while the component list below is guide-only.",
                ],
                checklist: ["Skandha of Form", "Skandha of Feeling", "Skandha of Thought", "Skandha of Choice", "Skandha of Consciousness", "Five Skandhas Pill from Xu Dog"]
            ),
            G(
                81069,
                "Medicine Meal",
                "Collect 5 world pickups of each Celestial pill type, plus the Five Skandhas Pill: 16 permanent medicines total.",
                "Collection",
                "Chapters 1-6",
                prerequisites: ["World pickups count; Celestial Medicines crafted from Mind Cores do not replace these achievement pickups."],
                steps:
                [
                    "Collect five Celestial Jade Lotus Pills for Health, five Celestial Taiyi Pills for Mana, and five Celestial Nonary Pills for Stamina.",
                    "Complete The Five Skandhas and receive its Pill from Xu Dog.",
                    "The save reports four requirement buckets because each five-pill family is grouped.",
                ],
                checklist: ["5 Celestial Jade Lotus Pills", "5 Celestial Taiyi Pills", "5 Celestial Nonary Pills", "Five Skandhas Pill"]
            ),
            G(
                81070,
                "Treaded Tracks",
                "Defeat every optional Yaoguai Chief and King roaming Mount Huaguo.",
                "Boss Cleanup",
                "Chapter 6",
                steps:
                [
                    "Use Somersault Cloud to sweep the outer edges, pools, plateaus, and poison-stone clearings of the open map.",
                    "The runtime groups this achievement into two save requirements, so the boss list below is a guide checklist rather than one-to-one decoded rows.",
                ],
                checklist: ["All four Poison Chiefs", "Water-Wood Beast", "Jiao-Loong of Waves", "Lang-Baw-Baw", "Son of Stones"]
            ),
            G(
                81071,
                "Guardian of Gear",
                "Obtain Wukong's four story armor pieces and Jingubang in Chapter 6.",
                "Story Collection",
                "Chapter 6",
                steps:
                [
                    "Defeat the four major armor guardians while exploring Mount Huaguo by cloud.",
                    "Equip the full set and enter Water Curtain Cave to claim Jingubang.",
                ],
                checklist: ["Gold Suozi Armor - Gold Armored Rhino", "Lotus Silk Cloudtreaders - Cloudtreading Deer", "Golden Feng-Tail Crown - Feng-Tail General", "Dian-Cui Loong-Soaring Bracers - Emerald-Armed Mantis", "Jingubang - Water Curtain Cave"]
            ),
            G(
                81072,
                "A Duel of Destiny",
                "Defeat the Great Sage's Broken Shell and complete the main ending.",
                "Story Boss",
                "Chapter 6",
                steps:
                [
                    "Enter the Birthstone after assembling Wukong's gear and completing Mount Huaguo.",
                    "Defeat Stone Monkey and both Great Sage's Broken Shell phases.",
                ],
                checklist: ["Stone Monkey", "Great Sage's Broken Shell"]
            ),
            G(
                81073,
                "Portraits Perfected",
                "Fill all 203 Journal portraits: 90 Lesser Yaoguais, 55 Chiefs, 26 Kings, and 32 Characters.",
                "Completion",
                "All Chapters",
                isMissable: true,
                missableNote: "Key missables include Lantern Wardens, Apramana Bat, Daoist Mi, Scorpionlord, and Crane Immortal. Great Sage's Broken Shell registers after starting New Game+.",
                requiresNewGamePlus: true,
                steps:
                [
                    "Talk to every named NPC, finish their quest endings, and defeat every enemy and boss with a portrait.",
                    "Use the four save buckets for category completion, then inspect the in-game Journal for the exact blank silhouette.",
                ],
                checklist: ["90 Lesser Yaoguais", "55 Yaoguai Chiefs", "26 Yaoguai Kings", "32 Characters", "Start New Game+ for the Great Sage's Broken Shell portrait"]
            ),
            G(
                81074,
                "Six Senses Secured",
                "Finish the game with all six relics and begin a New Cycle.",
                "Progression",
                "New Game+",
                requiresNewGamePlus: true,
                prerequisites: ["Defeat the final boss and choose Enter a New Cycle from the main menu."],
                steps:
                [
                    "Complete the first playthrough and make any desired cleanup backup before advancing.",
                    "Choose Enter a New Cycle; the achievement registers as the six relic senses carry into New Game+.",
                ]
            ),
            G(
                81075,
                "Master of Magic",
                "Learn all 7 base spells; Life-Saving Strand is awarded only after entering New Game+.",
                "Collection",
                "New Game+",
                requiresNewGamePlus: true,
                steps:
                [
                    "Story progression supplies most spells; finish the Ring of Fire and Spell Binder side routes before the ending.",
                    "Start New Game+ and continue until Life-Saving Strand is awarded, then rescan the save.",
                ],
                checklist: ["Immobilize", "Ring of Fire", "Spell Binder", "Cloud Step", "Rock Solid", "A Pluck of Many", "Life-Saving Strand (New Game+)"]
            ),
            G(
                81076,
                "Gourds Gathered",
                "Own the 9 gourd lines required by the PC/Xbox save data; Qing-Tian Gourd requires New Game+.",
                "Collection",
                "New Game+",
                requiresNewGamePlus: true,
                steps:
                [
                    "Finish every gourd reward, chest, and Shen Monkey purchase in the first cycle.",
                    "In New Game+, buy Qing-Tian Gourd from Shen Monkey after meeting its Journal prerequisite.",
                    "The nine runtime rows represent gourd lines, so upgraded names can differ from early guide names.",
                ],
                checklist: ["Xiang River Goddess Gourd", "Trailblazer's Scarlet Gourd", "Qing-Tian Gourd", "Plaguebane Gourd", "Stained Jade Gourd", "Immortal Blessing Gourd", "Multi-Glazed Gourd", "Jade Lotus Gourd", "Fiery Gourd"]
            ),
            G(
                81077,
                "Page Preserver",
                "Collect all 14 medicine formulas tracked by the save; Soul Remigration Pill is a New Game+ purchase.",
                "Collection",
                "New Game+",
                requiresNewGamePlus: true,
                steps:
                [
                    "Check Xu Dog and shrine stores after every chapter, then clear formula chests and quest rewards.",
                    "Start New Game+ and buy the Soul Remigration Pill formula from Xu Dog.",
                ],
                checklist: ["Body-Fleeting Powder", "Septenary Heartfire Pill", "Life-Saving Pill", "Ascension Powder", "Soul Remigration Pill", "Essence Decoction", "Tonifying Decoction", "Longevity Decoction", "Fortifying Medicament", "Mirage Pill", "Loong Aura Amplification Pellets", "Evil Repelling Medicament", "Enhanced Ginseng Pellets", "Enhanced Tiger Subduing Pellets"]
            ),
            G(
                81078,
                "Brewer's Bounty",
                "Collect all 27 save-tracked Soaks; Drinks and Gourds have their own separate PC/Xbox achievements.",
                "Collection",
                "New Game+",
                isMissable: true,
                missableNote: "Turtle Tear depends on the missable Apramana Bat route. Guanyin's Willow Leaf is sold only in New Game+.",
                requiresNewGamePlus: true,
                steps:
                [
                    "Buy every Shen Monkey soak, open chapter containers, and complete boss/quest rewards.",
                    "Farm plant and enemy random drops after collecting item-drop Curios.",
                    "Use the analyzer's 27 save-verified rows; this achievement does not combine the separate Drink and Gourd requirements on PC/Xbox.",
                ],
                checklist:
                [
                    "Guanyin's Willow Leaf", "Flower Primes", "Turtle Tear", "Stranded Loong's Whisker", "Mount Lingtai Seedlings", "Breath of Fire",
                    "Celestial Lotus Seeds", "Undying Vine", "Tiger Relic", "Laurel Buds", "Sweet Ice", "Thunderbolt Horn", "Deathstinger",
                    "Purple-Veined Peach Pit", "Bee Mountain Stone", "Iron Pellet", "Slumbering Beetle Husk", "Copper Pill", "Goji Shoots", "Fruit of Dao",
                    "Flame Mediator", "Double-Combed Rooster Blood", "Gall Gem", "Graceful Orchid", "Tender Jade Lotus", "Steel Ginseng", "Goat Skull",
                ]
            ),
            G(
                81079,
                "Mantled with Might",
                "Collect the 71 armor pieces named by the PC/Xbox achievement data; starter and Deluxe Edition extras are excluded.",
                "Collection",
                "New Game+",
                isMissable: true,
                missableNote: "Break four of Venom Daoist's rear arms in his first fight for Venomous Armguard, and defeat Scorpionlord before Duskveil for his armor reward.",
                requiresNewGamePlus: true,
                steps:
                [
                    "Craft every unlocked shrine set before spending rare boss materials elsewhere.",
                    "Farm standalone enemy-drop headgear and armguards, then use the analyzer's 71 exact ownership rows for cleanup.",
                    "Use New Game+ to obtain enough Bull King materials and any missed one-cycle rewards.",
                ],
                checklist:
                [
                    "Crafted sets: Ebongold, Pilgrim, Serpentscale, Bronze, Ochre, Galeguard, Centipede, Golden, Iron, Loongscale, Insect, Non-Pure, Yaksha, Bull King",
                    "Chapter 6 sets: Wukong's story set and the hidden common Wukong set",
                    "Standalone pieces: Earth Spirit Cap, Snout Mask, Skull of Turtle Treasure, Ginseng Cape, Locust Antennae Mask, Grey Wolf Mask, See No Evil",
                    "Standalone pieces: Yin-Yang Daoist Robe, Venomous Armguard, Guanyin's Prayer Beads, Vajra Armguard",
                    "All 71 save-verified item rows owned",
                ]
            ),
            G(
                81080,
                "Staffs and Spears",
                "Craft or obtain all 20 required weapons; three require materials or recipes available in New Game+.",
                "Collection",
                "New Game+",
                requiresNewGamePlus: true,
                steps:
                [
                    "Unlock every weapon tree through story bosses, Loong fights, secret areas, and the true-ending reward.",
                    "In New Game+, revisit Fuban/Bishui content and shrine crafting for the three final weapons.",
                    "A weapon counts as owned even if a later branch was crafted from it; compare the analyzer's exact 20 rows.",
                ],
                checklist:
                [
                    "Wind Bear Staff", "Twin Serpents Staff", "Willow Wood Staff", "Rat Sage Staff", "Loongwreathe Staff", "Cloud-Patterned Stone Staff",
                    "Spikeshaft Staff", "Kang-Jin Staff", "Chu-Bai Spear", "Chitin Staff", "Golden Loong Staff", "Spider Celestial Staff",
                    "Visionary Centipede Staff", "Staff of Blazing Karma", "Bishui Beast Staff", "Jingubang", "Tri-Point Double-Edged Spear",
                    "Adept Spine-Shooting Fuban Staff (New Game+)", "Dark Iron Staff (New Game+)", "Stormflash Loong Staff (New Game+)",
                ]
            ),
            G(
                81081,
                "Final Fulfillment",
                "Unlock the other 80 PC/Xbox achievements on the same platform profile.",
                "Completion",
                "New Game+",
                requiresNewGamePlus: true,
                steps:
                [
                    "Finish the remaining incomplete cards, prioritizing missables before starting another cycle.",
                    "After achievement 80 registers, rest, change area, or restart once if the final platform unlock is delayed.",
                ],
                checklist: ["All other 80 achievements complete"]
            ),
        };

        var duplicateIds = guides
            .GroupBy(guide => guide.Id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        if (duplicateIds.Length > 0)
        {
            throw new InvalidOperationException($"Achievement guide IDs must be unique: {string.Join(", ", duplicateIds)}.");
        }

        var expectedIds = Enumerable.Range(81001, 81).ToArray();
        var actualIds = guides.Select(guide => guide.Id).Order().ToArray();
        if (!actualIds.SequenceEqual(expectedIds))
        {
            var missingIds = expectedIds.Except(actualIds);
            var unexpectedIds = actualIds.Except(expectedIds);
            throw new InvalidOperationException(
                $"Achievement guide catalog must contain exactly IDs 81001-81081. Missing: {string.Join(", ", missingIds)}. Unexpected: {string.Join(", ", unexpectedIds)}."
            );
        }

        return guides.AsReadOnly();
    }

    private static AchievementGuide G(
        int id,
        string name,
        string requirementSummary,
        string category,
        string chapter,
        bool isMissable = false,
        string? missableNote = null,
        bool requiresNewGamePlus = false,
        IReadOnlyList<string>? prerequisites = null,
        IReadOnlyList<string>? steps = null,
        IReadOnlyList<string>? checklist = null
    )
    {
        return new AchievementGuide(
            id,
            name,
            requirementSummary,
            category,
            chapter,
            isMissable,
            missableNote,
            requiresNewGamePlus,
            prerequisites ?? [],
            steps ?? [],
            checklist ?? []
        );
    }
}
