using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Commander.Fody;
using Mono.Cecil;

public class WeaverFixture
{
    private readonly object _lock = new object();
    private string _projectPath;
    private Action<ModuleWeaver> _configure;
    private WeaverHelper _weaverHelper;

    public WeaverFixture SetProjectPath(string projectPath)
    {
        if (projectPath == null)
        {
            throw new ArgumentNullException(nameof(projectPath));
        }

        lock (_lock)
        {
            if(!string.Equals(projectPath, _projectPath, StringComparison.OrdinalIgnoreCase))
            {
                _projectPath = projectPath;
                _weaverHelper = null;
            }
            return this;
        }
    }

    public WeaverFixture ConfigureWeaver(Action<ModuleWeaver> configure)
    {
        lock (_lock)
        {
            if(!Object.Equals(configure, _configure))
            {
                _configure = configure;
                _weaverHelper = null;
            }
            return this;
        }
    }

    public WeaverHelper GetWeaverHelper()
    {
        lock (_lock)
        {
            if(_weaverHelper == null)
            {
                _weaverHelper = new WeaverHelper(_projectPath, _configure);
            }
            return _weaverHelper;
        }
    }
}