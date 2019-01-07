using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDrag : MonoBehaviour {

    void OnMouseDrag() {
        Camera camera = GameObject.Find("Main Camera").GetComponent<Camera>(); 
        float distance_to_screen = camera.WorldToScreenPoint(gameObject.transform.position).z; 
        Vector3 pos_move = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen)); 
        transform.position = new Vector3(pos_move.x, pos_move.y, pos_move.z);
    }
}
