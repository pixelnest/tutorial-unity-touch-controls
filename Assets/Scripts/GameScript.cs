using System.Collections.Generic;
using UnityEngine;

public class GameScript : MonoBehaviour
{
  private Dictionary<int, GameObject> trails = new Dictionary<int, GameObject>();
  private Touch pinchFinger1, pinchFinger2;
  private ParticleSystem vortex;

  void Update()
  {
    // -- Pinch
    // ------------------------------------------------
    // Works only with two fingers
    if (Input.touchCount == 2)
    {
      var finger1 = Input.GetTouch(0);
      var finger2 = Input.GetTouch(1);

      if (finger1.phase == TouchPhase.Began && finger2.phase == TouchPhase.Began)
      {
        this.pinchFinger1 = finger1;
        this.pinchFinger2 = finger2;
      }

      // On move, update
      if (finger1.phase == TouchPhase.Moved || finger2.phase == TouchPhase.Moved)
      {
        float baseDistance = Vector2.Distance(this.pinchFinger1.position, this.pinchFinger2.position);
        float currentDistance = Vector2.Distance(finger1.position, finger2.position);

        // Purcent
        float currentDistancePurcent = currentDistance / baseDistance;

        // Create an effect between the fingers if it doesn't exists
        if (vortex == null)
        {
          Vector3 finger1position = Camera.main.ScreenToWorldPoint(this.pinchFinger1.position);
          Vector3 finger2position = Camera.main.ScreenToWorldPoint(this.pinchFinger2.position);

          // Find the center between the two fingers
          Vector3 vortexPosition = Vector3.Lerp(finger1position, finger2position, 0.5f);
          vortex = SpecialEffectsScript.MakeVortex(vortexPosition);
        }

        // Take the base scale and make it smaller/bigger 
        vortex.transform.localScale = Vector3.one * (currentDistancePurcent * 1.5f);
      }
    }
    else
    {
      // Pinch release ?
      if (vortex != null)
      {
        // Create explosions!!!!!!!!!!!
        for (int i = 0; i < 10; i++)
        {
          var explosion = SpecialEffectsScript.MakeExplosion(vortex.transform.position);
          explosion.transform.localScale = vortex.transform.localScale;
        }

        // Destroy vortex
        Destroy(vortex.gameObject);
      }

      // Look for all fingers
      for (int i = 0; i < Input.touchCount; i++)
      {
        Touch touch = Input.GetTouch(i);

        // -- Tap: quick touch & release
        // ------------------------------------------------
        if (touch.phase == TouchPhase.Ended && touch.tapCount == 1)
        {
          // Touch are screens location. Convert to world
          Vector3 position = Camera.main.ScreenToWorldPoint(touch.position);

          // Effect for feedback
          SpecialEffectsScript.MakeExplosion((position));
        }
        else
        {
          // -- Drag
          // ------------------------------------------------
          if (touch.phase == TouchPhase.Began)
          {
            // Store this new value
            if (trails.ContainsKey(i) == false)
            {
              Vector3 position = Camera.main.ScreenToWorldPoint(touch.position);
              position.z = 0; // Make sure the trail is visible

              GameObject trail = SpecialEffectsScript.MakeTrail(position);

              if (trail != null)
              {
                Debug.Log(trail);
                trails.Add(i, trail);
              }
            }
          }
          else if (touch.phase == TouchPhase.Moved)
          {
            // Move the trail
            if (trails.ContainsKey(i))
            {
              GameObject trail = trails[i];

              Camera.main.ScreenToWorldPoint(touch.position);
              Vector3 position = Camera.main.ScreenToWorldPoint(touch.position);
              position.z = 0; // Make sure the trail is visible

              trail.transform.position = position;
            }
          }
          else if (touch.phase == TouchPhase.Ended)
          {
            // Clear known trails
            if (trails.ContainsKey(i))
            {
              GameObject trail = trails[i];

              // Let the trail fade out
              Destroy(trail, trail.GetComponent<TrailRenderer>().time);
              trails.Remove(i);
            }
          }
        }
      }
    }
  }
}