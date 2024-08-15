using LightningRod.Libraries.Byml;
using LibHac.Fs.Fsa;
using ZstdNet;

namespace LightningRod.Randomizers;
public class WeaponKitRandomizer {

    private static WeaponKitConfig? config;
    private static IFileSystem files;
    private readonly string savePath;

    public WeaponKitRandomizer(WeaponKitConfig kitConfig, IFileSystem fileSys, string save) {
        config = kitConfig;
        files = fileSys;
        savePath = save;
    }

    public void Randomize(long seed, string version) {
        LongRNG rand = new(seed);
        RandomizerUtil.DebugPrint("Loading Weapon Kit randomizer with seed " + seed + " & file version " + version);

        BymlArrayNode weaponMain = files.ReadCompressedByml($"/RSDB/WeaponInfoMain.Product.{version}.rstbl.byml.zs");
        BymlArrayNode weaponSub = files.ReadCompressedByml($"/RSDB/WeaponInfoSub.Product.{version}.rstbl.byml.zs");
        BymlArrayNode weaponSpecial = files.ReadCompressedByml($"/RSDB/WeaponInfoSpecial.Product.{version}.rstbl.byml.zs");

        List<string> MainBanList = ["_Coop", "_Msn", "_Sdodr", "RivalLv1", "RivalLv2"];
        List<string> SubBanList = ["_Mission", "_Rival", "_Sdodr", "SalmonBuddy"];
        List<string> SpecialBanList = ["_Mission", "Sdodr", "_Rival", "_Coop"];

        if (!config.heroSubSelection) SubBanList.Add("_Hero");
        if (!config.coopSplatBomb) SubBanList.Add("_Coop");
        if (config.allSubWeapons) SubBanList = [""];

        if (!config.heroModeSuperLanding) SpecialBanList.Add("SuperLanding");
        if (!config.useRainmaker) SpecialBanList.Add("Gachihoko");
        if (!config.useIkuraShoot) SpecialBanList.Add("IkuraShoot");
        if (config.useAllSpecials) SpecialBanList = [""];

        int minPFS = config.include170To220p ? 170 : 180;
        int maxPFS = config.include170To220p ? 220 : 210;

        List<string> subList = [];
        List<string> specialList = [];

        for (int i = 0; i < weaponSub.Length; i++) {
            string RowId = ((weaponSub[i] as BymlHashTable)["__RowId"] as BymlNode<string>).Data;
            if (!SubBanList.Any(RowId.Contains)) subList.Add(RowId);
        }

        for (int i = 0; i < weaponSpecial.Length; i++) {
            string RowId = ((weaponSpecial[i] as BymlHashTable)["__RowId"] as BymlNode<string>).Data;
            if (!SpecialBanList.Any(RowId.Contains)) specialList.Add(RowId);
        }

        Dictionary<string, int> periscopeIndexes = [];

        for (int i = 0; i < weaponMain.Length; i++) {
            BymlHashTable? mainData = weaponMain[i] as BymlHashTable;

            if (MainBanList.Any((mainData["__RowId"] as BymlNode<string>).Data.Contains)) continue;

            int pfs = rand.NextInt(maxPFS - minPFS) + minPFS + 5; // better odds towards 220 (sorry machine mains)
            if (!config.noPFSIncrementation) pfs = pfs / 10 * 10;

            ((BymlNode<int>)mainData["SpecialPoint"]).Data = pfs;

            (mainData["SubWeapon"] as BymlNode<string>).Data = $"Work/Gyml/{subList[rand.NextInt(subList.Count)]}.spl__WeaponInfoSub.gyml";
            (mainData["SpecialWeapon"] as BymlNode<string>).Data = $"Work/Gyml/{specialList[rand.NextInt(specialList.Count)]}.spl__WeaponInfoSpecial.gyml";

            if (config.matchPeriscopeKits && (mainData["__RowId"] as BymlNode<string>).Data.Contains("Charger")) {
                periscopeIndexes.Add((mainData["__RowId"] as BymlNode<string>).Data, i);
            }
        }

        if (config.matchPeriscopeKits) {
            foreach (string key in periscopeIndexes.Keys) {
                if (key.Contains("Scope")) {
                    ((weaponMain[periscopeIndexes[key]] as BymlHashTable)["SubWeapon"] as BymlNode<string>).Data = ((weaponMain[periscopeIndexes[key.Replace("Scope", "")]] as BymlHashTable)["SubWeapon"] as BymlNode<string>).Data;
                    ((weaponMain[periscopeIndexes[key]] as BymlHashTable)["SpecialWeapon"] as BymlNode<string>).Data = ((weaponMain[periscopeIndexes[key.Replace("Scope", "")]] as BymlHashTable)["SpecialWeapon"] as BymlNode<string>).Data;
                }
            }
        }

        Directory.CreateDirectory(savePath + "/romfs/RSDB");
        Stream streamwrite = File.Create($"{savePath}/romfs/RSDB/WeaponInfoMain.Product.{version}.rstbl.byml.zs");
        
        using (MemoryStream prezs = new()) {
            new Byml(weaponMain).Save(prezs); // save new byml with weaponMain as root
            RandomizerUtil.DebugPrint("Saved Weapon byml");

            using Compressor compressor = new();
            var compressedByml = compressor.Wrap(prezs.ToArray());
            streamwrite.Write(compressedByml, 0, compressedByml.Length);
        }
        
        streamwrite.Flush();
        streamwrite.Close();
    }

    public class WeaponKitConfig(bool hss, bool csb, bool asw, bool hmsl, bool ur, bool uis, bool uas, bool i1t2, bool npi, bool mpk, bool rk)
    {
        
        public bool heroSubSelection = hss; // Initial value
        public bool coopSplatBomb = csb;
        public bool allSubWeapons = asw;
        public bool heroModeSuperLanding = hmsl;
        public bool useRainmaker = ur;
        public bool useIkuraShoot = uis;
        public bool useAllSpecials = uas;
        public bool include170To220p = i1t2;
        public bool noPFSIncrementation = npi;
        public bool matchPeriscopeKits = mpk;
        public bool randomizeKits = rk;
    }
}