using LibGit2Sharp;
using ReviewMe;
using TextCopy;

var repositoryPath = Repository.Discover(Directory.GetCurrentDirectory());

var branchName = args.FirstOrDefault();

if (repositoryPath is null)
{
    Console.WriteLine("no repository found");
    return;
}

var gitService = new GitService(new Repository(repositoryPath));

var diffs = gitService.CompareWithBranch(branchName);

var safeDiff = string.Join(' ', diffs.Select(x => x.ToSafeDiff()));
await ClipboardService.SetTextAsync(safeDiff);

Console.WriteLine(safeDiff);