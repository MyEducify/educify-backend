using System;

var commitMsgFile = args.FirstOrDefault();
if (string.IsNullOrEmpty(commitMsgFile) || !File.Exists(commitMsgFile))
{
    Console.WriteLine("❌ Commit message file not found.");
    return 1;
}

var commitMessage = File.ReadAllText(commitMsgFile).Trim();

if (string.IsNullOrWhiteSpace(commitMessage))
{
    Console.WriteLine("❌ Commit message cannot be empty.");
    return 1;
}

// ✅ Example rule: must start with feat:, fix:, chore:, or refactor:
var allowedPrefixes = new[] { "feat:", "fix:", "chore:", "refactor:" };

if (!allowedPrefixes.Any(p => commitMessage.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
{
    Console.WriteLine("❌ Commit message must start with one of: " + string.Join(", ", allowedPrefixes));
    Console.WriteLine("➡ Example: feat: add login API");
    return 1;
}

Console.WriteLine("✅ Commit message is valid.");
return 0;
