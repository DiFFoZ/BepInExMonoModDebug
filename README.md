# BepInExMonoModDebug
 
Makes exception easier to debug

## What it does
By installing this mod, your exception will now show IL offset, even if some mod patched the method.

Before:
```js
[Error  : Unity Log] IndexOutOfRangeException: Index was outside the bounds of the array.
Stack trace:
(wrapper dynamic-method) HUDManager.DMD<HUDManager::AddPlayerChatMessageClientRpc>(HUDManager,string,int)
...
```
After:
```js
[Error  : Unity Log] IndexOutOfRangeException: Index was outside the bounds of the array.
Stack trace:
HUDManager.AddPlayerChatMessageClientRpc (System.String chatMessage, System.Int32 playerId) (at <af9b1eec498a45aebd42601d6ab85015>:IL_012E)
...
```

### Caution
Currently some mods may to break if they using Transpiler wrong, e.g. returning invalid instructions.

## For developers
For debugging further, you will need to dump all patches by enabling `[Dumps] Save` in `BepInExMonoModDebugPatcher.cfg`. After running, the directory `BepInEx/dumps` will contain all patched assemblies.

## Links
https://thunderstore.io/c/lethal-company/p/DiFFoZ/BepInEx_MonoMod_Debug_Patcher/
