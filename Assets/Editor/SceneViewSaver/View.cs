using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Nash1m.EditorSceneViewSaver
{
    [System.Serializable]
    public class View
    {
        #region Delegates
        public delegate void RemoveCallback(View view);
        public RemoveCallback OnDelete;

        public delegate void SetCallback(View view);
        public SetCallback OnSet;

        public delegate void OverrideCallback(View view);
        public OverrideCallback OnOverride;

        public delegate void EditedCallback(View view);
        public EditedCallback OnEdited;

        public delegate void ScreenshotCallback(View view);
        public ScreenshotCallback OnScreenshot;
        #endregion

        private bool isEditMode;
        private bool isExpanded;

        public string sceneName;
        public string title;
        public float cameraDistance;
        public Vector3 pivotPosition;
        public Quaternion rotation;
        public float size;

        public View(string titleArg, Vector3 pivotPositionArg, Quaternion rotationArg, float sizeArg,
            float cameraDistanceArg)
        {
            sceneName = EditorSceneManager.GetActiveScene().name;
            title = titleArg;
            pivotPosition = pivotPositionArg;
            rotation = rotationArg;
            size = sizeArg;
            cameraDistance = cameraDistanceArg;
        }

        public void Draw(Rect parentRect)
        {
            GUILayout.BeginVertical("box", GUILayout.Width(parentRect.width - 60));
            if (isExpanded)
                DrawExpandedView(parentRect);
            else
                DrawMinimizeView(parentRect);
            GUILayout.EndVertical();
        }

        private void DrawExpandedView(Rect parentRect)
        {
            GUILayout.BeginHorizontal();
            var icon = isExpanded ? "d_icon dropdown" : "d_forward";
            if (GUILayout.Button(EditorGUIUtility.IconContent(icon), GUIStyle.none, GUILayout.Width(20)))
                isExpanded = !isExpanded;

            if (!isEditMode)
                GUILayout.Label(title, GUILayout.Width(parentRect.width - 84));
            else
                title = GUILayout.TextField(title, GUILayout.Width(parentRect.width - 84));

            if (GUILayout.Button(isEditMode
                ? EditorGUIUtility.IconContent("d_SaveAs")
                : EditorGUIUtility.IconContent("d_editicon.sml")))
            {
                isEditMode = !isEditMode;
                if (!isEditMode)
                    OnEdited?.Invoke(this);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(EditorGUIUtility.IconContent("animationvisibilitytoggleon@2x")))
                OnSet?.Invoke(this);

            #region Screenshot, Override & Delete

            GUILayout.BeginVertical(GUILayout.Width(50));

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_FrameCapture")))
                OnScreenshot?.Invoke(this);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_RotateTool")))
                OnOverride?.Invoke(this);
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash")))
                OnDelete?.Invoke(this);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            #endregion

            GUILayout.EndHorizontal();
        }
        private void DrawMinimizeView(Rect parentRect)
        {
            GUILayout.BeginHorizontal();
            var icon = isExpanded ? "d_icon dropdown" : "d_forward";
            if (GUILayout.Button(EditorGUIUtility.IconContent(icon), GUIStyle.none, GUILayout.Width(20)))
                isExpanded = !isExpanded;

            if (!isEditMode)
                GUILayout.Label(title, GUILayout.Width(parentRect.width - 114));
            else
                title = GUILayout.TextField(title, GUILayout.Width(parentRect.width - 114));

            if (GUILayout.Button(isEditMode
                ? EditorGUIUtility.IconContent("d_SaveAs")
                : EditorGUIUtility.IconContent("d_editicon.sml")))
            {
                isEditMode = !isEditMode;
                if (!isEditMode)
                    OnEdited?.Invoke(this);
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("animationvisibilitytoggleon")))
                OnSet?.Invoke(this);

            GUILayout.EndHorizontal();
        }
    }
}