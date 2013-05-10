using System;
using Mono.Cecil.Cil;

namespace Commander.Fody
{
    public interface IFodyLogger
    {
        Action<string> LogInfo { get; set; }
        Action<string> LogWarning { get; set; }
        Action<string, SequencePoint> LogWarningPoint { get; set; }
        Action<string> LogError { get; set; }
        Action<string, SequencePoint> LogErrorPoint { get; set; }
    }
}