using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public static class InputFocusGuard
{
    public static bool IsInputFieldFocused()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected == null)
        {
            return false;
        }

        return selected.GetComponent<TMP_InputField>() != null;
    }
}
