using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CybersecurityAwarenessBot.ConsoleApp;

internal sealed class Program
{
    private static void Main()
    {
        var assetLocator = new AssetLocator();
        var consoleUi = new ConsoleUi(assetLocator);
        var audioGreetingPlayer = new AudioGreetingPlayer(assetLocator, consoleUi);
        var responseService = new CybersecurityResponseService();
        var chatbotApplication = new ChatbotApplication(consoleUi, audioGreetingPlayer, responseService);

        chatbotApplication.Run();
    }
}

internal sealed class ChatbotApplication
{
    private readonly ConsoleUi _consoleUi;
    private readonly AudioGreetingPlayer _audioGreetingPlayer;
    private readonly CybersecurityResponseService _responseService;

    public ChatbotApplication(
        ConsoleUi consoleUi,
        AudioGreetingPlayer audioGreetingPlayer,
        CybersecurityResponseService responseService)
    {
        _consoleUi = consoleUi;
        _audioGreetingPlayer = audioGreetingPlayer;
        _responseService = responseService;
    }

    public void Run()
    {
        _consoleUi.ConfigureConsole();
        _consoleUi.ShowLaunchScreen();
        _audioGreetingPlayer.PlayGreeting();

        var user = CaptureUserProfile();

        _consoleUi.ShowWelcomeMessage(user.Name, _responseService.GetSupportedTopics());
        RunConversationLoop(user);
        _consoleUi.ShowFarewellMessage(user.Name);
    }

    private UserProfile CaptureUserProfile()
    {
        while (true)
        {
            var name = _consoleUi.PromptForInput("Before we begin, what is your name?");

            if (string.IsNullOrWhiteSpace(name))
            {
                _consoleUi.ShowValidationMessage("Please enter a name so I can personalise the conversation.");
                continue;
            }

            return new UserProfile
            {
                Name = name.Trim(),
                StartedAt = DateTime.Now,
                ConversationCount = 0
            };
        }
    }

    private void RunConversationLoop(UserProfile user)
    {
        while (true)
        {
            var input = _consoleUi.PromptForInput("Ask me a cybersecurity question, or type exit to close the bot.");

            if (string.IsNullOrWhiteSpace(input))
            {
                _consoleUi.ShowValidationMessage("I did not receive anything there. Try a question like 'How do I spot phishing?'");
                continue;
            }

            if (_responseService.IsExitCommand(input))
            {
                break;
            }

            user.ConversationCount++;
            var reply = _responseService.GetResponse(input, user.Name);
            _consoleUi.WriteBotMessage(reply);
        }
    }
}

internal sealed class UserProfile
{
    public string Name { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }

    public int ConversationCount { get; set; }
}

internal sealed class AssetLocator
{
    private readonly string _baseDirectory = AppContext.BaseDirectory;

    public string GetAudioGreetingPath()
    {
        return Path.Combine(_baseDirectory, "assets", "audio", "welcome-greeting.wav");
    }

    public string GetAsciiArtPath()
    {
        return Path.Combine(_baseDirectory, "assets", "ascii", "cyber-banner.txt");
    }

    public string LoadAsciiArt()
    {
        var path = GetAsciiArtPath();

        return File.Exists(path)
            ? File.ReadAllText(path)
            : "CYBERSECURITY AWARENESS BOT";
    }
}

internal sealed class AudioGreetingPlayer
{
    private readonly AssetLocator _assetLocator;
    private readonly ConsoleUi _consoleUi;

    public AudioGreetingPlayer(AssetLocator assetLocator, ConsoleUi consoleUi)
    {
        _assetLocator = assetLocator;
        _consoleUi = consoleUi;
    }

    public void PlayGreeting()
    {
        var audioPath = _assetLocator.GetAudioGreetingPath();

        if (!File.Exists(audioPath))
        {
            _consoleUi.ShowSystemMessage("Greeting audio file not found, so the chatbot will continue in text mode.");
            return;
        }

        try
        {
            if (OperatingSystem.IsWindows())
            {
                RunAudioProcess("powershell", "-NoProfile", "-Command", $"(New-Object System.Media.SoundPlayer '{audioPath.Replace("'", "''")}').PlaySync()");
                return;
            }

            if (OperatingSystem.IsMacOS())
            {
                RunAudioProcess("afplay", audioPath);
                return;
            }

            if (OperatingSystem.IsLinux())
            {
                RunAudioProcess("aplay", audioPath);
                return;
            }
        }
        catch (Exception)
        {
            _consoleUi.ShowSystemMessage("The greeting sound could not be played on this device, so the chatbot continued normally.");
        }
    }

    private static void RunAudioProcess(string fileName, params string[] arguments)
    {
        using var process = new Process();
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        process.StartInfo = startInfo;
        process.Start();
        _ = process.StandardOutput.ReadToEnd();
        _ = process.StandardError.ReadToEnd();
        process.WaitForExit();
    }
}

internal sealed class CybersecurityResponseService
{
    private static readonly string[] SupportedTopics =
    {
        "password safety",
        "phishing emails and SMS scams",
        "suspicious links",
        "safe browsing",
        "malware and unsafe downloads",
        "social engineering"
    };

    public IReadOnlyList<string> GetSupportedTopics()
    {
        return SupportedTopics;
    }

    public bool IsExitCommand(string input)
    {
        var normalized = Normalize(input);
        return normalized is "exit" or "quit" or "bye" or "goodbye" or "close";
    }

    public string GetResponse(string input, string userName)
    {
        var normalized = Normalize(input);

        if (ContainsAny(normalized, "hello", "hi", "hey", "good morning", "good afternoon"))
        {
            return $"Hello, {userName}. I am ready to help you stay safer online.";
        }

        if (ContainsAny(normalized, "how are you", "how are u"))
        {
            return $"I am alert and ready, {userName}. My job is to help you recognise cyber threats before they cause harm.";
        }

        if (ContainsAny(normalized, "purpose", "what is your purpose", "what s your purpose", "why are you here"))
        {
            return "My purpose is to teach people how to recognise common cyber threats, avoid online scams, and build safer digital habits.";
        }

        if (ContainsAny(normalized, "what can i ask", "help", "topics", "what do you know"))
        {
            return $"You can ask me about {string.Join(", ", SupportedTopics)}. You can also try questions like 'How do I create a strong password?'";
        }

        if (ContainsAny(normalized, "password", "passphrase", "pin", "otp", "one time pin"))
        {
            return "Use a long and unique passphrase for every important account, never share your PIN or one-time code, and enable multi-factor authentication whenever it is available.";
        }

        if (ContainsAny(normalized, "phishing", "scam email", "fake email", "fake sms", "smishing", "scam"))
        {
            return "Phishing messages often create panic by mentioning parcel delays, banking problems, or SARS refunds. Check the sender carefully, avoid urgent links, and verify the message using an official website or phone number.";
        }

        if (ContainsAny(normalized, "link", "url", "website", "suspicious site"))
        {
            return "Treat unexpected links with caution. Hover to preview them, look for misspellings in the domain name, and type the official web address yourself when the message feels suspicious.";
        }

        if (ContainsAny(normalized, "browse", "browser", "safe browsing", "wifi", "public wifi"))
        {
            return "Safe browsing means keeping your browser updated, avoiding unknown downloads, checking for HTTPS on important sites, and being extra careful on public Wi-Fi networks.";
        }

        if (ContainsAny(normalized, "malware", "virus", "attachment", "download"))
        {
            return "Malware often arrives through unexpected attachments, pirated software, or fake pop-ups. Only download files from trusted sources and scan anything that seems unusual.";
        }

        if (ContainsAny(normalized, "social engineering", "impersonation", "pretending", "manipulate"))
        {
            return "Social engineering is when attackers trick people into handing over information. Never give out passwords, banking details, or one-time codes just because someone sounds official.";
        }

        if (ContainsAny(normalized, "identity theft", "privacy", "personal details", "id number"))
        {
            return "Protect your personal information by sharing only what is necessary, reviewing social media privacy settings, and never sending identity documents unless you trust the request completely.";
        }

        return "I didn't quite understand that. Could you rephrase? You can ask me about passwords, phishing, suspicious links, malware, safe browsing, or social engineering.";
    }

    private static bool ContainsAny(string normalizedInput, params string[] phrases)
    {
        return phrases.Any(phrase => ContainsPhrase(normalizedInput, phrase));
    }

    private static bool ContainsPhrase(string normalizedInput, string phrase)
    {
        var pattern = $@"\b{Regex.Escape(phrase).Replace("\\ ", "\\s+")}\b";
        return Regex.IsMatch(normalizedInput, pattern);
    }

    private static string Normalize(string input)
    {
        var cleaned = new string(
            input
                .ToLowerInvariant()
                .Select(character => char.IsLetterOrDigit(character) || char.IsWhiteSpace(character) ? character : ' ')
                .ToArray());

        return Regex.Replace(cleaned, "\\s+", " ").Trim();
    }
}

internal sealed class ConsoleUi
{
    private readonly AssetLocator _assetLocator;
    private readonly int _typingDelayInMilliseconds;

    public ConsoleUi(AssetLocator assetLocator, int typingDelayInMilliseconds = 12)
    {
        _assetLocator = assetLocator;
        _typingDelayInMilliseconds = typingDelayInMilliseconds;
    }

    public void ConfigureConsole()
    {
        try
        {
            Console.Title = "Cybersecurity Awareness Bot";
        }
        catch (PlatformNotSupportedException)
        {
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Clear();
    }

    public void ShowLaunchScreen()
    {
        WriteDivider();
        WriteAccentBlock(_assetLocator.LoadAsciiArt());
        WriteDivider();
        ShowSystemMessage("Launching the Cybersecurity Awareness Assistant...");
        Thread.Sleep(350);
    }

    public void ShowWelcomeMessage(string userName, IReadOnlyList<string> supportedTopics)
    {
        WriteHeader("Welcome");
        WriteBotMessage($"Hello, {userName}. Welcome to the Cybersecurity Awareness Bot.");
        WriteBotMessage("I can guide you through common online threats and help you build safer habits.");
        WriteBotMessage($"Try asking about {string.Join(", ", supportedTopics)}.");
    }

    public void ShowFarewellMessage(string userName)
    {
        WriteHeader("Session Closed");
        WriteBotMessage($"Goodbye, {userName}. Stay alert, protect your accounts, and think before you click.");
    }

    public void WriteBotMessage(string message)
    {
        WriteLabeledMessage("[BOT]", message, ConsoleColor.Green);
    }

    public void ShowSystemMessage(string message)
    {
        WriteLabeledMessage("[SYS]", message, ConsoleColor.DarkCyan);
    }

    public void ShowValidationMessage(string message)
    {
        WriteLabeledMessage("[WARN]", message, ConsoleColor.Yellow);
    }

    public string PromptForInput(string message)
    {
        WriteHeader("Your Turn");
        WriteLabeledMessage("[BOT]", message, ConsoleColor.Green);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("You > ");
        Console.ForegroundColor = ConsoleColor.White;

        return Console.ReadLine() ?? string.Empty;
    }

    private void WriteHeader(string title)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine();
        Console.WriteLine($"===== {title.ToUpperInvariant()} =====");
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void WriteDivider()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(new string('=', 72));
        Console.ForegroundColor = ConsoleColor.White;
    }

    private void WriteAccentBlock(string text)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;

        foreach (var line in text.Replace("\r", string.Empty).Split('\n', StringSplitOptions.None))
        {
            Console.WriteLine(line);
        }

        Console.ForegroundColor = ConsoleColor.White;
    }

    private void WriteLabeledMessage(string label, string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write($"{label} ");
        Console.ForegroundColor = ConsoleColor.White;

        foreach (var line in message.Replace("\r", string.Empty).Split('\n', StringSplitOptions.None))
        {
            TypeLine(line);
        }
    }

    private void TypeLine(string text)
    {
        foreach (var character in text)
        {
            Console.Write(character);
            Thread.Sleep(_typingDelayInMilliseconds);
        }

        Console.WriteLine();
    }
}
