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
    private string _assemblyName;
    private string _targetFramework = "net462";
    private string _fileExtension = ".dll";
    private WeaverHelper _weaverHelper;


    public WeaverFixture SetAssemblyName(string assemblyName)
    {
        if (assemblyName == null)
        {
            throw new ArgumentNullException(nameof(assemblyName));
        }

        lock (_lock)
        {
            if(!string.Equals(assemblyName, _assemblyName, StringComparison.OrdinalIgnoreCase))
            {
                _assemblyName = assemblyName;
                _weaverHelper = null;
            }
            return this;
        }
    }

    public WeaverFixture SetTargetFramework(string targetFramework)
    {
        if (targetFramework == null)
        {
            throw new ArgumentNullException(nameof(targetFramework));
        }

        lock (_lock)
        {
            if (!string.Equals(targetFramework, _targetFramework, StringComparison.OrdinalIgnoreCase))
            {
                _targetFramework = targetFramework;
                _weaverHelper = null;
            }
            return this;
        }
    }

    public WeaverFixture SetExtension(string extension)
    {
        if (extension == null)
        {
            throw new ArgumentNullException(nameof(extension));
        }

        lock (_lock)
        {
            if (!string.Equals(extension, _fileExtension, StringComparison.OrdinalIgnoreCase))
            {
                _fileExtension = extension;
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
                _weaverHelper = new WeaverHelper(_assemblyName, _targetFramework, _fileExtension);
            }
            return _weaverHelper;
        }
    }
}