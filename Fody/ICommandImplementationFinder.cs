using System.Collections.Generic;
using System.Text;

namespace Commander.Fody
{
    public interface ICommandImplementationFinder
    {
        bool TryFindCommandImplementation(out CommandInjectionAdviceBase injectionAdvice);
    }
}
