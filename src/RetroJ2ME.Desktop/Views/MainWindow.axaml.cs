
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;

using RetroJ2ME.Desktop.Models;

namespace RetroJ2ME.Desktop.Views;

public partial class MainWindow : Window
{
    private readonly string _dataDirectory =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RetroJ2ME");

    private string LibraryFile =>
        Path.Combine(_dataDirectory, "library.json");

    private readonly List<GameInfo> _games = new();

    private string _selectedDevice = "Nokia 6300";

    public MainWindow()
    {
        InitializeComponent();

        LoadLibrary();
        ShowLibrary();
    }

    // =========================================================
    // NAVIGATION
    // =========================================================

    private void Library_Click(object? sender, RoutedEventArgs e)
    {
        ShowLibrary();
    }

    private async void Import_Click(object? sender, RoutedEventArgs e)
    {
        await ImportGamesAsync();
    }

    private void Devices_Click(object? sender, RoutedEventArgs e)
    {
        ShowDevices();
    }

    private void Recent_Click(object? sender, RoutedEventArgs e)
    {
        ShowRecent();
    }

    private void Settings_Click(object? sender, RoutedEventArgs e)
    {
        ShowSettings();
    }

    private void About_Click(object? sender, RoutedEventArgs e)
    {
        ShowAbout();
    }

    private void SetHeader(string title, string subtitle)
    {
        PageTitle.Text = title;
        PageSubtitle.Text = subtitle;
    }

    // =========================================================
    // LIBRARY
    // =========================================================

    private void ShowLibrary()
    {
        SetHeader(
            "Game Library",
            "Your Java ME gaming collection");

        var panel = CreatePage();

        panel.Children.Add(CreateHeading("Your Games"));

        panel.Children.Add(CreateInfo(
            $"{_games.Count} game(s) in your collection"));

        if (_games.Count == 0)
        {
            panel.Children.Add(CreateInfo(
                "Your library is empty. Import a Java ME .jar game to get started."));
        }

        foreach (var game in _games.ToList())
        {
            var card = CreateGameCard(game);
            panel.Children.Add(card);
        }

        PageHost.Content = new ScrollViewer
        {
            Content = panel
        };
    }

    private Border CreateGameCard(GameInfo game)
    {
        var cardContent = new StackPanel
        {
            Spacing = 8
        };

        cardContent.Children.Add(new TextBlock
        {
            Text = game.Name,
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White
        });

        cardContent.Children.Add(CreateInfo(
            $"Vendor: {game.Vendor}"));

        cardContent.Children.Add(CreateInfo(
            $"Version: {game.Version}"));

        cardContent.Children.Add(CreateInfo(
            $"File size: {FormatFileSize(game.FileSize)}"));

        cardContent.Children.Add(CreateInfo(
            $"File: {game.JarPath}"));

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var launchButton = new Button
        {
            Content = "Launch",
            Background = Brush.Parse("#2563EB"),
            Foreground = Brushes.White,
            Padding = new Thickness(18, 9)
        };

        launchButton.Click += (_, _) => LaunchGame(game);

        var removeButton = new Button
        {
            Content = "Remove",
            Background = Brush.Parse("#263247"),
            Foreground = Brushes.White,
            Padding = new Thickness(18, 9)
        };

        removeButton.Click += (_, _) => RemoveGame(game);

        buttons.Children.Add(launchButton);
        buttons.Children.Add(removeButton);

        cardContent.Children.Add(buttons);

        return new Border
        {
            Background = Brush.Parse("#172235"),
            BorderBrush = Brush.Parse("#263247"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(18),
            Margin = new Thickness(0, 8),
            Child = cardContent
        };
    }

    private void RemoveGame(GameInfo game)
    {
        _games.Remove(game);

        SaveLibrary();
        ShowLibrary();
    }

    // =========================================================
    // JAR IMPORT
    // =========================================================

    private async Task ImportGamesAsync()
    {
        try
        {
            var storage = TopLevel.GetTopLevel(this)?.StorageProvider;

            if (storage == null)
            {
                return;
            }

            var files = await storage.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Import Java ME Game",
                    AllowMultiple = true,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("Java Archive")
                        {
                            Patterns = new[] { "*.jar" }
                        }
                    }
                });

            foreach (var file in files)
            {
                string jarPath = file.Path.LocalPath;

                if (!File.Exists(jarPath))
                {
                    continue;
                }

                if (_games.Any(g =>
                    string.Equals(
                        g.JarPath,
                        jarPath,
                        StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var game = ReadJarInformation(jarPath);

                if (game != null)
                {
                    _games.Add(game);
                }
            }

            SaveLibrary();
            ShowLibrary();
        }
        catch (Exception ex)
        {
            ShowErrorPage(
                "Import Failed",
                ex.Message);
        }
    }

    private GameInfo? ReadJarInformation(string jarPath)
    {
        try
        {
            var fileInfo = new FileInfo(jarPath);

            string name = Path.GetFileNameWithoutExtension(jarPath);
            string vendor = "Unknown";
            string version = "Unknown";

            using var archive = ZipFile.OpenRead(jarPath);

            var manifest = archive.GetEntry(
                "META-INF/MANIFEST.MF");

            if (manifest != null)
            {
                using var reader = new StreamReader(
                    manifest.Open());

                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(
                        "MIDlet-Name:",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        name = line["MIDlet-Name:".Length..].Trim();
                    }
                    else if (line.StartsWith(
                        "MIDlet-Vendor:",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        vendor = line["MIDlet-Vendor:".Length..].Trim();
                    }
                    else if (line.StartsWith(
                        "MIDlet-Version:",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        version = line["MIDlet-Version:".Length..].Trim();
                    }
                }
            }

            return new GameInfo
            {
                Name = name,
                Vendor = vendor,
                Version = version,
                JarPath = jarPath,
                FileSize = fileInfo.Length,
                HasBeenPlayed = false
            };
        }
        catch
        {
            // Invalid or unreadable JAR files are skipped.
            return null;
        }
    }

    // =========================================================
    // GAME LAUNCH
    // =========================================================

    private void LaunchGame(GameInfo game)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(game.JarPath) ||
                !File.Exists(game.JarPath))
            {
                ShowErrorPage(
                    "Game Not Found",
                    "The original JAR file could not be found:\n" +
                    game.JarPath);
                return;
            }

            string baseDirectory = AppContext.BaseDirectory;

            string javaPath = Path.Combine(
                baseDirectory,
                "runtime",
                "jdk-21.0.13+9-jre",
                "bin",
                "java.exe");

            string emulatorPath = Path.Combine(
                baseDirectory,
                "Emulator",
                "freej2me_plus.jar");

            if (!File.Exists(javaPath))
            {
                ShowErrorPage("Java Runtime Not Found", javaPath);
                return;
            }

            if (!File.Exists(emulatorPath))
            {
                ShowErrorPage("Emulator Not Found", emulatorPath);
                return;
            }

            string gameUrl = "file:////" + game.JarPath;

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = javaPath,
                WorkingDirectory = Path.GetDirectoryName(emulatorPath)!,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            startInfo.ArgumentList.Add("-jar");
            startInfo.ArgumentList.Add(emulatorPath);
            startInfo.ArgumentList.Add(gameUrl);

            var emulatorProcess =
                System.Diagnostics.Process.Start(startInfo);

            if (emulatorProcess == null)
            {
                ShowErrorPage(
                    "Launch Failed",
                    "Windows could not start the emulator.");
                return;
            }

            game.HasBeenPlayed = true;
            SaveLibrary();
        }
        catch (Exception ex)
        {
            ShowErrorPage("Game Launch Failed", ex.ToString());
        }
    }
    // DEVICE PROFILES
    // =========================================================

    private void ShowDevices()
    {
        SetHeader(
            "Device Profiles",
            "Choose the Java ME device to emulate");

        var panel = CreatePage();

        panel.Children.Add(
            CreateHeading("Available Devices"));

        var devices = new[]
        {
            "Nokia 6300",
            "Nokia N95",
            "Nokia 6600",
            "Sony Ericsson K750i",
            "Sony Ericsson W800i",
            "Samsung SGH-D900",
            "Motorola RAZR V3",
            "Generic MIDP 2.0"
        };

        var selector = new ComboBox
        {
            ItemsSource = devices,
            SelectedItem = _selectedDevice,
            MinWidth = 260,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        var description = CreateInfo(
            "Selected profile: " + _selectedDevice);

        selector.SelectionChanged += (_, _) =>
        {
            if (selector.SelectedItem is string selected)
            {
                _selectedDevice = selected;
                description.Text =
                    "Selected profile: " + selected;
            }
        };

        panel.Children.Add(CreateLabel("Select device"));
        panel.Children.Add(selector);
        panel.Children.Add(description);

        panel.Children.Add(CreateInfo(
            "Device profiles currently provide selection only. " +
            "Screen dimensions, keypad mapping and JSR compatibility " +
            "will be implemented with the emulator integration."));

        PageHost.Content = new ScrollViewer
        {
            Content = panel
        };
    }

    // =========================================================
    // RECENTLY PLAYED
    // =========================================================

    private void ShowRecent()
    {
        SetHeader(
            "Recently Played",
            "Your previously selected games");

        var panel = CreatePage();

        panel.Children.Add(
            CreateHeading("Recent Games"));

        var recentGames = _games
            .Where(g => g.HasBeenPlayed)
            .ToList();

        if (recentGames.Count == 0)
        {
            panel.Children.Add(CreateInfo(
                "No recently played games yet."));
        }

        foreach (var game in recentGames)
        {
            panel.Children.Add(CreateGameCard(game));
        }

        PageHost.Content = new ScrollViewer
        {
            Content = panel
        };
    }

    // =========================================================
    // SETTINGS
    // =========================================================

    private void ShowSettings()
    {
        SetHeader(
            "Settings",
            "Configure RetroJ2ME");

        var panel = CreatePage();

        panel.Children.Add(
            CreateHeading("Application Settings"));

        panel.Children.Add(CreateInfo(
            "Selected device: " + _selectedDevice));

        panel.Children.Add(CreateInfo(
            "Library location: " + _dataDirectory));

        panel.Children.Add(CreateInfo(
            "Game files are referenced from their original locations. " +
            "Removing a game from the library does not delete its JAR file."));

        panel.Children.Add(CreateInfo(
            "Emulator engine settings will be available after integration."));

        PageHost.Content = new ScrollViewer
        {
            Content = panel
        };
    }

    // =========================================================
    // ABOUT
    // =========================================================

    private void ShowAbout()
    {
        SetHeader(
            "About RetroJ2ME",
            "Java ME gaming on modern desktop systems");

        var panel = CreatePage();

        panel.Children.Add(
            CreateHeading("RetroJ2ME"));

        panel.Children.Add(CreateInfo(
            "A modern desktop launcher for Java ME / J2ME games."));

        panel.Children.Add(CreateInfo(
            "Built with C#, .NET 10 and Avalonia UI."));

        panel.Children.Add(CreateInfo(
            "The emulator engine is not yet integrated."));

        panel.Children.Add(CreateInfo(
            "Version 0.1 - Development Build"));

        PageHost.Content = new ScrollViewer
        {
            Content = panel
        };
    }

    // =========================================================
    // LOCAL LIBRARY STORAGE
    // =========================================================

    private void LoadLibrary()
    {
        try
        {
            Directory.CreateDirectory(_dataDirectory);

            if (!File.Exists(LibraryFile))
            {
                return;
            }

            string json = File.ReadAllText(LibraryFile);

            var loaded = JsonSerializer.Deserialize<List<GameInfo>>(
                json);

            if (loaded != null)
            {
                _games.Clear();
                _games.AddRange(loaded);
            }
        }
        catch
        {
            _games.Clear();
        }
    }

    private void SaveLibrary()
    {
        try
        {
            Directory.CreateDirectory(_dataDirectory);

            string json = JsonSerializer.Serialize(
                _games,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(LibraryFile, json);
        }
        catch (Exception ex)
        {
            ShowErrorPage(
                "Library Save Failed",
                ex.Message);
        }
    }

    // =========================================================
    // UI HELPERS
    // =========================================================

    private StackPanel CreatePage()
    {
        return new StackPanel
        {
            Margin = new Thickness(28),
            Spacing = 14
        };
    }

    private TextBlock CreateHeading(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 4, 0, 8)
        };
    }

    private TextBlock CreateLabel(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 8, 0, 2)
        };
    }

    private TextBlock CreateInfo(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = Brush.Parse("#AAB7CC"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 2)
        };
    }

    private void ShowErrorPage(string title, string message)
    {
        SetHeader(title, "RetroJ2ME");

        var panel = CreatePage();

        panel.Children.Add(CreateHeading(title));
        panel.Children.Add(CreateInfo(message));

        PageHost.Content = new ScrollViewer
        {
            Content = panel
        };
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        if (bytes < 1024 * 1024)
        {
            return $"{bytes / 1024.0:F2} KB";
        }

        return $"{bytes / (1024.0 * 1024.0):F2} MB";
    }
}
