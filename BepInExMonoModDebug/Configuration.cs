using BepInEx.Configuration;

namespace BepInExMonoModDebug;
internal class Configuration
{
    public ConfigEntry<bool> ShouldSaveDumpFile { get; }

    public Configuration(ConfigFile configFile)
    {
        ShouldSaveDumpFile = configFile.Bind("Dumps", "Save", false, "Should save patched assemblies in 'BepInEx/dumps' directory\nThis setting is for developers!");

        configFile.Save();
    }
}
