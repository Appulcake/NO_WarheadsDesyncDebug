using HarmonyLib;
using Mirage;
using NuclearOption.SavedMission;

namespace NO_WDD;

[HarmonyPatch]
internal class HarmonyPatches
{
    [HarmonyPatch(typeof(WarheadStorage), nameof(WarheadStorage.Disable))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static void WarheadStorage_DisablePrefix(WarheadStorage __instance)
    {
        var unit = __instance.attachedUnit;
        var airbase = unit != null ? unit.GetAirbase() : null;
        
        Plugin.Logger.LogWarning(
            $"Storage stats before disable ({WarheadDebugger.Role()}):\n" +
            $"NetId: {__instance.NetId} | " +
            $"number: {__instance.number} | " +
            $"selfDisabled: {__instance.selfDisabled} | " +
            $"unitDisabled: {unit?.disabled} | " +
            $"unit: \"{unit?.UniqueName}\" | " +
            $"GetAirbase: \"{airbase?.SavedAirbase?.UniqueName ?? "Null"}\"");
    }
    
    [HarmonyPatch(typeof(WarheadStorage), nameof(WarheadStorage.Disable))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void WarheadStorage_DisablePostfix(WarheadStorage __instance)
    {
        var unit = __instance.attachedUnit;
        var airbase = unit != null ? unit.GetAirbase() : null;
        
        Plugin.Logger.LogWarning(
            $"Storage stats after disable ({WarheadDebugger.Role()}):\n" +
            $"NetId: {__instance.NetId} | " +
            $"number: {__instance.number} | " +
            $"selfDisabled: {__instance.selfDisabled} | " +
            $"unitDisabled: {unit?.disabled} | " +
            $"unit: \"{unit?.UniqueName}\" | " +
            $"GetAirbase: \"{airbase?.SavedAirbase?.UniqueName ?? "Null"}\"");
        
        if (__instance is { selfDisabled: true, number: > 0 })
            Plugin.Logger.LogError("--- Server disabled WarheadStorage still has positive number!");
    }
    
    [HarmonyPatch(typeof(WarheadStorage), nameof(WarheadStorage.Repair))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static void WarheadStorage_RepairPrefix(WarheadStorage __instance)
    {
        Plugin.Logger.LogWarning(
            $"Storage stats before repair ({WarheadDebugger.Role()}):\n" +
            $"NetId: {__instance.NetId} | " +
            $"number: {__instance.number} | " +
            $"selfDisabled: {__instance.selfDisabled} | " +
            $"unitDisabled: {__instance.attachedUnit?.disabled}");
    }
    
    
    [HarmonyPatch(typeof(WarheadStorage), nameof(WarheadStorage.Repair))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void WarheadStorage_RepairPostfix(WarheadStorage __instance)
    {
        Plugin.Logger.LogWarning(
            $"Storage stats after repair ({WarheadDebugger.Role()}):\n" +
            $"NetId: {__instance.NetId} | " +
            $"number: {__instance.number} | " +
            $"selfDisabled: {__instance.selfDisabled} | " +
            $"unitDisabled: {__instance.attachedUnit?.disabled}");
        
        if (__instance.selfDisabled &&
            __instance.attachedUnit != null &&
            !__instance.attachedUnit.disabled)
            Plugin.Logger.LogError("--- Repaired unit, but storage remained selfDisabled!");
    }
    
    [HarmonyPatch(typeof(WarheadStorage), nameof(WarheadStorage.Networknumber), MethodType.Setter)]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static void WarheadStorage_Networknumber_SetterPrefix(WarheadStorage __instance, int value)
    {
        if (value == __instance.number)
            return;
        
        if (value > __instance.number && __instance.Disabled)
            Plugin.Logger.LogError(
                $"--- Adding warheads to disabled storage!\n" +
                $"NetId: {__instance.NetId} | " +
                $"old number: {__instance.number} => new number: {value} | " +
                $"selfDisabled: {__instance.selfDisabled} | " +
                $"unitDisabled: {__instance.attachedUnit?.disabled} | " +
                $"unit: \"{__instance.attachedUnit?.UniqueName}\"");
    }
    
    [HarmonyPatch(typeof(AircraftSelectionMenu), nameof(AircraftSelectionMenu.Refresh))]
    [HarmonyPostfix]
    private static void AircraftSelectionMenu_Refresh_Postfix(Airbase airbase)
    {
        Plugin.Logger.LogWarning(
            "Airbase selection menu warhead dump:\n" + WarheadDebugger.DumpAirbaseWarheads(airbase));
    }
    
    [HarmonyPatch(typeof(Spawner), nameof(Spawner.TrySpawnAircraft), typeof(Airbase), typeof(AircraftDefinition),
        typeof(LiveryKey), typeof(Loadout), typeof(float), typeof(INetworkPlayer))]
    [HarmonyPrefix]
    private static void Spawner_TrySpawnAircraftPrefix(Airbase airbase, AircraftDefinition definition, Loadout loadout)
    {
        Plugin.Logger.LogWarning(
            $"Server spawn request from aircraft: \"{definition?.unitName}\", " +
            "warhead dump:\n" + WarheadDebugger.DumpAirbaseWarheads(airbase));
    }
    
    [HarmonyPatch(typeof(Building), nameof(Building.UnitDisabled))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void Building_UnitDisabledPostfix(Building __instance, bool oldState, bool newState)
    {
        if (!oldState || newState)
            return;
        
        if (!__instance.TryGetComponent<WarheadStorage>(out var storage))
            return;
        
        Plugin.Logger.LogWarning(
            $"Warhead storage building re-enabled ({WarheadDebugger.Role()}):\n" +
            $"Building: \"{__instance.UniqueName}\" | " +
            $"NetId: {storage.NetId} | " +
            $"number: {storage.number} | " +
            $"Disabled: {storage.Disabled} | " +
            $"selfDisabled: {storage.selfDisabled} | " +
            $"unitDisabled: {__instance.disabled} | " +
            $"GetAirbase: \"{__instance.GetAirbase()?.SavedAirbase?.UniqueName ?? "Null"}\"");
        
        if (storage.selfDisabled && !__instance.disabled)
        {
            Plugin.Logger.LogError(
                "--- Storage building was re-enabled while WarheadStorage remained selfDisabled!");
        }
    }
}