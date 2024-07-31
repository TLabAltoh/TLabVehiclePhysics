using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TLab.EditorTool
{
#if UNITY_EDITOR
    public class AssetBundleBuilder : EditorWindow
    {
        static string root_path = "AssetBundle";
        static string variant = "assetbundl";

        private VisualElement m_rightPanel;

        private void BuildAssetBundle(string assetBundleName, BuildTarget targetPlatform)
        {
            var outputPath = System.IO.Path.Combine(root_path, targetPlatform.ToString(), assetBundleName);
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            var builder = new AssetBundleBuild();
            builder.assetBundleName = assetBundleName;
            builder.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(builder.assetBundleName);
            builder.assetBundleVariant = variant;

            BuildPipeline.BuildAssetBundles(
                outputPath,
                new AssetBundleBuild[] { builder },
                BuildAssetBundleOptions.ChunkBasedCompression,
                targetPlatform);
        }

        private Button CreateButton(string title, System.Action action, int width, int height)
        {
            var label = new Label(title);
            label.style.unityTextAlign = new StyleEnum<TextAnchor>
            {
                value = TextAnchor.MiddleLeft
            };
            label.style.left = new StyleLength
            {
                value = new Length(10)
            };

            var button = new Button(action);
            button.style.width = width;
            button.style.height = height;
            button.Add(label);

            return button;
        }

        private void OnAssetSelectionChange(IEnumerable<object> selectedItems)
        {
            m_rightPanel.Clear();

            var selectedAsset = selectedItems.First() as string;
            if (selectedAsset == null)
            {
                return;
            }

            var title = new Label(selectedAsset);
            title.style.fontSize = 20;
            m_rightPanel.Add(title);

            var buildfor = new Label("\nBuild for ...");
            m_rightPanel.Add(buildfor);

            int buttonWidth = 200;
            int buttonHeight = 20;

            foreach (BuildTarget target in Enum.GetValues(typeof(BuildTarget)))
            {
                m_rightPanel.Add(CreateButton(
                    target.ToString(),
                    () => {
                        BuildAssetBundle(selectedAsset, target);
                    },
                    buttonWidth, buttonHeight));
            }
        }

        public void CreateGUI()
        {
            var assetBundleNames = AssetDatabase.GetAllAssetBundleNames();

            // Split View
            var splitView = new TwoPaneSplitView(0, 150, TwoPaneSplitViewOrientation.Horizontal);
            rootVisualElement.Add(splitView);

            var leftPane = new ListView();
            splitView.Add(leftPane);

            m_rightPanel = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            splitView.Add(m_rightPanel);

            // Initialize the list view with all sprites' names
            leftPane.makeItem = () => new Label();
            leftPane.bindItem = (item, index) => { (item as Label).text = assetBundleNames[index]; };
            leftPane.itemsSource = assetBundleNames;
            leftPane.selectionChanged += OnAssetSelectionChange;
        }

        [MenuItem("Tools/Build AssetBundle")]
        public static void ShowWindow()
        {
            // Create window
            EditorWindow wnd = EditorWindow.GetWindow(typeof(AssetBundleBuilder));
            wnd.titleContent = new GUIContent("TLabAssetBundleBuilder");

            // Limit size of the window
            wnd.minSize = new Vector2(450, 200);
            wnd.maxSize = new Vector2(1920, 720);
        }
    }
#endif
}
