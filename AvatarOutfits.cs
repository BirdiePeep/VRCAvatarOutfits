#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;

namespace VRCAvatarOutfits
{
    [ExecuteAlways]
    public class AvatarOutfits : MonoBehaviour
    {
        public VRCAvatarDescriptor avatar;
        public GameObject basePrefab;
        public List<GameObject> prefabs = new List<GameObject>();
        public bool overwriteObjects = true;

        //Metadata
        List<GameObject> newObjects = new List<GameObject>();

        public void AttachOutfit()
        {
            //Attach new objects
            foreach (var obj in prefabs)
            {
                if (obj == null)
                    continue;
                AttachPrefab(obj);
            }

            //Dirty
            EditorUtility.SetDirty(avatar);
        }
        void Awake()
        {
            if (GetComponent<VRCAvatarDescriptor>() != null)
            {
                EditorUtility.DisplayDialog("Error", "You are unable to add this script directly to an avatar. Please place this on a blank game object in the scene.", "Okay");
                GameObject.DestroyImmediate(this);
            }
        }
        void AttachPrefab(GameObject prefabObj)
        {
            //Create instance
            var instance = GameObject.Instantiate(prefabObj);
            newObjects.Clear();

            //Attach each
            var children = new GameObject[instance.transform.childCount];
            for (int i = 0; i < instance.transform.childCount; i++)
                children[i] = instance.transform.GetChild(i).gameObject;
            foreach (var child in children)
            {
                AttachPrefab("", child);
            }

            //Attach bones
            foreach (var obj in newObjects)
            {
                var skinned = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (var renderer in skinned)
                    AttachBones(avatar.transform, renderer);
            }

            //Process
            foreach (var obj in newObjects)
            {
                var processes = obj.GetComponentsInChildren<IOutfitProcess>(true);
                foreach (var process in processes)
                {
                    process.ProcessOutfit();
                    GameObject.DestroyImmediate((MonoBehaviour)process);
                }
            }

            //Destroy Instance garbage
            GameObject.DestroyImmediate(instance);
        }
        void AttachPrefab(string path, GameObject instance)
        {
            //Path
            var parentPath = path;
            if (string.IsNullOrEmpty(path))
                path = instance.name;
            else
                path += "/" + instance.name;

            //Does this exist on the base prefab
            var existing = basePrefab.transform.Find(path);
            if (existing != null)
            {
                //Continue search
                var children = new GameObject[instance.transform.childCount];
                for (int i = 0; i < instance.transform.childCount; i++)
                    children[i] = instance.transform.GetChild(i).gameObject;
                foreach (var child in children)
                {
                    AttachPrefab(path, child);
                }
            }
            else
            {
                //Does this already exist on the current model
                existing = avatar.transform.Find(path);
                if (existing != null)
                {
                    if (overwriteObjects)
                        GameObject.DestroyImmediate(existing.gameObject);
                    else
                        return;
                }

                //Instantiate
                //var instance = Instantiate(prefabObj);
                //instance.name = prefabObj.name;
                //newObjects.Add(instance);

                //Add
                GameObject parent = string.IsNullOrEmpty(parentPath) ? avatar.gameObject : avatar.transform.Find(parentPath)?.gameObject;
                if (parent != null)
                {
                    instance.transform.SetParent(parent.transform, false);
                    newObjects.Add(instance);
                }
            }
        }
        void AttachBones(Transform armature, SkinnedMeshRenderer dest)
        {
            //Root
            if (dest.rootBone != null)
                dest.rootBone = FindRecursive(armature, dest.rootBone.name);

            //Find bones
            var bones = (Transform[])dest.bones.Clone();
            for (int i = 0; i < dest.bones.Length; i++)
            {
                var boneName = bones[i].name;
                var sourceBone = FindRecursive(armature, boneName);
                if (sourceBone != null)
                    bones[i] = sourceBone;
                else
                    Debug.LogError($"Unable to find matching bone '{boneName}'");
            }
            dest.bones = bones;
        }

        public void UnattachOutfit()
        {
            if (EditorUtility.DisplayDialog("Unattach Outfit?", "This will delete all outfit items currently attached.  Are you sure you want to unattach the outfit?", "Yes", "No"))
            {
                foreach(var prefab in prefabs)
                {
                    if (prefab != null)
                        UnattachPrefab(prefab);
                }
            }
        }
        public void UnattachPrefab(GameObject prefabObj)
        {
            //Attach each
            foreach (Transform child in prefabObj.transform)
            {
                UnattachPrefab("", child.gameObject);
            }
        }
        public void UnattachPrefab(string path, GameObject prefabObj)
        {
            //Path
            var parentPath = path;
            if (string.IsNullOrEmpty(path))
                path = prefabObj.name;
            else
                path += "/" + prefabObj.name;

            //Does this exist on the base prefab
            var existing = basePrefab.transform.Find(path);
            if (existing != null)
            {
                //Continue search
                foreach (Transform child in prefabObj.transform)
                {
                    UnattachPrefab(path, child.gameObject);
                }
            }
            else
            {
                //Does this already exist on the current model
                existing = avatar.transform.Find(path);
                if (existing != null)
                {
                    //Destroy
                    GameObject.DestroyImmediate(existing.gameObject);
                }
            }
        }

        Transform FindRecursive(Transform self, string name)
        {
            //Find
            var result = self.Find(name);
            if (result != null)
                return result;

            //Recusive
            foreach (Transform child in self)
            {
                result = FindRecursive(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }
    }

    [CustomEditor(typeof(AvatarOutfits))]
    public class AvatarOutfitsEditor : Editor
    {
        AvatarOutfits script;
        public override void OnInspectorGUI()
        {
            script = (AvatarOutfits)target;

            EditorGUI.BeginChangeCheck();

            script.avatar = (VRCAvatarDescriptor)EditorGUILayout.ObjectField("Avatar", script.avatar, typeof(VRCAvatarDescriptor), true);
            script.basePrefab = (GameObject)EditorGUILayout.ObjectField("Base Prefab", script.basePrefab, typeof(GameObject), false);

            if (GUILayout.Button("Add"))
            {
                script.prefabs.Add(null);
            }

            for (int i = 0; i < script.prefabs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                script.prefabs[i] = (GameObject)EditorGUILayout.ObjectField(script.prefabs[i], typeof(GameObject), false);
                if (GUILayout.Button("X", GUILayout.Width(32)))
                {
                    //Check if we want to delete objects
                    if(script.prefabs[i] != null)
                    {
                        if(EditorUtility.DisplayDialog("Delete Objects?", "Do you also want this outfit's objects to be deleted from the avatar?", "Yes", "No"))
                        {
                            //Unattach
                            script.UnattachPrefab(script.prefabs[i]);
                        }
                    }

                    //Remove
                    script.prefabs.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }

            script.overwriteObjects = EditorGUILayout.Toggle("Overwrite Objects", script.overwriteObjects);

            EditorGUI.BeginDisabledGroup(script.avatar == null || script.basePrefab == null);
            {
                if (GUILayout.Button("Attach Outfit", GUILayout.Height(32)))
                {
                    script.AttachOutfit();
                }
                if (GUILayout.Button("Unattach Outfit", GUILayout.Height(32)))
                {
                    script.UnattachOutfit();
                }
            }
        }

    }
}

#endif