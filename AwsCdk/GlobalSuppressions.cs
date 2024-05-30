[assembly:
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Potential Code Quality Issues",
        "RECS0026:Possible unassigned object created by 'new'",
        Justification = "Constructs add themselves to the scope in which they are created")]
[assembly:
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.OrderingRules",
        "SA1201:Elements should appear in the correct order",
        Justification = "Reviewed.")]
[assembly:
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Naming",
        "CA1711:Identifiers should not have incorrect suffix",
        Justification = "This is a stack.")]
[assembly:
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1859:Use concrete types when possible for improved performance",
        Justification = "This is not a high load code.")]