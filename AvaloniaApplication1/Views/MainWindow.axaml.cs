using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace AvaloniaApplication1.Views
{
    public partial class MainWindow : Window
    {
        string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Notion Clone");
        bool Saving = false;
        bool TitleBoxIsPlaceholder = false;
        private string? CurrentPath;

        public MainWindow()
        {
            InitializeComponent();
            PreLoadNotes();
        }
        private void LoadPagesList()
        {
            PagesPanel.Children.Clear();

            if (!Directory.Exists(DataPath))
                return;

            foreach (var file in Directory.GetFiles(DataPath, "*.json"))
            {
                var json = System.IO.File.ReadAllText(file);
                var note = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (note == null) continue;

                AddPageButton(file, note["title"]);
            }
        }

        private void AddPageButton(string filePath, string title)
        {
            var btn = new Button
            {
                Content = title,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
            };

            btn.Click += (_, __) =>
            {
                // sauvegarde la note actuelle
                if (!string.IsNullOrEmpty(CurrentPath) && !TitleBoxIsPlaceholder)
                {
                    SaveNote(
                        TitleEditorBox.Text,
                        EditorTextBox.Text ?? "",
                        CurrentPath
                    );
                }

                // ouvre la note cliquée
                CurrentPath = filePath;
                LoadNote(filePath);
            };

            PagesPanel.Children.Add(btn);
        }


        private void LoadNotes(string path)
        {
            if (!System.IO.File.Exists(path)) return;

            try
            {
                string Json = System.IO.File.ReadAllText(path);
                var note = JsonSerializer.Deserialize<Dictionary<string, string>>(Json);

                if (note != null)
                {
                    TitleEditorBox.Text = note["title"];
                    EditorTextBox.Text = note["content"];
                    CurrentPath = path;
                    TitleBoxIsPlaceholder = false;
                    TitleEditorBox.Foreground = new SolidColorBrush(Avalonia.Media.Color.FromArgb(255, 212, 212, 212));
                }
            } finally { }
        }
        private void LoadNote(string path)
        {
            if (!System.IO.File.Exists(path)) return;

            try
            {
                string json = System.IO.File.ReadAllText(path);
                var note = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                if (note != null)
                {
                    TitleEditorBox.Text = note["title"];
                    EditorTextBox.Text = note["content"];
                    CurrentPath = path;

                    TitleBoxIsPlaceholder = false;
                    TitleEditorBox.Foreground =
                        new SolidColorBrush(Color.FromArgb(255, 212, 212, 212));
                }
            }
            catch
            {
                TitleEditorBox.Clear();
                EditorTextBox.Clear();
                CurrentPath = null;
                TitleBoxIsPlaceholder = false;
            }
        }

        private void PreLoadNotes()
        {
            if (!Directory.Exists(DataPath))
                Directory.CreateDirectory(DataPath);

            LoadPagesList();

            var files = Directory.GetFiles(DataPath, "*.json");
            if (files.Length > 0)
            {
                CurrentPath = files[0];
                LoadNote(CurrentPath);
            }
        }


        private void SaveNote(string title, string content, string path)
        {
            if (Saving) return;

            bool isNewNoteCreation = title == "New Note" && string.IsNullOrEmpty(content);

            if (!isNewNoteCreation && string.IsNullOrWhiteSpace(title)) return;

            Saving = true;

            try
            {
                if (!Directory.Exists(DataPath))
                {
                    Directory.CreateDirectory(DataPath);
                }

                var Note = new
                {
                    title = title,
                    content = content ?? ""
                };

                string FilePath;

                if (!string.IsNullOrEmpty(path) && (System.IO.File.Exists(path) || path == CurrentPath))
                {
                    FilePath = path;
                }

                else
                {
                    int fileNumber = 1;
                    do
                    {
                        FilePath = Path.Combine(DataPath, $"{fileNumber}.json");
                        fileNumber++;
                    } while (System.IO.File.Exists(FilePath));

                    CurrentPath = FilePath;
                }

                System.IO.File.WriteAllText(FilePath, JsonSerializer.Serialize(Note, new JsonSerializerOptions { WriteIndented = true }));
            }

            catch (Exception ex)
            {
                var dialog = new Window
                {
                    Width = 300,
                    Height = 100,
                    Content = new TextBlock
                    {
                        Text = "My message here",
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                    }
                };
                dialog.ShowDialog(this);
            }

            finally
            {
                Saving = false;
            }
        }

        private void quit(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(CurrentPath) && !TitleBoxIsPlaceholder)
            {
                SaveNote(
                    TitleEditorBox.Text ?? "",
                    EditorTextBox.Text ?? "",
                    CurrentPath
                );
            }
            this.Close();
        }
        private void HeaderPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                this.BeginMoveDrag(e);
        }
        private void maximize(object? sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }

        }
        private void minimize(object? sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void github(object? sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/bestyeti788",
                UseShellExecute = true
            });
        }



        private void add_page(object sender, RoutedEventArgs e)
        {
            string title = MonTitleBox.Text?.Trim();
            if (string.IsNullOrEmpty(title))
                title = "New Note";

            if (!string.IsNullOrEmpty(CurrentPath) && !TitleBoxIsPlaceholder)
            {
                SaveNote(
                    TitleEditorBox.Text ?? "",
                    EditorTextBox.Text ?? "",
                    CurrentPath
                );
            }

            int fileNumber = 1;
            string newNotePath;
            do
            {
                newNotePath = Path.Combine(DataPath, $"{fileNumber}.json");
                fileNumber++;
            } while (System.IO.File.Exists(newNotePath));

            CurrentPath = newNotePath;

            SaveNote(title, "", CurrentPath);

            TitleEditorBox.Text = title;
            EditorTextBox.Clear();
            TitleBoxIsPlaceholder = false;

            AddPageButton(CurrentPath, title);

            MonTitleBox.Clear();
        }

        private void DeleteCurrentNote(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentPath) || !System.IO.File.Exists(CurrentPath))
                return;

            try
            {
                System.IO.File.Delete(CurrentPath);

                Button? btnToRemove = null;

                foreach (var child in PagesPanel.Children)
                {
                    if (child is Button btn && btn.Content?.ToString() == TitleEditorBox.Text)
                    {
                        btnToRemove = btn;
                        break;
                    }
                }

                if (btnToRemove != null)
                    PagesPanel.Children.Remove(btnToRemove);

                CurrentPath = null;
                TitleEditorBox.Text = "";
                EditorTextBox.Clear();
                TitleBoxIsPlaceholder = true;

                var files = Directory.GetFiles(DataPath, "*.json");
                if (files.Length > 0)
                {
                    CurrentPath = files[0];
                    LoadNote(CurrentPath);
                }
            }
            catch (Exception ex)
            {
                var dialog = new Window
                {
                    Width = 300,
                    Height = 100,
                    Content = new TextBlock
                    {
                        Text = ex.Message,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                    }
                };
                dialog.ShowDialog(this);
            }
        }


    }
}