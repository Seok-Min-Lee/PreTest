using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "PreTest/Game Config")]
public class GameConfig : ScriptableObject
{
    private const string ResourcePath = "GameConfig";

    private static GameConfig s_Instance;

    public static GameConfig Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = Resources.Load<GameConfig>(ResourcePath);
            }

            return s_Instance;
        }
    }

    [SerializeField] private int _maxReflectionCount = 10;
    [SerializeField] private int _maxMirrorCount = 100;

    public int MaxReflectionCount => _maxReflectionCount;
    public int MaxMirrorCount => _maxMirrorCount;
}
