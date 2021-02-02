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
            context.RegisterSyntaxNodeAction(
                Analyze,
                SyntaxKind.MethodDeclaration);

        }

        private void Analyze(SyntaxNodeAnalysisContext context)
        {
            var node = (MethodDeclarationSyntax)context.Node;
            var tokensToCheck = node.Body.SyntaxTree.GetRoot().DescendantTokens().OfType<SyntaxToken>().Where(x => x.Kind() == SyntaxKind.MinusToken || x.Kind() == SyntaxKind.SlashToken).ToList();
            var attributeArg = node.SyntaxTree.GetRoot().DescendantNodes().OfType<AttributeArgumentSyntax>().ToList();
            foreach (var token in tokensToCheck)
            {
                foreach (var arg in attributeArg)
                {
                        if (token.Parent.GetText().ToString().Contains(arg.GetText().ToString().Replace("\"", "")))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptor, token.Parent.GetLocation()));
                        }
                }
            }
        }

    }
}
