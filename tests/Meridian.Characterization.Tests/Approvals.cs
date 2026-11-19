using System.Runtime.CompilerServices;

namespace Meridian.Characterization.Tests;

/// <summary>
/// Minimal approval-file helper (no third-party dependency). On mismatch it
/// writes NAME.received.txt next to NAME.approved.txt so a human can diff them.
/// Approving = renaming received to approved in a reviewed commit.
/// </summary>
public static class Approvals
{
    public static void AssertMatches(string name, string actual, [CallerFilePath] string source = "")
    {
        var dir = Path.Combine(Path.GetDirectoryName(source)!, "golden");
        var approved = Path.Combine(dir, name + ".approved.txt");
        var received = Path.Combine(dir, name + ".received.txt");

        var expected = File.Exists(approved) ? File.ReadAllText(approved).ReplaceLineEndings("\n") : null;
        if (expected == actual)
        {
            File.Delete(received);
            return;
        }

        File.WriteAllText(received, actual);
        Assert.Fail(expected is null
            ? $"No approved snapshot yet. Review {received} and rename it to {Path.GetFileName(approved)}."
            : $"Snapshot differs. Compare {received} with {approved}.");
    }
}
