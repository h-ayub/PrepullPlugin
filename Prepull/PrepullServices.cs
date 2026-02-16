using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace Prepull;

#pragma warning disable CS8618
internal class PrepullServices
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; set; }
    [PluginService] public static ITextureProvider TextureProvider { get; set; }
    [PluginService] public static ICommandManager CommandManager { get; set; }
    [PluginService] public static ICondition Condition { get; set; }
    [PluginService] public static IClientState ClientState { get; set; }
    [PluginService] public static IDataManager DataManager { get; set; }
    [PluginService] public static IDutyState DutyState { get; set; }
    [PluginService] public static IBuddyList BuddyList { get; set; }
    [PluginService] public static IChatGui ChatGui { get; set; }
}
