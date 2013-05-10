using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Commander.Fody
{
    public class Assets
    {                

        private readonly Lazy<List<TypeDefinition>> _allClasses;
        private readonly ModuleDefinition _moduleDefinition;
        private readonly IFodyLogger _log;

        private readonly TypeReference _stringTypeReference;
        private readonly TypeReference _voidTypeReference;
        private readonly TypeReference _objectTypeReference;
        private readonly TypeReference _booleanTypeReference;
        private readonly TypeReference _iCommandTypeReference;

        public Assets([NotNull] ModuleDefinition moduleDefinition, [NotNull] IFodyLogger log)
        {
            if (moduleDefinition == null)
            {
                throw new ArgumentNullException("moduleDefinition");
            }
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }
            _moduleDefinition = moduleDefinition;
            _log = log;
            _allClasses = new Lazy<List<TypeDefinition>>(()=> ModuleDefinition.GetTypes().Where(x => x.IsClass).ToList());

            _stringTypeReference = moduleDefinition.TypeSystem.String;
            _voidTypeReference = moduleDefinition.TypeSystem.Void;
            _objectTypeReference = moduleDefinition.TypeSystem.Object;
            _booleanTypeReference = moduleDefinition.TypeSystem.Boolean;

            var assemblyResolver = ModuleDefinition.AssemblyResolver;
            var presentationCoreDefinition = GetPresentationCoreDefinition();
            var presentationCoreTypes = presentationCoreDefinition.MainModule.Types;
            var systemDefinition = assemblyResolver.Resolve("System");
            var systemTypes = systemDefinition.MainModule.Types;
            var iCommandDefinition = presentationCoreTypes.FirstOrDefault(x => x.Name == "ICommand");
            if (iCommandDefinition == null)
            {
                iCommandDefinition = systemTypes.FirstOrDefault(x => x.Name == "ICommand");
            }
            _iCommandTypeReference = ModuleDefinition.Import(iCommandDefinition);
            if (_iCommandTypeReference == null)
            {
                const string message = "Could not find type System.Windows.Input.ICommand.";
                throw new WeavingException(message);
            }
        }
        
        public ModuleDefinition ModuleDefinition
        {
            get { return _moduleDefinition; }
        }

        public IFodyLogger Log
        {
            get { return _log; }
        }

        public List<TypeDefinition> AllClasses
        {
            get { return _allClasses.Value; }
        }

        public TypeReference StringTypeReference
        {
            get { return _stringTypeReference; }
        }

        public TypeReference VoidTypeReference
        {
            get { return _voidTypeReference; }
        }

        public TypeReference ObjectTypeReference
        {
            get { return _objectTypeReference; }
        }

        public TypeReference BooleanTypeReference
        {
            get { return _booleanTypeReference; }
        }

        public TypeReference ICommandTypeReference
        {
            get { return _iCommandTypeReference; }
        }

        AssemblyDefinition GetPresentationCoreDefinition()
        {
            try
            {
                return ModuleDefinition.AssemblyResolver.Resolve("PresentationCore");
            }
            catch (Exception exception)
            {
                var message = string.Format(@"Could not resolve PresentationCore. Please ensure you are using .net 3.5 or higher.{0}Inner message:{1}.", Environment.NewLine, exception.Message);
                message += " AssemblyResolver is: " + ModuleDefinition.AssemblyResolver.GetType().FullName;
                throw new WeavingException(message);
            }
        }
    }
}