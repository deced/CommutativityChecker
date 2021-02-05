using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutativityChecker
{
    class MathOperation
    {
        public SyntaxKind Expression { get; set; }
        public List<string> Variables;
        public bool CanBeCommutative(params string[] args)
        {
            if (Expression == SyntaxKind.DivideExpression || Expression == SyntaxKind.SubtractExpression)
                if (args.Intersect(Variables).Count() > 1)
                    return false;
            return true;
        }
        public MathOperation(SyntaxKind expression = SyntaxKind.None)
        {
            Variables = new List<string>();
            Expression = expression;
        }
    }
}
