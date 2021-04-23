using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CoinTask : MonoBehaviour
{
    public VirtualEnvironment virtualEnvironment;
    public GameObject coinPrefab;
    public float height;

    private bool isEnd = false;

    public AudioClip clip;

    private UnityEvent onCollect = new UnityEvent();

    private GameObject coin;

    public void Destroy()
    {
        if (coin != null)
        {
            AudioSource.PlayClipAtPoint(clip, virtualEnvironment.userBody.transform.position);
            coin.SetActive(false);
        }
    }

    public void Generate()
    {
        if (!isEnd)
        {
            UserBody userBody = virtualEnvironment.userBody;
            Room currentRoom = virtualEnvironment.CurrentRoom;

            if (coin == null) coin = GameObject.Instantiate(coinPrefab, virtualEnvironment.transform);
            else coin.SetActive(true);

            Vector2 coinPos;

            do
            {
                coinPos = currentRoom.SamplingPosition(0.25f);
            } while (Vector2.Distance(userBody.Position, coinPos) < 0.8f);

            coin.transform.position = Utility.CastVector2Dto3D(coinPos, height);
        }
    }

    public void SetTaskEnd()
    {
        isEnd = true;
    }

    public void SetTaskStart()
    {
        isEnd = false;
        Generate();
    }

    // private void Start() {
    //     Generate();
    // }

    //private void OnTriggerEnter(Collider other)
    //{
    //    User user = other.GetComponent<User>();
    //    if (user != null)
    //    {
    //        Debug.Log("Coin is collected by user");
    //        Generate();
    //    }
    //}

    // public void Reposition() {
    //     User user =  virtualEnvironment.GetUser();
    //     Room currentRoom =  virtualEnvironment.GetCurrentRoom();

    //     Vector2 coinPos;

    //     do
    //     {
    //         coinPos = currentRoom.SamplingInnerPosition(0.2f);
    //     } while (Vector2.Distance(user.Position, coinPos) < 0.8f);

    //     coin.transform.position = Utility.CastVector2Dto3D(coinPos, 0.9f);
    // }
}
