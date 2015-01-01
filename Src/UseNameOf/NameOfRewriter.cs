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
    public sealed class NameOfRewriter : CSharpSyntaxRewriter
    {
        private readonly Dictionary<LiteralExpressionSyntax, NameOfExpressionSyntax> _map = new Dictionary<LiteralExpressionSyntax, NameOfExpressionSyntax>();

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
                    _map[stringExpr] = SyntaxFactory.NameOfExpression(
                        SyntaxFactory.IdentifierName("nameof"),
                        SyntaxFactory.IdentifierName(stringExpr.Token.ValueText));
                }
            }

            return base.VisitObjectCreationExpression(node);
        }
    }
}
