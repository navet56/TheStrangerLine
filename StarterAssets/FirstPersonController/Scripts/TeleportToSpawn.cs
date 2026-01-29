using UnityEngine;

public class TeleportToSpawn : MonoBehaviour
{
    public Transform spawn;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("allo");
        if (other.gameObject.name == "Sophie")
        {
            Debug.Log("allo 2 connard");

            var cc = other.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                other.transform.position = spawn.position;
                cc.enabled = true;
            }
            else
            {
                other.transform.position = spawn.position;
            }
        }
    }
}