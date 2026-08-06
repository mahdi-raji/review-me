using System.Text;
using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace ReviewMe;

public class GitService(IRepository repository)
{
    //todo: handle not safe words with file 
    private static readonly List<string> NotSafeWords = ["password", "username", "connection string"];
    //todo: add files to ignore(*.mrt etc)
    //todo: handle giving context (for example in console app first listing the changed files, then asking for the files that user thinks it can give more context )
    //todo: handle more scenarios (HEAD, Other Commit etc)
    public List<DiffInfo> CompareWithBranch(string branchName = null)
    {
        branchName ??= "dev";
        
        var diff = new List<DiffInfo>();

        var pattern = @"\b(" + string.Join("|", NotSafeWords.Select(Regex.Escape)) + @")\b";
        var badWordsRegex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);


        var branch = repository.Branches[branchName];

        var branchTree = branch.Tip?.Tree;
        var currentTree = repository.Head.Tip?.Tree;

        if (branch is null)
            throw new Exception($"Branch {branchName} not found");

        if (branchTree is null || currentTree is null)
            return diff;

        foreach (var c in repository.Diff.Compare<Patch>(branchTree, currentTree))
        {
            var result = new DiffInfo(c);
            foreach (var rawLine in c.Patch.Split('\n'))
            {
                var line = rawLine.TrimEnd('\r');
                
                if (line.StartsWith("--- ") || line.StartsWith("+++ "))
                    continue;
                
                if(line == "\\ No newline at end of file")
                    continue;

                var type = line.FirstOrDefault() switch
                {
                    '+' => DiffLineType.Added,
                    '-' => DiffLineType.Deleted,
                    _ => (DiffLineType?)null
                };

                if(line.Length < 2)
                    continue;
                
                var content = line[1..];

                if (type != null)
                    result.AddLine(new DiffLine
                    {
                        Type = type.Value,
                        Content = content,
                        IsBlocked = badWordsRegex.IsMatch(content)
                    });
            }

            diff.Add(result);
        }

        return diff;
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


            return builder.ToString();
        }
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