using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;
using Mono.Cecil;

namespace Commander.Fody
{
    public class ModuleWeaverSettings
    {
        public const string DefaultOnCommandAttributeName = "OnCommandAttribute";
        public const string DefaultOnCommandCanExecuteAttributeName = "OnCommandCanExecuteAttribute";

        private readonly XElement _config;
        private bool _fallbackToNestedCommands = true;

        public ModuleWeaverSettings()
        {
            OnCommandAttributeName = DefaultOnCommandAttributeName;
            OnCommandCanExecuteAttributeName = DefaultOnCommandCanExecuteAttributeName;
            CommandImplementationTypes = new List<string>
            {
                "DelegateCommand",
                "RelayCommand",
            };
        }

        public ModuleWeaverSettings(XElement config):this()
        {
            _config = config;
            if (_config != null)
            {
                ApplyConfiguration();
            }
        }

        public XElement Config
        {
            get { return _config; }
        }

        public string OnCommandAttributeName { get; set; }
        public string OnCommandCanExecuteAttributeName { get; set; }
        public bool MatchAttributesByFullName { get; set; }
        public bool FallbackToNestedCommands
        {
            get { return _fallbackToNestedCommands; }
            set { _fallbackToNestedCommands = value; }
        }

        public IList<string> CommandImplementationTypes { get; set; }

        public virtual IEnumerable<TypeDefinition> GetAllTypes(ModuleWeaver moduleWeaver)
        {
            return moduleWeaver.ModuleDefinition.GetTypes();
        }        

        public virtual IEnumerable<TypeDefinition> GetAllClasses(ModuleWeaver moduleWeaver)
        {
            return GetAllTypes(moduleWeaver).Where(x => x.IsClass);
        }

        public virtual IEnumerable<TypeDefinition> GetTypesToProcess(ModuleWeaver moduleWeaver)
        {
            return GetAllClasses(moduleWeaver);
        }

        internal void ApplyConfiguration()
        {
            var onCommandAttributeName = (string) Config.Attribute("OnCommandAttributeName");
            if (!String.IsNullOrWhiteSpace(onCommandAttributeName))
            {
                OnCommandAttributeName = onCommandAttributeName;
            }

            var onCommandCanExecuteAttributeName = (string)Config.Attribute("OnCommandCanExecuteAttributeName");
            if (!String.IsNullOrWhiteSpace(onCommandCanExecuteAttributeName))
            {
                OnCommandCanExecuteAttributeName = onCommandCanExecuteAttributeName;
            }

            var matchAttributesByFullName = (bool?)Config.Attribute("MatchAttributesByFullName");
            if (matchAttributesByFullName != null)
            {
                MatchAttributesByFullName = matchAttributesByFullName.Value;
            }

            var fallbackToNestedCommands = (bool?)Config.Attribute("FallbackToNestedCommands");
            if (fallbackToNestedCommands != null)
            {
                FallbackToNestedCommands = fallbackToNestedCommands.Value;
            }            
        }
    }
}