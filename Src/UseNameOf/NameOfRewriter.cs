using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UseNameOf
{
    internal sealed class NameOfRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _semanticModel;
        private readonly Dictionary<LiteralExpressionSyntax, NameOfExpressionSyntax> _map = new Dictionary<LiteralExpressionSyntax, NameOfExpressionSyntax>();

        internal NameOfRewriter(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            NameOfExpressionSyntax nameOfExpr;
            if (_map.TryGetValue(node, out nameOfExpr))
            {
                return nameOfExpr;
            }

            return base.VisitLiteralExpression(node);
        }

        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var arguments = node.ArgumentList.Arguments;
            if (arguments.Count > 0)
            {
                var expr = arguments[0].Expression;
                if (expr != null && expr.CSharpKind() == SyntaxKind.StringLiteralExpression)
                {
                    var stringExpr = (LiteralExpressionSyntax)expr;
                    var position = stringExpr.GetLocation().SourceSpan.Start;
                    var identExpr = SyntaxFactory.IdentifierName(stringExpr.Token.ValueText);
                    var symbolInfo = _semanticModel.GetSpeculativeSymbolInfo(position, identExpr, SpeculativeBindingOption.BindAsExpression);
                    if (symbolInfo.Symbol != null && symbolInfo.Symbol.Kind == SymbolKind.Parameter)
                    {
                        _map[stringExpr] = SyntaxFactory.NameOfExpression(
                            SyntaxFactory.IdentifierName("nameof"),
                            identExpr);
                    }
                }
            }

            return base.VisitObjectCreationExpression(node);
        }
    }
}
