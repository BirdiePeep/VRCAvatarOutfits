#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VRCAvatarOutfits
{
    public class IgnoreDynamicBones : MonoBehaviour, IOutfitProcess
    {
        public void ProcessOutfit()
        {
            //Search up the chain for dynamic bones
            Search(this.gameObject);
        }

        void Search(GameObject obj)
        {
            //Search for dynamic bones
            var comps = obj.GetComponents<DynamicBone>();
            foreach(var comp in comps)
            {
                //Does this affect us?
                if(IsParent(comp.m_Root))
                {
                    //Ignore
                    AddIgnore(comp);
                }
            }

            //Move up
            if (obj.transform.parent != null)
                Search(obj.transform.parent.gameObject);
        }
        bool IsParent(Transform parent)
        {
            Transform transform = this.transform;
            while(transform != null)
            {
                if (transform == parent)
                    return true;
                transform = transform.parent;
            }
            return false;
        }
        void AddIgnore(DynamicBone dynamicBone)
        {
            //Add if not already
            if(!dynamicBone.m_Exclusions.Contains(this.transform))
                dynamicBone.m_Exclusions.Add(this.transform);

            //Cleanup nulls
            for (int i = 0; i < dynamicBone.m_Exclusions.Count; i++)
            {
                if (dynamicBone.m_Exclusions[i] == null)
                {
                    dynamicBone.m_Exclusions.RemoveAt(i);
                    i--;
                }
            }

            //Mark dirty
            EditorUtility.SetDirty(dynamicBone);
        }
    }
}

#endif