using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace MagnumOpus.SourceGeneration
{
    /// <summary>
    /// Utility methods for code generation and string manipulation.
    /// These helpers abstract common operations needed during source generation.
    /// </summary>
    public static class CodeGenHelpers
    {
        /// <summary>
        /// Converts a field name to a backing field name with underscore prefix.
        /// Examples: "Position" → "_position", "Health" → "_health"
        /// </summary>
        /// <param name="fieldName">Original field name</param>
        /// <returns>Backing field name with underscore prefix and camelCase</returns>
        public static string ToBackingFieldName(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                return "_field";

            // Convert first character to lowercase and add underscore prefix
            return "_" + char.ToLowerInvariant(fieldName[0]) + fieldName.Substring(1);
        }

        /// <summary>
        /// Converts a field name to a property name (PascalCase).
        /// Examples: "position" → "Position", "maxHealth" → "MaxHealth"
        /// </summary>
        /// <param name="fieldName">Original field name</param>
        /// <returns>Property name in PascalCase</returns>
        public static string ToPropertyName(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                return "Property";

            // Convert first character to uppercase
            return char.ToUpperInvariant(fieldName[0]) + fieldName.Substring(1);
        }

        /// <summary>
        /// Gets the fully qualified type name for a given type symbol.
        /// Examples: "System.Numerics.Vector2", "MagnumOpus.Enums.Direction"
        /// </summary>
        /// <param name="typeSymbol">Type symbol from Roslyn analysis</param>
        /// <returns>Fully qualified type name</returns>
        public static string GetFullTypeName(ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                return "object";

            return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        /// <summary>
        /// Gets a simplified type name for use in generated code.
        /// Handles common types and removes unnecessary namespace qualifiers.
        /// </summary>
        /// <param name="typeSymbol">Type symbol from Roslyn analysis</param>
        /// <returns>Simplified type name suitable for code generation</returns>
        public static string GetSimpleTypeName(ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                return "object";

            var fullName = typeSymbol.ToDisplayString();

            // Handle common types with shorter names
            var commonTypes = new Dictionary<string, string>
            {
                { "System.Int32", "int" },
                { "System.Int64", "long" },
                { "System.Single", "float" },
                { "System.Double", "double" },
                { "System.Boolean", "bool" },
                { "System.String", "string" },
                { "System.Byte", "byte" },
                { "System.UInt32", "uint" },
                { "System.UInt64", "ulong" }
            };

            if (commonTypes.TryGetValue(fullName, out var simpleName))
                return simpleName;

            // For other types, use the simple name if no conflicts
            return typeSymbol.Name;
        }

        /// <summary>
        /// Determines the appropriate EqualityComparer method for a given type.
        /// Returns the most efficient comparison method for the type.
        /// </summary>
        /// <param name="typeSymbol">Type symbol to get comparer for</param>
        /// <returns>Code string for equality comparison</returns>
        public static string GetEqualityComparison(ITypeSymbol typeSymbol, string oldValue, string newValue)
        {
            if (typeSymbol == null)
                return $"object.Equals({oldValue}, {newValue})";

            var typeName = typeSymbol.ToDisplayString();

            // Value types with built-in equality
            if (typeSymbol.IsValueType)
            {
                // Special handling for floating point types (might want tolerance comparison)
                if (typeName == "System.Single" || typeName == "float")
                {
                    return $"Math.Abs({oldValue} - {newValue}) < float.Epsilon";
                }
                if (typeName == "System.Double" || typeName == "double")
                {
                    return $"Math.Abs({oldValue} - {newValue}) < double.Epsilon";
                }

                // For other value types, use direct comparison
                return $"{oldValue} != {newValue}";
            }

            // For reference types and complex value types, use EqualityComparer
            var simpleTypeName = GetSimpleTypeName(typeSymbol);
            return $"!EqualityComparer<{simpleTypeName}>.Default.Equals({oldValue}, {newValue})";
        }

        /// <summary>
        /// Generates proper indentation for code blocks.
        /// </summary>
        /// <param name="level">Indentation level (0 = no indent, 1 = 4 spaces, etc.)</param>
        /// <returns>String containing appropriate whitespace</returns>
        public static string Indent(int level)
        {
            return new string(' ', level * 4);
        }

        /// <summary>
        /// Sanitizes an identifier name to ensure it's valid C#.
        /// Removes invalid characters and ensures it doesn't conflict with keywords.
        /// </summary>
        /// <param name="name">Original identifier name</param>
        /// <returns>Valid C# identifier</returns>
        public static string SanitizeIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "identifier";

            // Remove invalid characters
            var sb = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(c);
                }
            }

            var result = sb.ToString();
            
            // Ensure it starts with letter or underscore
            if (result.Length == 0 || (!char.IsLetter(result[0]) && result[0] != '_'))
            {
                result = "_" + result;
            }

            // Avoid C# keywords
            var keywords = new HashSet<string>
            {
                "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char",
                "checked", "class", "const", "continue", "decimal", "default", "delegate",
                "do", "double", "else", "enum", "event", "explicit", "extern", "false",
                "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit",
                "in", "int", "interface", "internal", "is", "lock", "long", "namespace",
                "new", "null", "object", "operator", "out", "override", "params", "private",
                "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
                "short", "sizeof", "stackalloc", "static", "string", "struct", "switch",
                "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
                "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
            };

            if (keywords.Contains(result))
            {
                result = "@" + result;
            }

            return result;
        }

        /// <summary>
        /// Generates a unique identifier by appending numbers if conflicts exist.
        /// </summary>
        /// <param name="baseName">Base name for identifier</param>
        /// <param name="existingNames">Set of existing names to avoid conflicts</param>
        /// <returns>Unique identifier name</returns>
        public static string GetUniqueIdentifier(string baseName, HashSet<string> existingNames)
        {
            var sanitized = SanitizeIdentifier(baseName);
            
            if (!existingNames.Contains(sanitized))
                return sanitized;

            int counter = 1;
            string candidate;
            do
            {
                candidate = $"{sanitized}{counter}";
                counter++;
            }
            while (existingNames.Contains(candidate));

            return candidate;
        }

        /// <summary>
        /// Formats a code block with proper indentation and line breaks.
        /// </summary>
        /// <param name="code">Raw code string</param>
        /// <param name="baseIndentLevel">Base indentation level</param>
        /// <returns>Properly formatted code</returns>
        public static string FormatCodeBlock(string code, int baseIndentLevel = 0)
        {
            if (string.IsNullOrEmpty(code))
                return string.Empty;

            var lines = code.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    sb.AppendLine(Indent(baseIndentLevel) + trimmed);
                }
                else
                {
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates a comment block for generated code.
        /// </summary>
        /// <param name="description">Description of what the code does</param>
        /// <param name="indentLevel">Indentation level for the comment</param>
        /// <returns>Formatted comment block</returns>
        public static string GenerateComment(string description, int indentLevel = 0)
        {
            var indent = Indent(indentLevel);
            var lines = description.Split('\n');
            var sb = new StringBuilder();

            if (lines.Length == 1)
            {
                sb.AppendLine($"{indent}// {description}");
            }
            else
            {
                sb.AppendLine($"{indent}/*");
                foreach (var line in lines)
                {
                    sb.AppendLine($"{indent} * {line.Trim()}");
                }
                sb.AppendLine($"{indent} */");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Checks if a type name represents a primitive type that supports direct equality comparison.
        /// </summary>
        /// <param name="typeName">Full type name</param>
        /// <returns>True if type supports direct equality comparison</returns>
        public static bool IsPrimitiveType(string typeName)
        {
            var primitiveTypes = new HashSet<string>
            {
                "System.Boolean", "bool",
                "System.Byte", "byte",
                "System.SByte", "sbyte", 
                "System.Char", "char",
                "System.Int16", "short",
                "System.UInt16", "ushort",
                "System.Int32", "int",
                "System.UInt32", "uint",
                "System.Int64", "long",
                "System.UInt64", "ulong",
                "System.Single", "float",
                "System.Double", "double",
                "System.Decimal", "decimal"
            };

            return primitiveTypes.Contains(typeName);
        }
    }
}