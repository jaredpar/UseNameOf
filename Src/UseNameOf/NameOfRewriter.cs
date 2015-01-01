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
        private readonly ITypeSymbol _argumentExceptionType;
        private readonly Dictionary<LiteralExpressionSyntax, NameOfExpressionSyntax> _map = new Dictionary<LiteralExpressionSyntax, NameOfExpressionSyntax>();

        internal NameOfRewriter(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
            _argumentExceptionType = semanticModel.Compilation.GetTypeByMetadataName("System.ArgumentException");
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

        /// <summary>
        /// Rewrite any creation of an <see cref="ArgumentException" /> which uses a string literal
        /// that maps to a parameter name to use nameof for that parameter instead. 
        /// </summary>
        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            if (!IsArgumentExceptionType(node.Type))
            {
                return base.VisitObjectCreationExpression(node);
            }

            var arguments = node.ArgumentList.Arguments;
            if (arguments.Count == 0)
            {
                return base.VisitObjectCreationExpression(node);
            }

            var expr = arguments[0].Expression;
            if (expr == null || expr.CSharpKind() != SyntaxKind.StringLiteralExpression)
            {
                return base.VisitObjectCreationExpression(node);
            }

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

            return base.VisitObjectCreationExpression(node);
        }

        private bool IsArgumentExceptionType(TypeSyntax typeSyntax)
        {
            var conversion = _semanticModel.ClassifyConversion(typeSyntax, _argumentExceptionType);
            return conversion.IsReference && conversion.IsImplicit;
        }
    }
}
