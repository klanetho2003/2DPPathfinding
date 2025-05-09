using UnityEngine;

public class Managers : MonoBehaviour
{
    public static bool Initialized { get; private set; } = false;

    private static Managers s_instance;
    private static Managers Instance { get { Init(); return s_instance; } }

    #region Contents
    GameManager _game = new GameManager();
    MapManager _map = new MapManager();
    ObjectManager _obj = new ObjectManager();

    public static GameManager Game { get { return Instance?._game; } }
    public static MapManager Map { get { return Instance?._map; } }
    public static ObjectManager Object { get { return Instance?._obj; } }
    #endregion

    #region Cores
    DataManager _data = new DataManager();
    InputManager _input = new InputManager();
    PoolManager _pool = new PoolManager();
    ResourceManager _resource = new ResourceManager();
    SceneManagerEx _scene = new SceneManagerEx();
    UIManager _ui = new UIManager();
    
    public static DataManager Data { get { return Instance?._data; } }
    public static InputManager Input { get { return Instance?._input; } }
    public static PoolManager Pool { get { return Instance?._pool; } }
    public static ResourceManager Resource { get { return Instance?._resource; } }
    public static SceneManagerEx Scene { get { return Instance?._scene; } }
    public static UIManager UI { get { return Instance?._ui; } }
    #endregion

    private void Start()
    {
        Init();
    }

    static void Init()
    {
        if (s_instance == null && Initialized == false)
        {
            Initialized = true;

            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);

            // 초기화
            s_instance = go.GetComponent<Managers>();

            s_instance._input.Init();
        }
    }

    private void Update()
    {
        s_instance._input.OnUpdate();
    }
}
