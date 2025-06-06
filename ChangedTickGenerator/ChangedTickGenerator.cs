using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MagnumOpus.SourceGeneration
{
    /// <summary>
    /// Main source generator for automatic ChangedTick property generation.
    /// 
    /// This generator implements IIncrementalSourceGenerator for optimal performance:
    /// - Only regenerates code when source files actually change
    /// - Caches analysis results between builds
    /// - Supports partial compilation scenarios
    /// 
    /// The generator works in these steps:
    /// 1. Find all types marked with [AutoChangedTick] attribute
    /// 2. Analyze their fields marked with [Track] attribute  
    /// 3. Generate partial classes/structs with property wrappers
    /// 4. Properties automatically update ChangedTick when values change
    /// </summary>
    [Generator]
    public class ChangedTickGenerator : IIncrementalGenerator
    {
        /// <summary>
        /// Called once when the generator is first created.
        /// Sets up the incremental generation pipeline.
        /// </summary>
        /// <param name="context">Initialization context for setting up providers</param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register the attributes that this generator provides
            // This ensures they're available even if not explicitly referenced
            context.RegisterPostInitializationOutput(RegisterAttributes);

            // Create a provider that finds all types with [AutoChangedTick] attribute
            var componentProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: IsPotentialComponent,           // Fast syntax-only filter
                    transform: TransformComponent)             // Expensive semantic analysis
                .Where(component => component != null);       // Filter out null results

            // Combine all found components and generate code
            context.RegisterSourceOutput(
                componentProvider.Collect(),
                GenerateComponentCode);

            // Register diagnostics for invalid usage
            context.RegisterSourceOutput(
                componentProvider.Collect(),
                ReportDiagnostics);
        }

        /// <summary>
        /// Fast syntax-only check to identify potential components.
        /// This is called for every syntax node, so it must be very fast.
        /// </summary>
        /// <param name="syntaxNode">Syntax node to check</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if node might be a component with [AutoChangedTick]</returns>
        private static bool IsPotentialComponent(SyntaxNode syntaxNode, System.Threading.CancellationToken cancellationToken)
        {
            // Quick checks that don't require semantic analysis
            if (syntaxNode is not Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax typeDeclaration)
                return false;

            // Must be struct or class
            if (typeDeclaration is not (Microsoft.CodeAnalysis.CSharp.Syntax.StructDeclarationSyntax or Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax))
                return false;

            // Must have at least one attribute
            if (!typeDeclaration.AttributeLists.Any())
                return false;

            // Quick string check for potential [AutoChangedTick] or [Component] attribute
            // This is a fast filter - false positives are OK, false negatives are not
            var hasRelevantAttribute = typeDeclaration.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr => attr.Name.ToString().Contains("AutoChangedTick") || 
                           attr.Name.ToString().Contains("Component"));

            return hasRelevantAttribute;
        }

        /// <summary>
        /// Expensive semantic analysis to extract component information.
        /// Only called for nodes that passed the fast syntax filter.
        /// </summary>
        /// <param name="context">Generator syntax context with semantic model</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ComponentInfo if this is a valid component, null otherwise</returns>
        private static ComponentAnalyzer.ComponentInfo TransformComponent(
            GeneratorSyntaxContext context, 
            System.Threading.CancellationToken cancellationToken)
        {
            var typeDeclaration = (Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax)context.Node;

            try
            {
                // Use ComponentAnalyzer to extract detailed information
                var components = ComponentAnalyzer.FindMarkedComponents(
                    context.SemanticModel.Compilation, 
                    cancellationToken);

                // Find the component that matches this syntax node
                return components.FirstOrDefault(c => c.Declaration == typeDeclaration);
            }
            catch (OperationCanceledException)
            {
                // Cancellation is expected, just rethrow
                throw;
            }
            catch (Exception ex)
            {
                // For debugging: create a diagnostic about the error
                // In production, you might want to report this as a diagnostic
                System.Diagnostics.Debug.WriteLine($"Error analyzing component {typeDeclaration.Identifier}: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Generates source code for all found components.
        /// This is called once with all components found in the compilation.
        /// </summary>
        /// <param name="context">Source production context for adding generated files</param>
        /// <param name="components">All components that need code generation</param>
        private static void GenerateComponentCode(
            SourceProductionContext context, 
            System.Collections.Immutable.ImmutableArray<ComponentAnalyzer.ComponentInfo> components)
        {
            // Filter out null components and group by assembly/namespace for organization
            var validComponents = components
                .Where(c => c != null)
                .ToList();

            foreach (var component in validComponents)
            {
                try
                {
                    // Generate the partial class/struct code
                    var sourceCode = ComponentTemplate.GeneratePartialComponent(component);
                    
                    // Create a unique filename for this generated file
                    var fileName = $"{component.TypeName}.ChangedTick.g.cs";
                    
                    // Add the generated source to the compilation
                    context.AddSource(fileName, SourceText.From(sourceCode, Encoding.UTF8));
                }
                catch (Exception ex)
                {
                    // Report generation errors as diagnostics
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.GenerationError,
                        Location.None,
                        component.TypeName,
                        ex.Message);
                    
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // Generate a summary file with metadata about all generated components
            if (validComponents.Any())
            {
                var summaryCode = GenerateGenerationSummary(validComponents);
                context.AddSource("ChangedTick.GenerationSummary.g.cs", SourceText.From(summaryCode, Encoding.UTF8));
            }
        }

        /// <summary>
        /// Reports diagnostics for invalid component usage.
        /// This helps developers understand why their components aren't being processed.
        /// </summary>
        private static void ReportDiagnostics(
            SourceProductionContext context,
            System.Collections.Immutable.ImmutableArray<ComponentAnalyzer.ComponentInfo> components)
        {
            foreach (var component in components.Where(c => c != null))
            {
                // Check for common issues and report helpful diagnostics

                // Check if component is missing partial keyword
                if (!component!.Declaration.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)))
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.MissingPartialKeyword,
                        component.Declaration.Identifier.GetLocation(),
                        component.TypeName);
                    
                    context.ReportDiagnostic(diagnostic);
                }

                // Check if component is missing ChangedTick field
                if (!component.HasChangedTickField)
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.MissingChangedTickField,
                        component.Declaration.Identifier.GetLocation(),
                        component.TypeName);
                    
                    context.ReportDiagnostic(diagnostic);
                }

                // Check if component has no tracked fields
                if (component.TrackedFields.Count == 0)
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.NoTrackedFields,
                        component.Declaration.Identifier.GetLocation(),
                        component.TypeName);
                    
                    context.ReportDiagnostic(diagnostic);
                }

                // Report successful generation
                var successDiagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.GenerationSuccess,
                    component.Declaration.Identifier.GetLocation(),
                    component.TypeName,
                    component.TrackedFields.Count);
                
                context.ReportDiagnostic(successDiagnostic);
            }
        }

        /// <summary>
        /// Registers the attribute types that this generator uses.
        /// This makes them available in the compilation even if not explicitly referenced.
        /// </summary>
        private static void RegisterAttributes(IncrementalGeneratorPostInitializationContext context)
        {
            // Inline the attribute source code since File IO is not allowed in generators
            var attributeSource = @"using System;

namespace MagnumOpus.SourceGeneration
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class AutoChangedTickAttribute : Attribute
    {
        public string TickSource { get; set; } = ""NttWorld.Tick"";
        public bool UseEqualityCheck { get; set; } = true;
        public bool AggressiveInlining { get; set; } = true;
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class TrackAttribute : Attribute
    {
        public string CustomEqualityMethod { get; set; }
        public bool AlwaysUpdate { get; set; } = false;
        public string BackingFieldName { get; set; }
    }
}";

            context.AddSource("AutoChangedTickAttribute.g.cs", SourceText.From(attributeSource, Encoding.UTF8));
        }

        /// <summary>
        /// Generates a summary file containing metadata about all generated components.
        /// This is useful for debugging and understanding what was generated.
        /// </summary>
        private static string GenerateGenerationSummary(List<ComponentAnalyzer.ComponentInfo> components)
        {
            var sb = new StringBuilder();

            sb.AppendLine("//------------------------------------------------------------------------------");
            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("//     ChangedTick Generation Summary");
            sb.AppendLine($"//     Generated at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"//     Components processed: {components.Count}");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine("//------------------------------------------------------------------------------");
            sb.AppendLine();

            sb.AppendLine("namespace MagnumOpus.SourceGeneration.Generated");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Summary of all components processed by the ChangedTick generator.");
            sb.AppendLine("    /// This class is generated for debugging and introspection purposes.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static class GenerationSummary");
            sb.AppendLine("    {");
            sb.AppendLine($"        public static readonly System.DateTime GeneratedAt = new System.DateTime({DateTime.Now.Ticks});");
            sb.AppendLine($"        public static readonly int ComponentCount = {components.Count};");
            sb.AppendLine($"        public static readonly int TotalTrackedFields = {components.Sum(c => c.TrackedFields.Count)};");
            sb.AppendLine();

            sb.AppendLine("        public static readonly string[] GeneratedComponents = new[]");
            sb.AppendLine("        {");
            foreach (var component in components)
            {
                sb.AppendLine($"            \"{component.Namespace}.{component.TypeName} ({component.TrackedFields.Count} fields)\",");
            }
            sb.AppendLine("        };");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }

    /// <summary>
    /// Diagnostic descriptors for the ChangedTick generator.
    /// These provide helpful error and warning messages to developers.
    /// </summary>
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor MissingPartialKeyword = new(
            id: "CTSG0001",
            title: "Component must be declared as partial",
            messageFormat: "Component '{0}' must be declared as 'partial struct' or 'partial class' to use [AutoChangedTick]",
            category: "ChangedTickGenerator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Source generators can only add members to partial types. Add the 'partial' keyword to your component declaration.");

        public static readonly DiagnosticDescriptor MissingChangedTickField = new(
            id: "CTSG0002", 
            title: "Component must have ChangedTick field",
            messageFormat: "Component '{0}' must have a 'public long ChangedTick' field to use [AutoChangedTick]",
            category: "ChangedTickGenerator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The generator needs a ChangedTick field to update when tracked properties change.");

        public static readonly DiagnosticDescriptor NoTrackedFields = new(
            id: "CTSG0003",
            title: "Component has no tracked fields", 
            messageFormat: "Component '{0}' has [AutoChangedTick] but no fields marked with [Track]",
            category: "ChangedTickGenerator",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Mark fields with [Track] attribute to generate properties with ChangedTick tracking.");

        public static readonly DiagnosticDescriptor GenerationSuccess = new(
            id: "CTSG0004",
            title: "ChangedTick properties generated successfully",
            messageFormat: "Generated ChangedTick properties for '{0}' with {1} tracked field(s)",
            category: "ChangedTickGenerator", 
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Properties with automatic ChangedTick tracking have been generated successfully.");

        public static readonly DiagnosticDescriptor GenerationError = new(
            id: "CTSG0005",
            title: "Error generating ChangedTick properties",
            messageFormat: "Failed to generate ChangedTick properties for '{0}': {1}",
            category: "ChangedTickGenerator",
            defaultSeverity: DiagnosticSeverity.Error, 
            isEnabledByDefault: true,
            description: "An unexpected error occurred during code generation.");
    }
}