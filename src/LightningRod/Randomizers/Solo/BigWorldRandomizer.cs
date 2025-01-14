using LibHac.FsSystem;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using LightningRod.Randomizers.Solo.BigWorld;
using LightningRod.Utilities;

namespace LightningRod.Randomizers;

public static class BigWorldRandomizer
{
    public static void Randomize()
    {
        Logger.Log("Starting Alterna randomizer!");

        BigWorldStageRandomizer.Randomize();
        BigWorldWeaponRandomizer.Randomize();
    }

    public enum SupplyWeaponType
    {
        Normal = 0,
        Hero = 1,
        Special = 2,
        MainAndSpecial = 3,
    }
}
