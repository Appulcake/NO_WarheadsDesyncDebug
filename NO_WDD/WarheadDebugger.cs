using System.Text;
using NuclearOption.Networking;

namespace NO_WDD;

internal static class WarheadDebugger
{
    internal static string Role()
    {
        var server = NetworkManagerNuclearOption.i.Server.Active;
        var client = NetworkManagerNuclearOption.i.Client.Active;
        
        if (server && client)
            return "Host";
        if (server)
            return "Server";
        if (client)
            return "Client";
        
        return "Unknown role";
    }
    
    internal static string DumpAirbaseWarheads(Airbase airbase)
    {
        if (airbase == null)
            return "(null airbase)";
        
        var sb = new StringBuilder();
        
        sb.Append(
            $"({Role()}) Airbase: \"{airbase.SavedAirbase?.DisplayName}\" | " +
            $"Unique: \"{airbase.SavedAirbase?.UniqueName}\" | " +
            $"NetId: {airbase.NetId} | " +
            $"HQ: \"{airbase.CurrentHQ?.faction?.factionName}\" | " +
            $"GetWarheads(): {airbase.GetWarheads()} | " +
            $"Stores: {airbase.stores.Count} ");
        
        for (var i = 0; i < airbase.stores.Count; i++)
        {
            var storage = airbase.stores[i];
            
            if (storage == null)
            {
                sb.Append($"| Store[{i}] is null ");
                continue;
            }
            
            var unit = storage.attachedUnit;
            var returnedAirbase = unit != null ? unit.GetAirbase() : null;
            
            sb.Append(
                $"| Store[{i}] " +
                $"netId: {storage.NetId} " +
                $"number: {storage.number} " +
                $"Disabled: {storage.Disabled} " +
                $"selfDisabled: {storage.selfDisabled} " +
                $"unitDisabled: {unit?.disabled} " +
                $"unit: \"{unit?.UniqueName}\" " +
                $"unitPID: {unit?.persistentID} " +
                $"GetAirbase: \"{returnedAirbase?.SavedAirbase?.UniqueName ?? "Null"}\"");
        }
        
        return sb.ToString();
    }
}