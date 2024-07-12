using UnityEngine;
using System.IO;
using System;

namespace UnityEditor.UI
{
    public class AtlasSelector : ScriptableWizard
    {
        private string m_Title;
        private string m_AtlasParentFolder;
        private Action<string> m_SelectAtlasCallback;

        private Vector2 scrollPos;

        public static void Show(string atlasFolder, Action<string> callback)
        {
            AtlasSelector selector = ScriptableWizard.DisplayWizard<AtlasSelector>("图集");
            selector.m_AtlasParentFolder = atlasFolder;
            selector.m_SelectAtlasCallback = callback;

            if (!Directory.Exists(atlasFolder))
            {
                Directory.CreateDirectory(atlasFolder);
                AssetDatabase.Refresh();
            }
        }
        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 80f;
            GUILayout.Label("请选择一个图集", "LODLevelNotifyText");
            GUILayout.Space(6f);


            DirectoryInfo directory = new DirectoryInfo(m_AtlasParentFolder);
            if (directory == null) return;

            DirectoryInfo[] atlas_item = directory.GetDirectories();
            if (m_AtlasParentFolder.Length > 0)
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                for (int i = 0, iMax = atlas_item.Length; i < iMax; i++)
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(10);
                        GUILayout.Label(atlas_item[i].Name);
                        if (GUILayout.Button("选择", GUILayout.Width(100)))
                        {
                            m_SelectAtlasCallback?.Invoke(atlas_item[i].Name);
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Button("", GUILayout.Height(1));
                }
                EditorGUILayout.EndScrollView();
            }
        }
    }
}

