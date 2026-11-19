// Legacy estate inventory: counts migration-relevant API usage per file.
// Run with the .NET 10 SDK (file-based app, no project needed):
//   dotnet run scripts/inventory.cs -- legacy docs/inventory.md
// The output is a starting point for a human assessment, not a verdict.
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

var Inv = CultureInfo.InvariantCulture;
var root = args.Length > 0 ? args[0] : "legacy";
var output = args.Length > 1 ? args[1] : null;

var rules = new (string Area, string Signal, Regex Pattern, string Risk)[]
{
    ("System.Web", "HttpContext.Current", new(@"HttpContext\.Current"), "High"),
    ("System.Web", "Session state", new(@"\bSession\["), "High"),
    ("System.Web", "Server.MapPath", new(@"Server\.MapPath"), "Medium"),
    ("System.Web", "Global.asax events", new(@"Application_(Start|Error|BeginRequest|AuthenticateRequest)"), "Medium"),
    ("Web Forms", "Page / code-behind", new(@":\s*(System\.Web\.UI\.)?Page\b"), "High"),
    ("Web Forms", "Server controls", new(@"<asp:(GridView|Repeater|FormView|DetailsView|ListView|UpdatePanel)"), "High"),
    ("Web Forms", "ViewState / postback", new(@"\bViewState\b|IsPostBack|AutoPostBack"), "Medium"),
    ("Web Forms", "AjaxControlToolkit", new(@"AjaxControlToolkit|ajaxToolkit:"), "High"),
    ("Auth", "Forms auth / Membership", new(@"FormsAuthentication|Membership\.|SqlMembershipProvider"), "High"),
    ("Auth", "Roles", new(@"IsInRole|SqlRoleProvider|Roles\."), "Medium"),
    ("Auth", "machineKey", new(@"<machineKey"), "High"),
    ("WCF", "Service contracts", new(@"\[ServiceContract|\[OperationContract"), "High"),
    ("WCF", "system.serviceModel config", new(@"<system\.serviceModel>"), "High"),
    ("WCF", "ASP.NET compatibility mode", new(@"aspNetCompatibilityEnabled=""true""|AspNetCompatibilityRequirements"), "High"),
    ("EF6", "EF6 namespaces", new(@"System\.Data\.Entity"), "High"),
    ("EF6", "DbModelBuilder / EntityTypeConfiguration", new(@"DbModelBuilder|EntityTypeConfiguration<"), "Medium"),
    ("EF6", "EDMX model", new(@"\.edmx"), "High"),
    ("Config", "ConfigurationManager", new(@"ConfigurationManager\."), "Low"),
    ("Config", "appSettings", new(@"<appSettings>"), "Low"),
    ("Logging", "log4net / Trace", new(@"log4net|Trace\.Write"), "Low"),
    ("Semantics", "DateTime.Now (local time)", new(@"DateTime\.Now\b"), "Medium"),
    ("Semantics", "Culture-sensitive casing", new(@"\.ToUpper\(\)|\.ToLower\(\)"), "Medium"),
    ("Semantics", "Culture-sensitive formatting", new(@"ToString\(""[CcNn]\d?""\)|\{0:[Cc]\}"), "Medium"),
    ("Semantics", "Midpoint rounding (default ToEven)", new(@"Math\.Round\([^,()]+,\s*\d+\)"), "Medium"),
};

var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    { ".cs", ".vb", ".aspx", ".ascx", ".master", ".asax", ".config", ".svc", ".edmx", ".cshtml" };

var hits = new List<(string Area, string Signal, string Risk, string File, int Count)>();
foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
             .Where(f => extensions.Contains(Path.GetExtension(f)))
             .OrderBy(f => f, StringComparer.Ordinal))
{
    var text = File.ReadAllText(file);
    foreach (var (area, signal, pattern, risk) in rules)
    {
        int count = pattern.Count(text);
        if (count > 0)
        {
            hits.Add((area, signal, risk, Path.GetRelativePath(root, file), count));
        }
    }
}

var packages = Directory.EnumerateFiles(root, "packages.config", SearchOption.AllDirectories)
    .SelectMany(f => Regex.Matches(File.ReadAllText(f), @"id=""([^""]+)""\s+version=""([^""]+)""")
        .Select(m => $"{m.Groups[1].Value} {m.Groups[2].Value} ({Path.GetRelativePath(root, f)})"))
    .ToList();

var md = new StringBuilder();
md.AppendLine(Inv, $"# Legacy inventory: `{root}`").AppendLine();
md.AppendLine("| Area | Signal | Risk | Files | Occurrences |").AppendLine("|---|---|---|---|---|");
foreach (var g in hits.GroupBy(h => (h.Area, h.Signal, h.Risk)).OrderBy(g => g.Key.Area, StringComparer.Ordinal))
{
    md.AppendLine(Inv, $"| {g.Key.Area} | {g.Key.Signal} | {g.Key.Risk} | {g.Count()} | {g.Sum(h => h.Count)} |");
}

md.AppendLine().AppendLine("## Files by signal").AppendLine();
foreach (var h in hits.OrderBy(h => h.File, StringComparer.Ordinal))
{
    md.AppendLine(Inv, $"- `{h.File}`: {h.Signal} ×{h.Count}");
}

md.AppendLine().AppendLine("## packages.config").AppendLine();
packages.ForEach(p => md.AppendLine(Inv, $"- {p}"));

if (output is null)
{
    Console.Write(md.ToString());
}
else
{
    File.WriteAllText(output, md.ToString());
    Console.WriteLine($"Wrote {hits.Count} findings to {output}");
}
