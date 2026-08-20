# Review Me

A small .NET CLI tool for preparing Git diffs for AI-assisted code review.

Review Me collects changes from your Git repository, removes potentially sensitive content, formats the result as a Markdown diff, and copies it directly to your clipboard.

## Features

- Get uncommitted Git changes
- Compare the current branch with another branch
- Hide lines containing sensitive words
- Ignore configured files
- Generate AI-friendly Markdown diff output
- Automatically copy the generated diff to clipboard
- Simple local configuration
- Standalone builds for Windows and Linux

## Download

Pre-built standalone executables are available in the **Releases** section.

Available builds:

- Windows x64
- Linux x64

The standalone builds include the required .NET runtime, so you do not need to install .NET separately.

## Requirements

- Git
- A Git repository

## Usage

Run Review Me from inside a Git repository.

### Windows

Get uncommitted changes:

```bash
ReviewMe.exe
```

or:

```bash
ReviewMe.exe changes
```

Compare the current branch with another branch:

```bash
ReviewMe.exe branch dev
```

### Linux

Make the downloaded file executable:

```bash
chmod +x ReviewMe
```

Get uncommitted changes:

```bash
./ReviewMe
```

or:

```bash
./ReviewMe changes
```

Compare the current branch with another branch:

```bash
./ReviewMe branch dev
```

If no branch name is provided, `dev` is used by default.

## Configuration

On the first run, Review Me creates a local `config.json` file.

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

### Sensitive Words

Any changed line containing one of the configured `NotSafeWords` is replaced with:

```text
[REMOVED]
```

This helps prevent accidentally sending sensitive values when sharing the generated diff with an AI assistant.

### Ignored Files

Files added to `IgnoredFiles` are excluded from the generated diff.

## Output

The generated output looks like this:

```diff
## File: Example.cs
- old code
+ new code
+ [REMOVED]
```

The result is printed to the console and automatically copied to your clipboard.

You can then paste it directly into ChatGPT or any other AI assistant for code review.

## Build From Source

If you want to build the project yourself, .NET 10 SDK is required.

Windows x64:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o publish/windows
```

Linux x64:

```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o publish/linux
```

## Status

This was a small experimental project and is not actively developed.
