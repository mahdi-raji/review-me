# Review Me

A small .NET CLI tool for preparing Git diffs to review with AI tools such as ChatGPT.

It collects changed lines from your Git repository, removes potentially sensitive content, formats the result as a Markdown diff, and copies it to your clipboard.

## Features

* Get uncommitted Git changes
* Compare the current branch with another branch
* Hide lines containing sensitive words
* Generate AI-friendly Markdown diff output
* Automatically copy the result to clipboard
* Simple local configuration

## Requirements

* .NET 10
* Git repository

## Usage

Run the application from inside a Git repository.

### Uncommitted changes

```bash
dotnet run --project ReviewMe -- changes
```

`changes` is also the default command:

```bash
dotnet run --project ReviewMe
```

### Compare with a branch

```bash
dotnet run --project ReviewMe -- branch dev
```

If no branch name is provided, `dev` is used by default.

## Configuration

On the first run, Review Me creates a `config.json` file under the application's user data directory.

Example:

```json
{
  "NotSafeWords": [
    "password",
    "username",
    "connection string"
  ],
  "IgnoredFiles": []
}
```

Lines containing configured sensitive words are replaced with:

```text
[REMOVED]
```

before the diff is copied to the clipboard.

## Output

The generated output looks like this:

```diff
## File: Example.cs
- old code
+ new code
+ [REMOVED]
```

You can paste the generated diff directly into your preferred AI assistant for code review.

## Status

This was a small experimental project and is not actively developed.
