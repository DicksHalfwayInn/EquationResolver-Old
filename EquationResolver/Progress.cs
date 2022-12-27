using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquationResolver
{
    public enum Progress
    {
        None,
        SolveButtonHasBeenPressed,
        EquationHasBeenSolvedWithoutErrors,
        EquationNotSolvedBecauseOfErrorsInTheTextbox

    }
}
