using Graph.RenderingSample;
using StateForge.Core.Introspection;
using StateForge.Visualization.Graphviz.Rendering;
using StateForge.Visualization.Mermaid.Rendering;
using StateForge.Visualization.PlantUML.Rendering;

var outputDirectory = Path.Combine("artifacts", "graph-rendering");
Directory.CreateDirectory(outputDirectory);

foreach (var existingFile in Directory.EnumerateFiles(outputDirectory)) File.Delete(existingFile);

var generatedArtifacts = new List<string>();

WriteArtifacts("order-flow", SampleGraphs.OrderFlow());
WriteArtifacts("offer-order-invoice-cancellation-flow", SampleGraphs.OfferOrderInvoiceCancellationFlow());

Console.WriteLine($"Graph rendering sample completed: {string.Join(", ", generatedArtifacts)}");

void WriteArtifacts<TState, TEvent>(string prefix, DefinitionGraph<TState, TEvent> graph)
{
    var mermaidPath = Path.Combine(outputDirectory, $"{prefix}.mmd");
    var graphvizPath = Path.Combine(outputDirectory, $"{prefix}.dot");
    var plantUmlPath = Path.Combine(outputDirectory, $"{prefix}.puml");

    File.WriteAllText(mermaidPath, MermaidGraphRenderer.Render(graph));
    File.WriteAllText(graphvizPath, GraphvizDotRenderer.Render(graph));
    File.WriteAllText(plantUmlPath, PlantUmlGraphRenderer.Render(graph));

    generatedArtifacts.Add(mermaidPath);
    generatedArtifacts.Add(graphvizPath);
    generatedArtifacts.Add(plantUmlPath);
}