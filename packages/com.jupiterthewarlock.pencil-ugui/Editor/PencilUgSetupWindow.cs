using System.IO;
using UnityEditor;
using UnityEngine;

namespace OpenPencilUGUI
{
    public class PencilUgSetupWindow : EditorWindow
    {
        static readonly Color ServerRunningColor = new Color(0.35f, 0.75f, 0.45f);
        static readonly Color ServerStoppedColor = new Color(0.85f, 0.35f, 0.35f);

        PencilUgConfig config;
        Vector2 scrollPosition;
        string statusMessage = "";
        MessageType statusType = MessageType.Info;
        bool showConfigurationDetails;
        bool installCursorSkill = true;
        bool installCodexSkill;
        bool installClaudeSkill;
        bool installQoderSkill;
        bool installCustomSkill;
        bool stylesInitialized;

        GUIStyle serverButtonStyle;
        GUIStyle readOnlyLabelStyle;

        [MenuItem("Tools/Pencil UGUI/Setup...")]
        public static void ShowWindow()
        {
            var window = GetWindow<PencilUgSetupWindow>("Pencil UGUI Setup");
            window.minSize = new Vector2(520f, 460f);
            window.Show();
        }

        void OnEnable()
        {
            config = PencilUgConfigStore.LoadOrCreateDefault();
            installCursorSkill = ContainsSkillTarget("cursor");
            installCodexSkill = ContainsSkillTarget("codex");
            installClaudeSkill = ContainsSkillTarget("claude");
            installQoderSkill = ContainsSkillTarget("qoder");
            installCustomSkill = ContainsSkillTarget("custom");
        }

        void InitStyles()
        {
            if (stylesInitialized)
            {
                return;
            }

            serverButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold
            };
            readOnlyLabelStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = false
            };
            stylesInitialized = true;
        }

        void OnGUI()
        {
            InitStyles();

            if (config == null)
            {
                config = PencilUgConfigStore.LoadOrCreateDefault();
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Harness Status", EditorStyles.boldLabel);
            config.serverPort = EditorGUILayout.IntField("Server Port", config.serverPort);
            DrawEditableFolderField("OpenPencil Dir", ref config.openPencilDir, true);
            EditorGUILayout.HelpBox(
                "Path to your open-pencil/open-pencil checkout. It is usually outside the Unity project. Relative paths resolve from the Unity project root.",
                MessageType.None);
            DrawReadOnlyRow("Config", PencilUgHarnessPaths.ConfigRelativePath);
            DrawReadOnlyRow("Local Server", PencilUgLocalServer.IsRunning
                ? $"Running on port {PencilUgLocalServer.Port}"
                : "Stopped");
            DrawReadOnlyRow("Installed Skills", DescribeInstalledSkills());

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save Config"))
                {
                    SaveConfig();
                }

                DrawServerButton();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Install / Update Skill"))
                {
                    InstallSkill();
                }

                if (GUILayout.Button("Run Doctor"))
                {
                    RunDoctor();
                }
            }

            EditorGUILayout.Space(8f);
            showConfigurationDetails = EditorGUILayout.Foldout(showConfigurationDetails, "Configuration Details", true);
            if (showConfigurationDetails)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawReadOnlyRow("Exporter Command", config.exporterCommand);
                    DrawReadOnlyRow("Generated Dir", config.generatedDir);
                    DrawReadOnlyRow("Default Target Mode", config.defaultTargetMode);
                    DrawReadOnlyRow("Provider", config.provider);
                }
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Agent Skill Targets", EditorStyles.boldLabel);
            installCursorSkill = EditorGUILayout.ToggleLeft("Cursor (.cursor/skills)", installCursorSkill);
            installCodexSkill = EditorGUILayout.ToggleLeft("Codex (.codex/skills)", installCodexSkill);
            installClaudeSkill = EditorGUILayout.ToggleLeft("Claude (.claude/skills)", installClaudeSkill);
            installQoderSkill = EditorGUILayout.ToggleLeft("Qoder (.qoder/skills)", installQoderSkill);
            installCustomSkill = EditorGUILayout.ToggleLeft("Custom folder", installCustomSkill);
            if (installCustomSkill)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawEditableFolderField("Custom Skill Dir", ref config.customSkillDir, true);
                }
            }

            if (!string.IsNullOrEmpty(statusMessage))
            {
                EditorGUILayout.Space(8f);
                EditorGUILayout.HelpBox(statusMessage, statusType);
            }

            EditorGUILayout.EndScrollView();
        }

        void DrawReadOnlyRow(string label, string value)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(120f));
                EditorGUILayout.LabelField(value ?? string.Empty, readOnlyLabelStyle);
            }
        }

        void DrawEditableFolderField(string label, ref string pathValue, bool allowOutsideProject = false)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                pathValue = EditorGUILayout.TextField(label, pathValue);
                if (GUILayout.Button("...", GUILayout.Width(28f)))
                {
                    var currentPath = PencilUgHarnessPaths.ResolveProjectPath(pathValue);
                    var picked = EditorUtility.OpenFolderPanel($"Select {label}", currentPath, string.Empty);
                    if (!string.IsNullOrEmpty(picked))
                    {
                        pathValue = allowOutsideProject
                            ? picked
                            : PencilUgHarnessPaths.ToProjectRelativePath(picked);
                        if (string.IsNullOrWhiteSpace(pathValue))
                        {
                            pathValue = picked;
                        }
                    }
                }
            }
        }

        void DrawServerButton()
        {
            var previousColor = GUI.backgroundColor;
            GUI.backgroundColor = PencilUgLocalServer.IsRunning ? ServerRunningColor : ServerStoppedColor;
            var label = PencilUgLocalServer.IsRunning ? "Stop Server" : "Start Server";
            if (GUILayout.Button(label, serverButtonStyle))
            {
                ToggleServer();
            }

            GUI.backgroundColor = previousColor;
        }

        string DescribeInstalledSkills()
        {
            var targets = BuildSkillTargets();
            var installed = new System.Collections.Generic.List<string>();
            foreach (var target in targets)
            {
                if (PencilUgSkillInstaller.IsSkillInstalled(config, target))
                {
                    installed.Add(target);
                }
            }

            return installed.Count == 0 ? "None" : string.Join(", ", installed);
        }

        void SaveConfig()
        {
            SaveSkillTargetsToConfig();
            PencilUgConfigStore.MigrateAndSave(config);
            statusMessage = $"Saved config to {PencilUgHarnessPaths.ConfigRelativePath}";
            statusType = MessageType.Info;
        }

        void ToggleServer()
        {
            if (PencilUgLocalServer.IsRunning)
            {
                PencilUgLocalServer.Stop();
                statusMessage = "Local server stopped.";
                statusType = MessageType.Info;
                return;
            }

            SaveConfig();
            PencilUgLocalServer.Start(config);
            statusMessage = $"Local server started on port {config.serverPort}.";
            statusType = MessageType.Info;
        }

        void InstallSkill()
        {
            SaveConfig();
            if (installCustomSkill && string.IsNullOrWhiteSpace(config.customSkillDir))
            {
                statusMessage = "Select a custom skill folder before installing.";
                statusType = MessageType.Warning;
                return;
            }

            var result = PencilUgSkillInstaller.Install(config);
            statusMessage = result.ok
                ? $"{result.message} Targets: {string.Join(", ", result.targets)}"
                : result.message;
            statusType = result.ok ? MessageType.Info : MessageType.Error;
        }

        void RunDoctor()
        {
            SaveConfig();
            var report = PencilUgDoctor.Run(config);
            statusMessage = report.Format();
            statusType = report.passed ? MessageType.Info : MessageType.Warning;
        }

        void SaveSkillTargetsToConfig()
        {
            config.skillTargets = BuildSkillTargets();
        }

        string[] BuildSkillTargets()
        {
            var targets = new System.Collections.Generic.List<string>();
            if (installCursorSkill)
            {
                targets.Add("cursor");
            }

            if (installCodexSkill)
            {
                targets.Add("codex");
            }

            if (installClaudeSkill)
            {
                targets.Add("claude");
            }

            if (installQoderSkill)
            {
                targets.Add("qoder");
            }

            if (installCustomSkill)
            {
                targets.Add("custom");
            }

            if (targets.Count == 0)
            {
                targets.Add("cursor");
            }

            return targets.ToArray();
        }

        bool ContainsSkillTarget(string target)
        {
            if (config.skillTargets == null)
            {
                return false;
            }

            foreach (var skillTarget in config.skillTargets)
            {
                if (skillTarget == target)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
