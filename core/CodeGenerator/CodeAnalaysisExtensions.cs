using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGen
{
    internal static class CodeAnalaysisExtensions
    {
        public static string GetTypeName(this InterfaceDeclarationSyntax node)
        {
            var parts = node.GetFullTypeName().Split('.');
            return parts[parts.Length - 1];
        }

        public static string GetFullTypeName(this InterfaceDeclarationSyntax node)
        {
            var ns = GetNamespaceScope(node.Parent);
            var fullName = (ns.Any() ? ns + "." : "") + node.Identifier;
            return fullName;
        }

        public static string GetNamespaceScope(this SyntaxNode node)
        {
            if (node == null)
                return "";
            var namespaceDecl = node as NamespaceDeclarationSyntax;
            var current = (namespaceDecl != null) ? namespaceDecl.Name.ToString() : "";
            var parent = GetNamespaceScope(node.Parent);
            if (parent.Any() && current.Any())
                return parent + "." + current;
            if (parent.Any())
                return parent;
            return current;
        }

        public static SyntaxNode GetRootNode(this SyntaxNode node)
        {
            if (node == null)
                return null;
            if (node.Parent == null)
                return node;
            else
                return GetRootNode(node.Parent);
        }

        public static PropertyDeclarationSyntax[] GetProperties(this InterfaceDeclarationSyntax node)
        {
            return node.Members.OfType<PropertyDeclarationSyntax>().ToArray();
        }

        public static AttributeSyntax GetAttribute(this SyntaxList<AttributeListSyntax> attributeLists, string name)
        {
            var canonicalName = name.EndsWith("Attribute") ? name : name + "Attribute";
            foreach (var attributeList in attributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attrName = attribute.Name.ToString();
                    var attrCanonicalName = attrName.EndsWith("Attribute") ? attrName : attrName + "Attribute";
                    if (CompareTypeName(attrCanonicalName, canonicalName))
                        return attribute;
                }
            }
            return null;
        }

        public static bool HasBase(this InterfaceDeclarationSyntax node, string name)
        {
            return node.BaseList.Types.Any(type => CompareTypeName(type.Type.ToString(), name));
        }

        public static bool CompareTypeName(string a, string b)
        {
            var ap = a.Split('.').Reverse();
            var bp = b.Split('.').Reverse();
            return ap.Zip(bp, (x, y) => x == y).All(x => x);
        }

        public static bool IsValueType(this TypeSyntax node)
        {
            return false;
        }
    }
}
