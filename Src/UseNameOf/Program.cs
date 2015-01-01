using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UseNameOf
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Work(args).Wait();
        }

        private static async Task Work(string[] args)
        {
            var text = File.ReadAllText(args[0]);
            var workspace = new CustomWorkspace();
            var project = workspace.AddProject("NameOfRewrite", LanguageNames.CSharp)
                .WithMetadataReferences(new[] { MetadataReference.CreateFromAssembly(typeof(object).Assembly) });
            var document = project.AddDocument("Test", SourceText.From(text));
            project = document.Project;

            var compilation = await project.GetCompilationAsync();
            var argumentExceptionType = compilation.GetTypeByMetadataName("System.ArgumentException");
            var root = await document.GetSyntaxRootAsync();
            var rewriter = new NameOfRewriter(compilation.GetSemanticModel(root.SyntaxTree));
            var node = rewriter.Visit(root);
            Console.WriteLine(node.ToFullString());
        }
    }
}
