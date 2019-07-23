using UnityEditor;

public class LockInspectorShortcut { 
	
	[MenuItem("Edit/Toggle Inspector Lock %l")]
	public static void ToggleLock()
	{
		ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
		ActiveEditorTracker.sharedTracker.ForceRebuild();
	}
}
