using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_TitleScene : UI_Scene
{
    enum GameObjects
    {
        TouchArea
    }

    enum Texts
    {
        Text
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));

        GetObject((int)GameObjects.TouchArea).BindEvent
            (
            (UIEvent.Click, (evt) => { Debug.Log("ChangeScene"); Managers.Scene.LoadScene(EScene.GameScene); })
            );

        GetObject((int)GameObjects.TouchArea).gameObject.SetActive(false);
        GetText((int)Texts.Text).text = $"";

        StartLoadAssets();

        return true;
    }

    void StartLoadAssets()
    {
        Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, count, totalCount) =>
        {
            Debug.Log($"{key} {count}/{totalCount}");

            if (count == totalCount)
            {
                Managers.Data.Init();

                // 데이터 있는지 확인
                if (Managers.Game.LoadGame() == false)
                {
                    Managers.Game.InitGame();
                    Managers.Game.SaveGame();
                }

                GetObject((int)GameObjects.TouchArea).gameObject.SetActive(true);
                GetText((int)Texts.Text).text = "Click To Start";
            }
        });
    }
}
