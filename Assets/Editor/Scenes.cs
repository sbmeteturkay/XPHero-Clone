using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class Scenes : EditorWindow
{
    [MenuItem("Window/Scenes...", false)]
	public static void ShowWindow ()
	{
        EditorWindow.GetWindow (typeof(Scenes));
	}

    FileInfo[] _files;
    Dictionary<string, bool> _folds;
    Vector2 _scrollPosition;

    public Scenes()
    {
        _scrollPosition = new Vector2();
        _folds = new Dictionary<string, bool>();
        GetScenes();
    }

    void OnProjectChange()
    {
        GetScenes();
        Repaint();
    }

    void OnUpdate()
    {
        GetScenes();
    }

	void OnGUI ()
	{
        EditorGUILayout.Space();
        string d = "";
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.MaxWidth(170), GUILayout.ExpandHeight(true));
        for (int i = 0; i < _files.Length; i++)
        {
            FileInfo f = _files[i];

            if (d != f.DirectoryName)
            {
                d = f.DirectoryName;
                var label = f.DirectoryName.Replace(Application.dataPath.Replace("/", "\\") + "\\", "");
                bool show = !_folds.ContainsKey(d) ? false : _folds[d];
                _folds[d] = EditorGUILayout.Foldout(show, label);
            }
            
            if (_folds[d])
            {
                if (GUILayout.Button(f.Name.Replace(".unity", ""), GUILayout.MaxWidth(170), GUILayout.MaxHeight(20)))
                {
                    EditorSceneManager.OpenScene(f.FullName);
                }
            }
        }
        GUILayout.EndScrollView();
	}

    FileInfo[] GetScenes()
    {
        DirectoryInfo directory = new DirectoryInfo(Application.dataPath);
        _files = directory.GetFiles("*.unity", SearchOption.AllDirectories);
        System.Array.Sort(_files, delegate(FileInfo f1, FileInfo f2)
        {
            return f1.DirectoryName.CompareTo(f2.DirectoryName);
        });

        var dict = new Dictionary<string, bool>();
        var d = "";
        for (int i = 0; i < _files.Length; i++)
        {
            FileInfo f = _files[i];
            if (d != f.DirectoryName)
            {
                d = f.DirectoryName;
                var label = f.DirectoryName.Replace(Application.dataPath.Replace("/", "\\") + "\\", "");
                if (_folds.ContainsKey(d))
                {
                    dict[d] = _folds[d];
                }
            }
        }
        _folds = dict;
        return _files;
    }
}

