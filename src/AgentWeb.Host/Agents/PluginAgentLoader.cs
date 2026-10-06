using System.Reflection;
using System.Runtime.Loader;
using AgentWeb.Contracts;

namespace AgentWeb.Host.Agents;

public static class PluginAgentLoader
{
    public static IReadOnlyList<ISiteAgent> Load(string pluginDirectory)
    {
        if (!Directory.Exists(pluginDirectory))
            return [];

        var agents = new List<ISiteAgent>();

        foreach (var dll in Directory.EnumerateFiles(pluginDirectory, "*.dll"))
        {
            Assembly assembly;
            try
            {
                assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(dll));
            }
            catch (Exception)
            {
                continue;
            }

            foreach (var type in assembly.GetTypes()
                         .Where(type => typeof(ISiteAgent).IsAssignableFrom(type)
                                        && type is { IsClass: true, IsAbstract: false }))
            {
                if (Activator.CreateInstance(type) is ISiteAgent agent)
                    agents.Add(agent);
            }
        }

        return agents;
    }
}
