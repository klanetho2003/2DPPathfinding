using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class ObjectManager
{
    public BaseController TempObject { get; private set; }
    public HashSet<BaseController> TempObejects { get; } = new HashSet<BaseController>();

    public GameObject SpawnGameObject(Vector3 position, string prefabName)
    {
        GameObject go = Managers.Resource.Instantiate(prefabName, pooling: true);
        go.transform.position = position;
        return go;
    }

    public T Spawn<T>(Vector3 position, int templateID = 0) where T : BaseController
    {
        string prefabName = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate(prefabName, pooling: true);
        go.name = prefabName;
        go.transform.position = position;

        BaseController obj = go.GetComponent<BaseController>();

        if (obj.ObjectType == EObjectType.None)
        {
            BaseController tempObject = go.GetComponent<BaseController>();
            TempObject = tempObject;
            // TempObject.SetInfo(templateID);
        }
        else if (obj.ObjectType == EObjectType.TempType)
        {
            BaseController temp = go.GetComponent<BaseController>();
            TempObejects.Add(temp);
            // temp.SetInfo(templateID);
        }

        return obj as T;
    }

    public void Despawn<T>(T obj) where T : BaseController
    {
        GameObject objGameObject = obj.gameObject;
        if (objGameObject.IsValid() == false)
            return;

        EObjectType objectType = obj.ObjectType;

        if (obj.ObjectType == EObjectType.None)
        {
            BaseController player = obj.GetComponent<BaseController>();
            TempObject = null;
        }
        else if (obj.ObjectType == EObjectType.TempType)
        {
            BaseController temp = obj.GetComponent<BaseController>();
            TempObejects.Remove(temp);
        }

        // To Pool
        {
            string poolName = typeof(T).Name;
            obj.gameObject.name = poolName;
        }

        Managers.Resource.Destroy(obj.gameObject);

    }

    public void DespawnAllTemps()
    {
        var temps = TempObejects.ToList();

        foreach (var temp in temps)
            Managers.Object.Despawn(temp);
    }
}
