using Microsoft.CodeAnalysis.CSharp;
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
            var text = File.ReadAllText(args[0]);
            var tree = CSharpSyntaxTree.ParseText(text);
            var node = (new NameOfRewriter()).Visit(tree.GetRoot());
            Console.WriteLine(node.ToFullString());
        }
    }
}
