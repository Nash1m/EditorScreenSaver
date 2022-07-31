using System.Collections.Generic;
using System.Linq;
using Nash1m.EditorSave;
using Nash1m.Extensions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Nash1m.EditorSceneViewSaver
{
    public class EditorSceneViewWindow : EditorWindow
    {
        private SaveModule _saveModule;

        private Vector2 scrollPosition;

        [MenuItem("Nash1m/Tools/Scene Views")]
        public static void Init()
        {
            var window = GetWindow<EditorSceneViewWindow>("Scene Views");
            window.minSize = new Vector2(200, 200);
            window.Show();
        }
        private void OnEnable()
        {
            _saveModule = EditorSaveSystem.SaveData.GetModule<SaveModule>();
            if (_saveModule is null)
            {
                _saveModule = EditorSaveSystem.SaveData.AddModule<SaveModule>();
                _saveModule.screenshotsPath = Application.dataPath.Replace("/Assets", "/Screenshots");
            }

            foreach (var view in _saveModule.views)
                InitializeViewCallbacks(view);
        }
        private void OnDisable()
        {
            EditorSaveSystem.Save();
        }
        
        private void OnGUI()
        {
            var activeSceneName = EditorSceneManager.GetActiveScene().name;
            var sceneViews = _saveModule.views.ToArray().Where(x => x.sceneName == activeSceneName);

            GUILayout.Label($"{activeSceneName}", new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                fontSize = 20,
                normal = new GUIStyleState
                {
                    textColor = Color.grey
                },
                alignment = TextAnchor.MiddleCenter
            });

            scrollPosition =
                GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Height(position.height - 50));
            GUILayout.BeginVertical();
            foreach (var view in sceneViews)
                view.Draw(position);
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            if (GUILayout.Button("Add View"))
                AddViewFromCurrentPosition();
        }

        private View GetCurrentView()
        {
            var sceneView = SceneView.lastActiveSceneView;

            var position = sceneView.pivot;
            var rotation = sceneView.rotation;
            var size = sceneView.size;
            var title = $"View {_saveModule.views.Count + 1}";

            return new View(title, position, rotation, size, sceneView.cameraDistance);
        }

        #region View Callbacks
        private void InitializeViewCallbacks(View view)
        {
            view.OnDelete = OnViewDelete;
            view.OnSet = OnViewSet;
            view.OnOverride = OnViewOverride;
            view.OnEdited = OnViewEdited;
            view.OnScreenshot = MakeScreenshot;
        }

        private void OnViewDelete(View view)
        {
            _saveModule.views.Remove(view);
            EditorSaveSystem.Save();
        }
        private void OnViewSet(View view)
        {
            var sceneView = SceneView.lastActiveSceneView;
            sceneView.pivot = view.pivotPosition;
            sceneView.rotation = view.rotation;
            sceneView.size = view.size;
        }
        private void OnViewOverride(View view)
        {
            var tempView = GetCurrentView();
            view.pivotPosition = tempView.pivotPosition;
            view.rotation = tempView.rotation;
            view.size = tempView.size;
            view.cameraDistance = tempView.cameraDistance;
            EditorSaveSystem.Save();
        }
        private void OnViewEdited(View view)
        {
            EditorSaveSystem.Save();
        }
        private void MakeScreenshot(View view)
        {
            if (!System.IO.Directory.Exists(_saveModule.screenshotsPath))
                System.IO.Directory.CreateDirectory(_saveModule.screenshotsPath);

            var sceneView = SceneView.lastActiveSceneView;

            #region Creating screenshot camera
            var camera = new GameObject("Screenshot camera").AddComponent<Camera>();
            camera.transform.rotation = view.rotation;
            camera.transform.position = view.pivotPosition - camera.transform.forward * view.cameraDistance;
            #endregion
            #region File path
            var fileName = $"{view.sceneName}_{view.title}.png";
            var screenshotPath = System.IO.Path.Combine(_saveModule.screenshotsPath, fileName);
            #endregion

            #region Render texture template
            var renderTexture =
                new RenderTexture((int) sceneView.position.width, (int) sceneView.position.height,
                    sceneView.depthBufferBits);
            #endregion

            camera.SaveScreenshot(screenshotPath, EncodeFormat.JPG, renderTexture);

            DestroyImmediate(camera.gameObject);
            DestroyImmediate(renderTexture);
        }
        #endregion
        #region Add Views
        private void AddView(View view)
        {
            _saveModule.views.Add(view);
            EditorSaveSystem.Save();

            InitializeViewCallbacks(view);
        }
        private void AddViewFromCurrentPosition()
        {
            AddView(GetCurrentView());
        }
        #endregion
        #region Save Module
        [System.Serializable]
        public class SaveModule : EditorSaveModule
        {
            public List<View> views = new List<View>();
            public string screenshotsPath;
        }
        #endregion
    }
}