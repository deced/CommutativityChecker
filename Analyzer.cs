using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CommutativityChecker
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]

    public class Analyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor DiagnosticDescriptor =
            new DiagnosticDescriptor(
                "error",
                "Operation is not commutative",
                "Operation is not commutative",
                "Operation is not commutative",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(DiagnosticDescriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.MethodDeclaration);
        }
        private void Analyze(SyntaxNodeAnalysisContext context)
        {
            var node = (MethodDeclarationSyntax)context.Node;
            var tokensToCheck = node.DescendantNodes().OfType<StatementSyntax>()
                .Select(x => x.DescendantNodes()
                .OfType<BinaryExpressionSyntax>().FirstOrDefault());
            var attributeArg = node.SyntaxTree.GetRoot().DescendantNodes()
                .OfType<AttributeArgumentSyntax>()
                .Select(x => x.ToString().Replace("\"", "")).ToList();
            if (tokensToCheck != null)
                foreach (var token in tokensToCheck)
                {
                    var Tree = GetMathOperations(token);
                    if (!(Tree.All(x => x.CanBeCommutative(attributeArg.ToArray()) && IsCommutative(Tree, attributeArg.ToArray()))))
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptor, token.Parent.GetLocation()));
                }
        }
        static bool IsCommutative(List<MathOperation> operations, params string[] args)
        {
            List<string> placeCheck = new List<string> { }; // a*b + a : false, не хватает b во втором слагаемом
            foreach (var opeation in operations)
            {
                var countCheck = opeation.Variables.Where(x => args.Contains(x)).ToList(); // a*a*b  : false, не хватает множителя b
                int count = 0;
                if (countCheck.Count > 1)
                {
                    foreach (string op in countCheck)
                    {
                        if (count != 0 && countCheck.Count > 1 && count != countCheck.Count(x => x == op))
                            return false;
                        count = countCheck.Count(x => x == op);
                    }                   
                }
                placeCheck.AddRange(countCheck.Distinct());
            }
            if (placeCheck.Count == 1)
                return true;
            return operations.All(x => placeCheck.Except(x.Variables).Count() == 0); // проверка, везде ли были обязательные переменные из placeCheck
        }

        static SyntaxKind GetParentExpression(SyntaxNode syntaxNode)
        {
            while (true)
            {
                if (syntaxNode is BinaryExpressionSyntax binaryExpression)
                {
                    return binaryExpression.Kind();
                }
                if (!(syntaxNode is ExpressionSyntax))
                {
                    return SyntaxKind.None;
                }
                syntaxNode = syntaxNode.Parent;
            }
        }
        static List<MathOperation> GetMathOperations(ExpressionSyntax expression)
        {
            List<MathOperation> ret = new List<MathOperation>();
            if (expression is BinaryExpressionSyntax binary)
            {
                if (binary.Left is IdentifierNameSyntax)
                {
                    if (ret.LastOrDefault()?.Expression != binary.Kind())
                        ret.Add(new MathOperation(binary.Kind()));
                    ret.Last().Variables.Add(binary.Left.ToString());
                }
                else
                {
                    ret.AddRange(GetMathOperations(binary.Left));
                }
                if (binary.Right is IdentifierNameSyntax)
                {
                    if (ret.LastOrDefault()?.Expression != binary.Kind())
                        ret.Add(new MathOperation(binary.Kind()));
                    ret.Last().Variables.Add(binary.Right.ToString());
                }
                else
                {
                    ret.AddRange(GetMathOperations(binary.Right));
                }
            }
            else if (expression is ParenthesizedExpressionSyntax parenthesized)
            {
                ret.AddRange(GetMathOperations(parenthesized.Expression));
            }
            else if (expression is IdentifierNameSyntax)
            {
                ret.Add(new MathOperation(GetParentExpression(expression.Parent)));
                ret.Last().Variables.Add(expression.ToString());
            }
            return ret;
        }

    }
}
