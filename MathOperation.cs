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
        public SyntaxKind SyntaxToken { get; set; }
        public List<string> Variables;
        public bool CanBeCommutative(params string[] args)
        {
            if (SyntaxToken == SyntaxKind.SlashToken || SyntaxToken == SyntaxKind.MinusToken)
                if (args.Intersect(Variables).Count() > 0)
                    return false;
            return true;
        }
        public MathOperation()
        {
            Variables = new List<string>();
        }
    }
}
