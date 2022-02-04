# VRCAvatarOutfits
Simple script to help attaching and and managing outfits for vrchat avatars.

USEAGE

- Attach the AvatarOutfits script to a GameObject other then your avatar in the scene.
- Provide some reference to the base avatar prefab.  This tells the script what the avatar looks like without outfits and what not to touch on the avatar in the scene.
- Give reference to the avatar descriptor in the scene as well as any number of outfit prefabs.
- Attach or Remove the outfits with the button provided.

NOTES

The script will look at the hierarchy of each outfit prefab, matching it against the base prefab.  Anything new will be copied over onto the avatar, anything similar will be left alone.

Skinned mesh renderers are automatically updated to use the bones on the scene avatar.  Typically this isn't doable inside of the editor by hand and forces many creators to provide a model file with the avatar and all outfit options.  Separate clothing prefab/files just need to include the armature that it uses so they can be matched up with the base prefab.  The bone names need to match for this to work.  Also no re-weighting is done, this assume the clothing is intended for this specific armature.

Use the IgnoreDynamicBones script on any GameObject inside of an outfit to have it automatically added to a DynamicBone's ignore list.  This only works if the DynamicBone script is a parent to the affected bone.
