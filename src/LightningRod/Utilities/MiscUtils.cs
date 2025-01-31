using System.Text;
using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.FsSystem;
using LightningRod.Libraries;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using ZstdNet;

namespace LightningRod.Utilities;

public static class MiscUtils
{
    public static void CreateFolder(string folderName)
    {
        Directory.CreateDirectory($"{GameData.DataPath}/{folderName}");
    }

    public static BymlArrayNode RandomizeData(this BymlArrayNode arrayNode, LongRNG rand)
    {
        foreach (BymlNode<float> bymlNode in arrayNode.Array)
        {
            bymlNode.Data = rand.NextFloat() * 2 * bymlNode.Data;
        }
        return arrayNode;
    }

    public static void SimpleAddNode<T>(
        this BymlHashTable table,
        string name,
        T data,
        BymlNodeId nodeId
    )
    {
        table.AddNode(nodeId, new BymlNode<T>(nodeId, data), name);
    }

    public static void AddHashPair<T>(
        this BymlHashTable table,
        string name,
        T data,
        BymlNodeId nodeId
    )
    {
        BymlHashPair pair = new BymlHashPair();
        pair.Name = name;
        pair.Id = BymlNodeId.Hash;
        pair.Value = new BymlNode<T>(nodeId, data);
        table.Pairs.Add(pair);
    }

    public static string GetRandomIndex(this List<Weapon> weaponTable, WeaponType constraint)
    {
        Weapon data = new("TempWeapon", WeaponType.Other);
        int tableLength = weaponTable.Count;
        while (!(data.Type == constraint))
            data = weaponTable[GameData.Random.NextInt(tableLength)];
        return data.Name;
    }

    public static List<string> GetAllWeaponsOfType(this List<Weapon> weaponTable, WeaponType constraint)
    {
        List<string> weaponList = [];
        foreach (Weapon weapon in weaponTable)
        {
            if (weapon.Type == constraint)
                weaponList.Add(weapon.Name);
        }
        return weaponList;
    }

    public static string Compile(this string uncompiledString)
    {
        return uncompiledString.Replace("Work/", "").Replace(".gyml", ".bgyml");
    }

    // https://stackoverflow.com/questions/16100/convert-a-string-to-an-enum-in-c-sharp
    public static T ToEnum<T>(this string value)
    {
        return (T) Enum.Parse(typeof(T), value, true);
    }

    public static bool RandomChance(int chance)
    {
        return GameData.Random.NextInt(100) < chance;
    }

    public static bool IsOfAnyType<T>(this T value, List<T> types)
    {
        for (int i = 0; i < types.Count; i++)
            if (value.Equals(types[i])) return true;
        return false;
    }
}
