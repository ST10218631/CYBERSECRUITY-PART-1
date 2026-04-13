# Cybersecurity Awareness Bot - Part 1

This project is a command-line cybersecurity awareness chatbot created for South African citizens. It teaches users how to spot phishing, create stronger passwords, browse more safely, and avoid common social-engineering tricks.

## Features

- Voice greeting that plays when the application launches.
- ASCII art title screen for a more polished console experience.
- Personalised interaction by asking the user for their name.
- Topic-based responses for cybersecurity questions.
- Input validation for empty or unsupported user input.
- Coloured console interface with section headers, dividers, and a typing effect.
- Most of the application logic is now kept in one main source file for easier submission and copying into Visual Studio 2022.
- GitHub Actions workflow for continuous integration.

## Project Structure

```text
CybersecurityAwarenessBot-Part1/
|-- .github/workflows/dotnet-ci.yml
|-- assets/
|   |-- ascii/cyber-banner.txt
|   `-- audio/welcome-greeting.wav
|-- src/
|   `-- CybersecurityAwarenessBot.Console/
|       |-- CybersecurityAwarenessBotApp.cs
|       |-- CybersecurityAwarenessBot.Console.csproj
`-- CybersecurityAwarenessBot.sln
```

## Topics Covered

The chatbot responds to questions about:

- password safety
- phishing emails and SMS scams
- suspicious links
- safe browsing
- malware and unsafe downloads
- social engineering

## How To Run

1. Open the project folder in a terminal.
2. Run `dotnet restore`.
3. Run `dotnet run --project src/CybersecurityAwarenessBot.Console`.
4. Ask the chatbot questions such as:
   - `How are you?`
   - `What is your purpose?`
   - `How do I spot phishing?`
   - `How do I create a strong password?`
   - `What should I do with suspicious links?`

## Voice Greeting Note

The repository already includes `assets/audio/welcome-greeting.wav` so the feature works immediately. If your lecturer requires a greeting recorded in your own voice, replace this file with your personal WAV recording before submission.

## Visual Studio 2022

This project targets `.NET 7`, which works with Visual Studio 2022.

1. Open `CybersecurityAwarenessBot.sln` in Visual Studio 2022.
2. If prompted, allow Visual Studio to restore packages or install the .NET 7 SDK.
3. Set `CybersecurityAwarenessBot.Console` as the startup project.
4. Press `Ctrl+F5` or click `Start` to run the chatbot.

## Continuous Integration

The workflow file is stored at `.github/workflows/dotnet-ci.yml` and performs:

- repository checkout
- .NET SDK setup
- solution restore
- release build validation

## README Submission Checklist

- [ ] Push this folder to GitHub.
- [ ] Make at least six meaningful commits.
- [ ] Confirm the GitHub Actions workflow shows a green check mark.
- [ ] Add a screenshot of the successful GitHub Actions run to this README.
- [ ] Add your unlisted YouTube presentation link to this README before final submission.

## Suggested Commit Plan

If you still need your six required commits, this sequence works well:

1. Initial solution and console app setup
2. Added ASCII art header and console styling
3. Implemented personalised greeting and conversation flow
4. Added cybersecurity topic responses and validation
5. Added voice greeting asset and audio playback support
6. Added README and GitHub Actions CI workflow
