using LibGit2Sharp;
using ReviewMe;
using TextCopy;


var repositoryPath = Repository.Discover(Directory.GetCurrentDirectory());

var configService = new ConfigService();

if (repositoryPath is null)
{
    Console.WriteLine("no repository found");
    return;
}

var gitService = new GitService(new Repository(repositoryPath), configService.GetOrCreateConfig());


var commands = new Dictionary<string, Func<string, List<DiffInfo>>>
{
    ["branch"] = branchName => gitService.CompareWithBranch(branchName),
    ["changes"] = _ => gitService.GetUncommittedChanges()
};

var command = args.ElementAtOrDefault(0) ?? "changes";
var value = args.ElementAtOrDefault(1);

if (!commands.TryGetValue(command, out var action))
{
    Console.WriteLine($"unknown command: {command}");
    return;
}

var diffs = action(value);

var safeDiff = string.Join(
    Environment.NewLine,
    diffs.Select(x => x.ToSafeDiff()));

await ClipboardService.SetTextAsync(safeDiff);

Console.WriteLine(safeDiff);