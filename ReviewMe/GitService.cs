using System.Text;
using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace ReviewMe;

//todo: add files to ignore(*.mrt etc)
//todo: handle giving context (for example in console app first listing the changed files, then asking for the files that user thinks it can give more context )
public class GitService
{
    private readonly IRepository _repository;
    private readonly Regex _badWordsRegex;

    public GitService(IRepository repository, ApplicationConfig config)
    {
        _repository = repository;
        _badWordsRegex = new Regex(@"\b(" + string.Join("|", config.NotSafeWords.Select(Regex.Escape)) + @")\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
    
    public List<DiffInfo> CompareWithBranch(string branchName = null)
    {
        branchName ??= "dev";

        var branch = _repository.Branches[branchName];

        if (branch is null)
            throw new Exception($"Branch {branchName} not found");


        var currentCommit = _repository.Head.Tip;
        var branchCommit = branch.Tip;
        
        if (currentCommit is null || branchCommit is null)
            return [];

        var mergeBase = _repository.ObjectDatabase.FindMergeBase(branchCommit, currentCommit);

        if (mergeBase is null)
            return [];

        var changes = _repository.Diff.Compare<Patch>(mergeBase.Tree, currentCommit.Tree);

        return CompareChanges(changes);
    }

    public List<DiffInfo> GetUncommittedChanges()
    {
        var changes = _repository.Diff.Compare<Patch>(_repository.Head.Tip.Tree, DiffTargets.Index | DiffTargets.WorkingDirectory);

        return CompareChanges(changes);
    }

    private List<DiffInfo> CompareChanges(Patch changes)
    {
        var diff = new List<DiffInfo>();

        foreach (var c in changes)
        {
            var result = new DiffInfo(c);
            foreach (var rawLine in c.Patch.Split('\n'))
            {
                var line = rawLine.TrimEnd('\r');

                if (line.StartsWith("--- ") || line.StartsWith("+++ "))
                    continue;

                if (line == "\\ No newline at end of file")
                    continue;

                var type = line.FirstOrDefault() switch
                {
                    '+' => DiffLineType.Added,
                    '-' => DiffLineType.Deleted,
                    _ => (DiffLineType?)null
                };

                if (line.Length < 2)
                    continue;

                var content = line[1..];

                if (type != null)
                    result.AddLine(new DiffLine
                    {
                        Type = type.Value,
                        Content = content,
                        IsBlocked = _badWordsRegex.IsMatch(content)
                    });
            }

            diff.Add(result);
        }

        return diff;
    }
}

public class DiffInfo(PatchEntryChanges changes)
{
    private PatchEntryChanges Changes { get; set; } = changes;

    public string FilePath => Changes.Path;

    public List<DiffLine> Lines { get; private set; } = [];

    public void AddLine(DiffLine line)
    {
        Lines.Add(line);
    }

    public string ToSafeDiff()
    {
        var builder = new StringBuilder();

        builder.AppendLine("```diff");

        builder.AppendLine($"## File: {FilePath}");

        foreach (var line in Lines)
        {
            var prefix = line.Type switch
            {
                DiffLineType.Added => "+",
                DiffLineType.Deleted => "-",
                _ => " "
            };

            var content = line.IsBlocked ? "[REMOVED]" : line.Content;

            builder.AppendLine($"{prefix} {content}");
        }

        builder.AppendLine("```");

        return builder.ToString();
    }
}

public enum DiffLineType
{
    Added,
    Deleted,
}

public class DiffLine
{
    public DiffLineType Type { get; init; }

    public bool IsBlocked { get; set; }

    public string Content { get; init; } = string.Empty;
}