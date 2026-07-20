using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using System.Runtime.Versioning;

namespace Prepull.Classes.Extensions;

[SupportedOSPlatform("windows")]

public static class StatusExtensions
{
    /// <summary>
    /// Checks if the StatusList contains a specific status ID.
    /// </summary>
    public static bool HasStatus(this StatusList statusList, uint statusId)
    {
        foreach (var status in statusList)
        {
            if (status.StatusId == statusId)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if an IBattleChara has a specific status ID.
    /// </summary>
    public static bool HasStatus(this IBattleChara character, uint statusId)
    {
        return character.StatusList.HasStatus(statusId);
    }
}
