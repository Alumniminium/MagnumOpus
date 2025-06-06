using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MagnumOpus.SourceGeneration
{
    /// <summary>
    /// Analyzes source code to find components marked for automatic ChangedTick generation.
    /// This class handles the Roslyn syntax tree analysis and extracts the information
    /// needed to generate property wrappers with change tracking.
    /// </summary>
    public static class ComponentAnalyzer
    {
        /// <summary>
        /// Represents a component that should have ChangedTick properties generated.
        /// Contains all the information needed for code generation.
        /// </summary>
        public class ComponentInfo
        {
            /// <summary>
            /// The syntax node for the component struct/class declaration.
            /// </summary>
            public TypeDeclarationSyntax Declaration { get; set; } = null!;

            /// <summary>
            /// Semantic model for type resolution and symbol information.
            /// </summary>
            public SemanticModel SemanticModel { get; set; } = null!;

            /// <summary>
            /// Full namespace of the component (e.g., "MagnumOpus.Components").
            /// </summary>
            public string Namespace { get; set; } = string.Empty;

            /// <summary>
            /// Name of the component class/struct (e.g., "PositionComponent").
            /// </summary>
            public string TypeName { get; set; } = string.Empty;

            /// <summary>
            /// Whether this is a struct (true) or class (false).
            /// </summary>
            public bool IsStruct { get; set; }

            /// <summary>
            /// Settings from the [AutoChangedTick] attribute.
            /// </summary>
            public AutoChangedTickSettings Settings { get; set; } = new();

            /// <summary>
            /// List of fields that should be converted to properties with change tracking.
            /// </summary>
            public List<TrackedFieldInfo> TrackedFields { get; set; } = new();

            /// <summary>
            /// Whether the component already has a ChangedTick field.
            /// </summary>
            public bool HasChangedTickField { get; set; }
        }

        /// <summary>
        /// Settings extracted from the [AutoChangedTick] attribute.
        /// </summary>
        public class AutoChangedTickSettings
        {
            public string TickSource { get; set; } = "NttWorld.Tick";
            public bool UseEqualityCheck { get; set; } = true;
            public bool AggressiveInlining { get; set; } = true;
        }

        /// <summary>
        /// Information about a field that should be converted to a tracked property.
        /// </summary>
        public class TrackedFieldInfo
        {
            /// <summary>
            /// The syntax node for the field declaration.
            /// </summary>
            public FieldDeclarationSyntax FieldSyntax { get; set; } = null!;

            /// <summary>
            /// The variable declarator (contains name and initializer).
            /// </summary>
            public VariableDeclaratorSyntax Variable { get; set; } = null!;

            /// <summary>
            /// The field's type symbol for type analysis.
            /// </summary>
            public ITypeSymbol FieldType { get; set; } = null!;

            /// <summary>
            /// Original field name (e.g., "Position").
            /// </summary>
            public string FieldName { get; set; } = string.Empty;

            /// <summary>
            /// Generated property name (usually same as field name).
            /// </summary>
            public string PropertyName { get; set; } = string.Empty;

            /// <summary>
            /// Generated backing field name (e.g., "_position").
            /// </summary>
            public string BackingFieldName { get; set; } = string.Empty;

            /// <summary>
            /// Type name for code generation.
            /// </summary>
            public string TypeName { get; set; } = string.Empty;

            /// <summary>
            /// Settings from the [Track] attribute.
            /// </summary>
            public TrackSettings Settings { get; set; } = new();

            /// <summary>
            /// Settings from the [NetworkSync] attribute if present.
            /// </summary>
            public NetworkSyncSettings? NetworkSyncSettings { get; set; }

            /// <summary>
            /// Initial value expression if the field has an initializer.
            /// </summary>
            public string InitialValue { get; set; }

            /// <summary>
            /// Whether this field has network synchronization enabled.
            /// </summary>
            public bool IsNetworkSynced => NetworkSyncSettings != null;
        }

        /// <summary>
        /// Settings extracted from the [Track] attribute.
        /// </summary>
        public class TrackSettings
        {
            public string CustomEqualityMethod { get; set; }
            public bool AlwaysUpdate { get; set; } = false;
            public string BackingFieldName { get; set; }
        }

        /// <summary>
        /// Settings extracted from the [NetworkSync] attribute.
        /// </summary>
        public class NetworkSyncSettings
        {
            public string MsgType { get; set; } = string.Empty;
            public bool UseEqualityCheck { get; set; } = true;
            public bool Broadcast { get; set; } = true;
            public string? PropertyName { get; set; }
        }

        /// <summary>
        /// Finds all components in the compilation that have the [AutoChangedTick] attribute.
        /// </summary>
        /// <param name="compilation">The compilation to analyze</param>
        /// <param name="cancellationToken">Cancellation token for long-running operations</param>
        /// <returns>List of components that need code generation</returns>
        public static List<ComponentInfo> FindMarkedComponents(Compilation compilation, System.Threading.CancellationToken cancellationToken = default)
        {
            var components = new List<ComponentInfo>();

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var root = syntaxTree.GetRoot(cancellationToken);

                // Find all struct and class declarations
                var typeDeclarations = root.DescendantNodes()
                    .OfType<TypeDeclarationSyntax>()
                    .Where(t => t is StructDeclarationSyntax || t is ClassDeclarationSyntax);

                foreach (var typeDeclaration in typeDeclarations)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Check if type has [AutoChangedTick] attribute
                    if (HasAutoChangedTickAttribute(typeDeclaration))
                    {
                        var componentInfo = AnalyzeComponent(typeDeclaration, semanticModel);
                        if (componentInfo != null)
                        {
                            components.Add(componentInfo);
                        }
                    }
                }
            }

            return components;
        }

        /// <summary>
        /// Checks if a type declaration has the [AutoChangedTick], [Component] attribute, or any fields with [NetworkSync].
        /// </summary>
        private static bool HasAutoChangedTickAttribute(TypeDeclarationSyntax typeDeclaration)
        {
            // Check for type-level attributes
            var hasTypeAttribute = typeDeclaration.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr => IsAutoChangedTickAttribute(attr) || IsComponentAttribute(attr));

            if (hasTypeAttribute)
                return true;

            // Check for fields with NetworkSync attribute
            var hasNetworkSyncFields = typeDeclaration.Members
                .OfType<FieldDeclarationSyntax>()
                .Any(field => field.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Any(attr => IsNetworkSyncAttribute(attr)));

            return hasNetworkSyncFields;
        }

        /// <summary>
        /// Determines if an attribute syntax represents the [AutoChangedTick] attribute.
        /// </summary>
        private static bool IsAutoChangedTickAttribute(AttributeSyntax attribute)
        {
            var name = attribute.Name.ToString();
            return name == "AutoChangedTick" || 
                   name == "AutoChangedTickAttribute" ||
                   name.EndsWith("AutoChangedTick") ||
                   name.EndsWith("AutoChangedTickAttribute");
        }

        /// <summary>
        /// Determines if an attribute syntax represents the [Component] attribute.
        /// </summary>
        private static bool IsComponentAttribute(AttributeSyntax attribute)
        {
            var name = attribute.Name.ToString();
            return name == "Component" || 
                   name == "ComponentAttribute" ||
                   name.EndsWith("Component") ||
                   name.EndsWith("ComponentAttribute");
        }

        /// <summary>
        /// Analyzes a component type to extract all information needed for code generation.
        /// </summary>
        private static ComponentInfo AnalyzeComponent(TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel)
        {
            // Verify this is a partial type (required for source generation)
            if (!typeDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            {
                // TODO: Report diagnostic that partial keyword is required
                return null;
            }

            var componentInfo = new ComponentInfo
            {
                Declaration = typeDeclaration,
                SemanticModel = semanticModel,
                TypeName = typeDeclaration.Identifier.ValueText,
                IsStruct = typeDeclaration is StructDeclarationSyntax
            };

            // Extract namespace
            componentInfo.Namespace = GetNamespace(typeDeclaration);

            // Parse [AutoChangedTick] attribute settings
            componentInfo.Settings = ParseAutoChangedTickAttribute(typeDeclaration);

            // Check for existing ChangedTick field
            componentInfo.HasChangedTickField = HasChangedTickField(typeDeclaration);

            // Find all fields marked with [Track] or auto-track all fields if using [Component]
            bool hasComponentAttribute = typeDeclaration.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr => IsComponentAttribute(attr));
            
            componentInfo.TrackedFields = FindTrackedFields(typeDeclaration, semanticModel, hasComponentAttribute);

            // Only generate code if there are tracked fields
            if (componentInfo.TrackedFields.Count == 0)
            {
                return null;
            }

            return componentInfo;
        }

        /// <summary>
        /// Extracts the namespace from a type declaration.
        /// </summary>
        private static string GetNamespace(TypeDeclarationSyntax typeDeclaration)
        {
            var namespaceDeclaration = typeDeclaration.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            if (namespaceDeclaration != null)
            {
                return namespaceDeclaration.Name.ToString();
            }

            // Check for file-scoped namespace
            var fileScopedNamespace = typeDeclaration.Ancestors().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
            if (fileScopedNamespace != null)
            {
                return fileScopedNamespace.Name.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Parses settings from the [AutoChangedTick] attribute.
        /// </summary>
        private static AutoChangedTickSettings ParseAutoChangedTickAttribute(TypeDeclarationSyntax typeDeclaration)
        {
            var settings = new AutoChangedTickSettings();

            var attribute = typeDeclaration.AttributeLists
                .SelectMany(al => al.Attributes)
                .FirstOrDefault(attr => IsAutoChangedTickAttribute(attr));

            if (attribute?.ArgumentList != null)
            {
                foreach (var argument in attribute.ArgumentList.Arguments)
                {
                    var nameEquals = argument.NameEquals;
                    if (nameEquals != null)
                    {
                        var propertyName = nameEquals.Name.Identifier.ValueText;
                        var value = argument.Expression;

                        switch (propertyName)
                        {
                            case "TickSource":
                                if (value is LiteralExpressionSyntax literal)
                                {
                                    settings.TickSource = literal.Token.ValueText;
                                }
                                break;
                            case "UseEqualityCheck":
                                if (value is LiteralExpressionSyntax boolLiteral && 
                                    bool.TryParse(boolLiteral.Token.ValueText, out var useEquality))
                                {
                                    settings.UseEqualityCheck = useEquality;
                                }
                                break;
                            case "AggressiveInlining":
                                if (value is LiteralExpressionSyntax inlineLiteral && 
                                    bool.TryParse(inlineLiteral.Token.ValueText, out var aggressive))
                                {
                                    settings.AggressiveInlining = aggressive;
                                }
                                break;
                        }
                    }
                }
            }

            return settings;
        }

        /// <summary>
        /// Checks if the component already has a ChangedTick field.
        /// </summary>
        private static bool HasChangedTickField(TypeDeclarationSyntax typeDeclaration)
        {
            return typeDeclaration.Members
                .OfType<FieldDeclarationSyntax>()
                .Any(field => field.Declaration.Variables
                    .Any(variable => variable.Identifier.ValueText == "ChangedTick"));
        }

        /// <summary>
        /// Finds all fields to be tracked within the component.
        /// If autoTrackAll is true, tracks all fields except ChangedTick.
        /// Otherwise, only tracks fields marked with [Track] attribute.
        /// </summary>
        private static List<TrackedFieldInfo> FindTrackedFields(TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel, bool autoTrackAll = false)
        {
            var trackedFields = new List<TrackedFieldInfo>();

            foreach (var member in typeDeclaration.Members.OfType<FieldDeclarationSyntax>())
            {
                // Check if field has [Track] or [NetworkSync] attribute
                var hasTrackAttribute = member.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Any(attr => IsTrackAttribute(attr));

                var hasNetworkSyncAttribute = member.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Any(attr => IsNetworkSyncAttribute(attr));

                // When auto-tracking, track private fields OR fields with NetworkSync attribute  
                // When not auto-tracking, only track fields with [Track] or [NetworkSync] attribute
                // For NetworkSync, we also track public fields since they need to be converted to properties
                bool shouldTrack = autoTrackAll ? 
                    (member.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)) || hasNetworkSyncAttribute) :
                    (hasTrackAttribute || hasNetworkSyncAttribute);
                    
                if (!shouldTrack)
                    continue;

                foreach (var variable in member.Declaration.Variables)
                {
                    var fieldSymbol = semanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
                    if (fieldSymbol == null)
                        continue;

                    var fieldName = variable.Identifier.ValueText;
                    
                    // Skip ChangedTick field when auto-tracking
                    if (autoTrackAll && fieldName == "ChangedTick")
                        continue;
                        
                    // Skip fields that already have corresponding properties when auto-tracking
                    if (autoTrackAll && HasCorrespondingProperty(typeDeclaration, fieldName))
                        continue;
                        
                    // Check if this is a public field
                    var isPublicField = member.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword));
                    
                    // Skip public fields with NetworkSync - they should be used as-is
                    // TODO: Implement field assignment interception for these
                    if (hasNetworkSyncAttribute && isPublicField)
                        continue;
                    
                    string propertyName;
                    string backingFieldName;
                    
                    if (fieldName.StartsWith("_"))
                    {
                        // Private backing field: generate property name
                        propertyName = CodeGenHelpers.ToPropertyName(fieldName.Substring(1));
                        backingFieldName = fieldName;
                    }
                    else
                    {
                        // Regular field: generate property name and backing field
                        propertyName = CodeGenHelpers.ToPropertyName(fieldName);
                        backingFieldName = CodeGenHelpers.ToBackingFieldName(fieldName);
                    }
                    
                    var fieldInfo = new TrackedFieldInfo
                    {
                        FieldSyntax = member,
                        Variable = variable,
                        FieldType = fieldSymbol.Type,
                        FieldName = fieldName,
                        PropertyName = propertyName,
                        BackingFieldName = backingFieldName,
                        TypeName = CodeGenHelpers.GetSimpleTypeName(fieldSymbol.Type),
                        Settings = ParseTrackAttribute(member),
                        NetworkSyncSettings = hasNetworkSyncAttribute ? ParseNetworkSyncAttribute(member) : null
                    };

                    // Extract initial value if present
                    if (variable.Initializer != null)
                    {
                        fieldInfo.InitialValue = variable.Initializer.Value.ToString();
                    }

                    trackedFields.Add(fieldInfo);
                }
            }

            return trackedFields;
        }

        /// <summary>
        /// Determines if an attribute syntax represents the [Track] attribute.
        /// </summary>
        private static bool IsTrackAttribute(AttributeSyntax attribute)
        {
            var name = attribute.Name.ToString();
            return name == "Track" || 
                   name == "TrackAttribute" ||
                   name.EndsWith("Track") ||
                   name.EndsWith("TrackAttribute");
        }

        /// <summary>
        /// Determines if an attribute syntax represents the [NetworkSync] attribute.
        /// </summary>
        private static bool IsNetworkSyncAttribute(AttributeSyntax attribute)
        {
            var name = attribute.Name.ToString();
            return name == "NetworkSync" || 
                   name == "NetworkSyncAttribute" ||
                   name.EndsWith("NetworkSync") ||
                   name.EndsWith("NetworkSyncAttribute");
        }

        /// <summary>
        /// Checks if a field already has a corresponding property with the same name (case-insensitive).
        /// </summary>
        private static bool HasCorrespondingProperty(TypeDeclarationSyntax typeDeclaration, string fieldName)
        {
            // Generate the expected property name for this field
            var expectedPropertyName = fieldName.StartsWith("_") 
                ? CodeGenHelpers.ToPropertyName(fieldName.Substring(1))
                : CodeGenHelpers.ToPropertyName(fieldName);
                
            // Check if a property with that name already exists
            return typeDeclaration.Members
                .OfType<PropertyDeclarationSyntax>()
                .Any(prop => prop.Identifier.ValueText.Equals(expectedPropertyName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Parses settings from the [Track] attribute.
        /// </summary>
        private static TrackSettings ParseTrackAttribute(FieldDeclarationSyntax fieldDeclaration)
        {
            var settings = new TrackSettings();

            var attribute = fieldDeclaration.AttributeLists
                .SelectMany(al => al.Attributes)
                .FirstOrDefault(attr => IsTrackAttribute(attr));

            if (attribute?.ArgumentList != null)
            {
                foreach (var argument in attribute.ArgumentList.Arguments)
                {
                    var nameEquals = argument.NameEquals;
                    if (nameEquals != null)
                    {
                        var propertyName = nameEquals.Name.Identifier.ValueText;
                        var value = argument.Expression;

                        switch (propertyName)
                        {
                            case "CustomEqualityMethod":
                                if (value is LiteralExpressionSyntax literal)
                                {
                                    settings.CustomEqualityMethod = literal.Token.ValueText;
                                }
                                break;
                            case "AlwaysUpdate":
                                if (value is LiteralExpressionSyntax boolLiteral && 
                                    bool.TryParse(boolLiteral.Token.ValueText, out var alwaysUpdate))
                                {
                                    settings.AlwaysUpdate = alwaysUpdate;
                                }
                                break;
                            case "BackingFieldName":
                                if (value is LiteralExpressionSyntax nameLiteral)
                                {
                                    settings.BackingFieldName = nameLiteral.Token.ValueText;
                                }
                                break;
                        }
                    }
                }
            }

            return settings;
        }

        /// <summary>
        /// Parses settings from the [NetworkSync] attribute.
        /// </summary>
        private static NetworkSyncSettings ParseNetworkSyncAttribute(FieldDeclarationSyntax fieldDeclaration)
        {
            var settings = new NetworkSyncSettings();

            var attribute = fieldDeclaration.AttributeLists
                .SelectMany(al => al.Attributes)
                .FirstOrDefault(attr => IsNetworkSyncAttribute(attr));

            if (attribute != null)
            {
                // Parse the first argument (MsgType) - this is required
                if (attribute.ArgumentList?.Arguments.Count > 0)
                {
                    var firstArgument = attribute.ArgumentList.Arguments[0];
                    if (firstArgument.NameEquals == null) // Positional argument
                    {
                        settings.MsgType = firstArgument.Expression.ToString();
                    }
                }

                // Parse named arguments
                if (attribute.ArgumentList != null)
                {
                    foreach (var argument in attribute.ArgumentList.Arguments)
                    {
                        var nameEquals = argument.NameEquals;
                        if (nameEquals != null)
                        {
                            var propertyName = nameEquals.Name.Identifier.ValueText;
                            var value = argument.Expression;

                            switch (propertyName)
                            {
                                case "UseEqualityCheck":
                                    if (value is LiteralExpressionSyntax boolLiteral && 
                                        bool.TryParse(boolLiteral.Token.ValueText, out var useEquality))
                                    {
                                        settings.UseEqualityCheck = useEquality;
                                    }
                                    break;
                                case "Broadcast":
                                    if (value is LiteralExpressionSyntax broadcastLiteral && 
                                        bool.TryParse(broadcastLiteral.Token.ValueText, out var broadcast))
                                    {
                                        settings.Broadcast = broadcast;
                                    }
                                    break;
                                case "PropertyName":
                                    if (value is LiteralExpressionSyntax nameLiteral)
                                    {
                                        settings.PropertyName = nameLiteral.Token.ValueText;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

            return settings;
        }
    }
}