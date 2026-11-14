using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AngryBirds.Ui;

namespace AngryBirds.EditorTools.Builder
{
    /// <summary>
    /// Creates the Loader scene: the transition curtain with a status label and a progress
    /// bar, driven by GameSceneManager through LoadingScreenView. Run again after changing
    /// the curtain layout or the view; the result is the same scene.
    /// </summary>
    internal static class LoaderSceneBuilder
    {
        internal const string LoaderSceneName = "Loader";
        internal const string LoaderScenePath = "Assets/AngryBirds/Scenes/" + LoaderSceneName + ".unity";

        [MenuItem("Tools/AngryBirds/4. Build Loader Scene", false, 40)]
        public static void Build()
        {
            CreateScene();
            RegisterInBuildSettings();
            SceneNameGenerator.Generate();

            Debug.Log("[LoaderSceneBuilder] Loader scene rebuilt.");
        }

        /// <summary>Creates and saves the scene only, used by the full content build.</summary>
        internal static void CreateScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var canvasObject = new GameObject("LoaderCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            RectTransform background = UiFactory.CreateStretch("Background", canvas.transform);
            background.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 1f);

            Text statusLabel = UiFactory.CreateCenteredText(
                background, new Vector2(0f, 60f), new Vector2(600f, 60f), "LOADING", 42, Color.white);
            statusLabel.name = "StatusLabel";

            Image barBackground = UiFactory.CreateImage(
                "BarBackground", background, new Vector2(0f, -30f), new Vector2(700f, 26f), new Color(1f, 1f, 1f, 0.15f));
            Image progressFill = UiFactory.CreateImage(
                "ProgressFill", barBackground.rectTransform, Vector2.zero, Vector2.zero, new Color(0.35f, 0.75f, 0.45f, 1f), false);

            // Stretch the fill inside the bar, the fill amount is driven by the view.
            progressFill.rectTransform.anchorMin = Vector2.zero;
            progressFill.rectTransform.anchorMax = Vector2.one;
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillOrigin = (int)Image.OriginHorizontal.Left;

            Text progressLabel = UiFactory.CreateCenteredText(
                background, new Vector2(0f, -85f), new Vector2(200f, 40f), "0%", 28, new Color(1f, 1f, 1f, 0.8f));
            progressLabel.name = "ProgressLabel";

            var view = background.gameObject.AddComponent<LoadingScreenView>();
            var serialized = new SerializedObject(view);
            serialized.FindProperty("progressFill").objectReferenceValue = progressFill;
            serialized.FindProperty("progressLabel").objectReferenceValue = progressLabel;
            serialized.FindProperty("statusLabel").objectReferenceValue = statusLabel;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, LoaderScenePath);
            AssetDatabase.SaveAssets();
        }

        private static void RegisterInBuildSettings()
        {
            // The Boot scene always goes first, the game starts from it. The Loader scene is
            // loaded additively during transitions, it has no build order requirements.
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene("Assets/AngryBirds/Scenes/Boot.unity", true),
                new EditorBuildSettingsScene(LoaderScenePath, true)
            };

            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            {
                bool alreadyAdded = false;

                foreach (EditorBuildSettingsScene scene in scenes)
                {
                    if (scene.path == existing.path)
                    {
                        alreadyAdded = true;
                        break;
                    }
                }

                if (!alreadyAdded)
                {
                    scenes.Add(existing);
                }
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
