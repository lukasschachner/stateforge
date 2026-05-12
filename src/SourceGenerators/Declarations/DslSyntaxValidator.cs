using Microsoft.CodeAnalysis.CSharp.Syntax;
using StateMachineLibrary.SourceGenerators.Diagnostics;

namespace StateMachineLibrary.SourceGenerators.Declarations;

public static class DslSyntaxValidator
{
    public static bool IsUnsupportedStatement(StatementSyntax statement)
    {
        return statement is not ExpressionStatementSyntax;
    }

    public static void ReportUnsupported(StatementSyntax statement, DiagnosticReporter reporter)
    {
        reporter.UnsupportedDsl(statement.Kind().ToString(), statement.GetLocation());
    }
}