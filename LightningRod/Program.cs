using LibHac.FsSystem;

namespace LightningRod;

public class Program
{
    public static void Main(string[] args)
    {
        BaseHandler handler = new BaseHandler(
            new LocalFileSystem(@"E:\Dumped Games\Splatoon 3\9.1.0\Program\Data")
        );

        handler.TriggerRandomizers(
            0123456789012345,
            new Randomizers.WeaponKitRandomizer.WeaponKitConfig( // lol
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                true,
                false,
                true,
                true
            ),
            new Randomizers.VSStageRandomizer.VSStageConfig(
                true,
                true,
                true,
                10,
                true,
                true,
                true,
                true
            ),
            new Randomizers.ParameterRandomizer.ParameterConfig(
                true,
                1,
                true,
                true,
                true
            ),
            @"C:\Users\Ash\Documents\TestData"
        );
    }
}