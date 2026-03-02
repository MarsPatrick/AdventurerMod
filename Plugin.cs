using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Dungeonator;
using Gunfiguration;

namespace AdventurerMod
{
    [BepInDependency(ETGModMainBehaviour.GUID)]
    [BepInDependency(Gunfiguration.C.MOD_GUID)]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class LostAdventurerPlugin : BaseUnityPlugin
    {
        public const string GUID = "com.MarsPatrick.lostadventurer";
        public const string NAME = "Lost Adventurer Mod";
        public const string VERSION = "1.0.0";

        public const string KEY_SHOW_UI = "show_ui";
        public const string KEY_SHOW_FLOOR = "show_floor";
        public const string KEY_SHOW_PATCH_LOG = "show_patch_log";
        public const string KEY_SHOW_BLUEPRINT = "show_blueprint";
        public const string KEY_FORCE_FLOOR = "force_floor";

        public static Gunfig Config;
        public static string patchLog = "";
        public static string blueprintLog = "";

        void Awake()
        {
            ETGModConsole.Log($"Initialized {NAME} v{VERSION}");
            SetupGunfig();
            new Harmony(GUID).PatchAll();
        }

        private void SetupGunfig()
        {
            Config = Gunfig.Get(NAME.WithColor(new Color(1f, 0.8f, 0.2f)));

            Config.AddLabel("-- UI --".Cyan());
            Config.AddToggle(KEY_SHOW_UI, enabled: true, label: "Mostrar pisos ayudados");
            Config.AddToggle(KEY_SHOW_FLOOR, enabled: true, label: "Mostrar piso actual");
            Config.AddToggle(KEY_SHOW_PATCH_LOG, enabled: true, label: "Mostrar patch log");
            Config.AddToggle(KEY_SHOW_BLUEPRINT, enabled: true, label: "Mostrar blueprint log");

            Config.AddLabel("-- Forzar Aparicion --".Cyan());
            Config.AddScrollBox(
                key: KEY_FORCE_FLOOR,
                options: new List<string> { "Ninguno", "Castle", "Gungeon", "Mines", "Catacombs", "Forge" },
                label: "Forzar en piso",
                updateType: Gunfig.Update.Immediate,
                info: new List<string> {
                    "No forzar en ningun piso",
                    "Forzar aparicion en Castillo",
                    "Forzar aparicion en Gungeon",
                    "Forzar aparicion en Minas",
                    "Forzar aparicion en Catacumbas",
                    "Forzar aparicion en Forja"
                }
            );
        }

        public static GlobalDungeonData.ValidTilesets? ForceFloorFromConfig()
        {
            switch (Config.Value(KEY_FORCE_FLOOR))
            {
                case "Castle": return GlobalDungeonData.ValidTilesets.CASTLEGEON;
                case "Gungeon": return GlobalDungeonData.ValidTilesets.GUNGEON;
                case "Mines": return GlobalDungeonData.ValidTilesets.MINEGEON;
                case "Catacombs": return GlobalDungeonData.ValidTilesets.CATACOMBGEON;
                case "Forge": return GlobalDungeonData.ValidTilesets.FORGEGEON;
                default: return null;
            }
        }

        public static List<GlobalDungeonData.ValidTilesets> AllFloors()
        {
            return new List<GlobalDungeonData.ValidTilesets>
            {
                GlobalDungeonData.ValidTilesets.CASTLEGEON,
                GlobalDungeonData.ValidTilesets.GUNGEON,
                GlobalDungeonData.ValidTilesets.MINEGEON,
                GlobalDungeonData.ValidTilesets.CATACOMBGEON,
                GlobalDungeonData.ValidTilesets.FORGEGEON
            };
        }

        public static List<GlobalDungeonData.ValidTilesets> GetFloorsHelpedList()
        {
            var result = new List<GlobalDungeonData.ValidTilesets>();
            if (GameStatsManager.Instance == null) return result;
            foreach (var floor in AllFloors())
            {
                GungeonFlags? flag = FlagFromFloor(floor);
                if (flag.HasValue && GameStatsManager.Instance.GetFlag(flag.Value))
                    result.Add(floor);
            }
            return result;
        }

        public static int GetFloorsHelped() => GetFloorsHelpedList().Count;

        public static GungeonFlags? FlagFromFloor(GlobalDungeonData.ValidTilesets floor)
        {
            switch (floor)
            {
                case GlobalDungeonData.ValidTilesets.CASTLEGEON: return GungeonFlags.LOST_ADVENTURER_HELPED_CASTLE;
                case GlobalDungeonData.ValidTilesets.GUNGEON: return GungeonFlags.LOST_ADVENTURER_HELPED_GUNGEON;
                case GlobalDungeonData.ValidTilesets.MINEGEON: return GungeonFlags.LOST_ADVENTURER_HELPED_MINES;
                case GlobalDungeonData.ValidTilesets.CATACOMBGEON: return GungeonFlags.LOST_ADVENTURER_HELPED_CATACOMBS;
                case GlobalDungeonData.ValidTilesets.FORGEGEON: return GungeonFlags.LOST_ADVENTURER_HELPED_FORGE;
                default: return null;
            }
        }

        public static string FloorName(GlobalDungeonData.ValidTilesets floor)
        {
            switch (floor)
            {
                case GlobalDungeonData.ValidTilesets.CASTLEGEON: return "Castle";
                case GlobalDungeonData.ValidTilesets.GUNGEON: return "Gungeon";
                case GlobalDungeonData.ValidTilesets.MINEGEON: return "Mines";
                case GlobalDungeonData.ValidTilesets.CATACOMBGEON: return "Catacombs";
                case GlobalDungeonData.ValidTilesets.FORGEGEON: return "Forge";
                default: return floor.ToString();
            }
        }

        private List<GlobalDungeonData.ValidTilesets> _helpedFloors = new List<GlobalDungeonData.ValidTilesets>();
        private GlobalDungeonData.ValidTilesets _currentFloor;
        private bool _cellGenerated;
        private bool _currentFloorHelped;

        void Update()
        {
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.BestGenerationDungeonPrefab == null) return;
            if (GameStatsManager.Instance == null) return;
            _helpedFloors = GetFloorsHelpedList();
            _currentFloor = GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId;
            _cellGenerated = MetaInjectionData.CellGeneratedForCurrentBlueprint;
            GungeonFlags? flag = FlagFromFloor(_currentFloor);
            _currentFloorHelped = flag.HasValue && GameStatsManager.Instance.GetFlag(flag.Value);
        }

        void OnGUI()
        {
            if (Config == null) return;

            float y = 10f;

            // --- Pisos ayudados ---
            if (Config.Enabled(KEY_SHOW_UI))
            {
                GUIStyle styleGreen = new GUIStyle(GUI.skin.label)
                { fontSize = 18, fontStyle = FontStyle.Bold, normal = { textColor = Color.green } };
                GUIStyle styleRed = new GUIStyle(GUI.skin.label)
                { fontSize = 18, fontStyle = FontStyle.Bold, normal = { textColor = Color.red } };

                float x = 10f;
                foreach (var floor in AllFloors())
                {
                    bool helped = _helpedFloors.Contains(floor);
                    GUI.Label(new Rect(x, y, 130f, 24f),
                        (helped ? "✓ " : "✗ ") + FloorName(floor),
                        helped ? styleGreen : styleRed);
                    x += 140f;
                }
                y += 28f;
            }

            // --- Piso actual ---
            if (Config.Enabled(KEY_SHOW_FLOOR))
            {
                GUIStyle styleYellow = new GUIStyle(GUI.skin.label)
                { fontSize = 14, normal = { textColor = Color.yellow } };
                GUI.Label(new Rect(10f, y, 900f, 22f),
                    $"Floor: {_currentFloor} | Ayudados: {_helpedFloors.Count}/5 | CellGen: {_cellGenerated} | FloorHelped: {_currentFloorHelped}",
                    styleYellow);
                y += 26f;
            }

            // --- Patch log ---
            if (Config.Enabled(KEY_SHOW_PATCH_LOG) && !string.IsNullOrEmpty(patchLog))
            {
                GUIStyle styleCyan = new GUIStyle(GUI.skin.label)
                { fontSize = 13, normal = { textColor = Color.cyan } };
                GUI.Label(new Rect(10f, y, 1200f, 22f), patchLog, styleCyan);
                y += 24f;
            }

            // --- Blueprint log ---
            if (Config.Enabled(KEY_SHOW_BLUEPRINT) && !string.IsNullOrEmpty(blueprintLog))
            {
                GUIStyle styleWhite = new GUIStyle(GUI.skin.label)
                { fontSize = 13, normal = { textColor = Color.white } };
                GUI.Label(new Rect(10f, y, 1200f, 22f), blueprintLog, styleWhite);
            }
        }
    }

    [HarmonyPatch]
    public static class ShouldDoLostAdventurerHelpPatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(LoopFlowBuilder).GetMethod(
                "ShouldDoLostAdventurerHelp",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
        }

        static void Prefix(SharedInjectionData injectionData)
        {
            if (GameManager.Instance == null || GameStatsManager.Instance == null) return;
            var floor = GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId;
            if (floor != GlobalDungeonData.ValidTilesets.FORGEGEON) return;

            string bpLog = "Blueprint Forge: ";
            if (MetaInjectionData.CurrentRunBlueprint != null &&
                MetaInjectionData.CurrentRunBlueprint.ContainsKey(GlobalDungeonData.ValidTilesets.FORGEGEON))
            {
                foreach (var entry in MetaInjectionData.CurrentRunBlueprint[GlobalDungeonData.ValidTilesets.FORGEGEON])
                    bpLog += $"[{entry.injectionData.name}] ";
            }
            else
                bpLog += "SIN ENTRIES";

            LostAdventurerPlugin.blueprintLog = bpLog;

            int count = LostAdventurerPlugin.GetFloorsHelped();
            bool forgeFlag = GameStatsManager.Instance.GetFlag(GungeonFlags.LOST_ADVENTURER_HELPED_FORGE);
            string annotations = "";
            for (int i = 0; i < injectionData.InjectionData.Count; i++)
                annotations += $"[{i}:{injectionData.InjectionData[i].annotation}] ";

            string log = $"[PATCH] Floor:{floor} | ConditionMet:{count == 4 && !forgeFlag} | Annotations: {annotations}";
            LostAdventurerPlugin.patchLog = log;
            ETGModConsole.Log(log);
        }
    }

    [HarmonyPatch]
    public static class ProcessSingleNodeInjectionPatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(LoopFlowBuilder).GetMethod(
                "ProcessSingleNodeInjection",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
        }

        static void Prefix(object __instance, ProceduralFlowModifierData currentInjectionData,
            BuilderFlowNode root, RuntimeInjectionFlags injectionFlags,
            FlowCompositeMetastructure metastructure, RuntimeInjectionMetadata optionalMetadata)
        {
            if (currentInjectionData == null) return;
            if (currentInjectionData.annotation != "lost adventurer") return;
            if (GameManager.Instance == null || GameStatsManager.Instance == null) return;
            if (LostAdventurerPlugin.Config == null) return;

            var floor = GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId;
            GlobalDungeonData.ValidTilesets? forceFloor = LostAdventurerPlugin.ForceFloorFromConfig();

            ETGModConsole.Log($"[PATCH] Prefix | floor:{floor} | forceFloor:{(forceFloor.HasValue ? forceFloor.Value.ToString() : "null")} | chanceToSpawn:{currentInjectionData.chanceToSpawn}");

            if (!forceFloor.HasValue || floor != forceFloor.Value) return;

            GungeonFlags? currentFlag = LostAdventurerPlugin.FlagFromFloor(floor);
            if (currentFlag.HasValue && GameStatsManager.Instance.GetFlag(currentFlag.Value)) return;

            if (LostAdventurerPlugin.GetFloorsHelped() != 4) return;

            if (optionalMetadata?.SucceededRandomizationCheckMap != null &&
                optionalMetadata.SucceededRandomizationCheckMap.ContainsKey(currentInjectionData))
            {
                optionalMetadata.SucceededRandomizationCheckMap.Remove(currentInjectionData);
                ETGModConsole.Log("[PATCH] Cache limpiado");
            }

            currentInjectionData.chanceToSpawn = 1f;
            string log = $"[PATCH] chanceToSpawn=1 forzado | floor:{floor} | room:{currentInjectionData.exactRoom?.name ?? "NULL"}";
            LostAdventurerPlugin.patchLog = log;
            ETGModConsole.Log(log);
        }

        static void Postfix(bool __result, ProceduralFlowModifierData currentInjectionData)
        {
            if (currentInjectionData == null) return;
            if (currentInjectionData.annotation != "lost adventurer") return;

            string log = $"[PATCH] RESULT:{__result} | chanceToSpawn:{currentInjectionData.chanceToSpawn}";
            LostAdventurerPlugin.patchLog = log;
            ETGModConsole.Log(log);
        }
    }
}