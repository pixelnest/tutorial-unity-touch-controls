using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameScript : MonoBehaviour
{
  // Prefabs
  public ParticleSystem explosionEffect, vortexEffect;
  public GameObject trailPrefab;

  private Dictionary<int, GameObject> trails = new Dictionary<int, GameObject>();

  private Touch pinchFinger1, pinchFinger2;
  private ParticleSystem vortex;

  void Start()
  {
    // Check prefabs
    if (explosionEffect == null)
      Debug.LogError("Missing Explosion Effect!");
    if (vortexEffect == null)
      Debug.LogError("Missing Vortex Effect!");
    if (trailPrefab == null)
      Debug.LogError("Missing Trail Prefab!");
  }

  void Update()
  {
    HandleTap();
    HandleDrag();
    HandlePinch();
  }

  private void HandleTap()
  {
    //-- Tap: quick touch & release
    for (int i = 0; i < Input.touchCount; i++)
    {
      Touch touch = Input.GetTouch(i);
      if (touch.phase == TouchPhase.Ended && touch.tapCount == 1)
      {
        MakeExplosion(ToWorld(touch.position));
      }
    }
  }

  private void HandleDrag()
  {
    for (int i = 0; i < Input.touchCount; i++)
    {
      Touch touch = Input.GetTouch(i);
      // -- Drag
      if (touch.phase == TouchPhase.Began)
      {
        // Store this new value
        if (trails.ContainsKey(i) == false)
        {
          GameObject trail = MakeTrail(ToWorld(touch.position));
          trails.Add(i, trail);
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
      else if (touch.phase == TouchPhase.Moved)
      {
        // Move the trail
        if (trails.ContainsKey(i))
        {
          GameObject trail = trails[i];
          trail.transform.position = ToWorld(touch.position);
        }
      }
    }
  }

  private void HandlePinch()
  {
    // -- Pinch
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
          vortex = MakeVortex(Vector3.Lerp(ToWorld(this.pinchFinger1.position), ToWorld(this.pinchFinger2.position), 0.5f));
        }

        // Take the base scale and make it smaller/bigger 
        vortex.transform.localScale = Vector3.one * (currentDistancePurcent * 1.5f);
      }

      // At least one finger is not there anymore
      if (finger1.phase == TouchPhase.Ended || finger2.phase == TouchPhase.Ended)
      {
        // Create explosions!!!!!!!!!!!
        for (int i = 0; i < 10; i++)
        {
          var explosion = MakeExplosion(vortex.transform.position);
          explosion.transform.localScale = vortex.transform.localScale;
        }

        // Destroy vortex
        Destroy(vortex.gameObject);
      }
    }
  }


  /// <summary>
  /// Transform a screen location to a world one
  /// </summary>
  /// <returns>The world.</returns>
  /// <param name="screenPosition">Screen position.</param>
  public Vector3 ToWorld(Vector3 screenPosition)
  {
    Vector3 p = Camera.main.ScreenToWorldPoint(screenPosition);
    p.z = 0;

    return p;
  }

  /// <summary>
  /// Create an explosion at the given position
  /// </summary>
  /// <param name="position"></param>
  public ParticleSystem MakeExplosion(Vector3 position)
  {
    ParticleSystem effect = Instantiate(explosionEffect) as ParticleSystem;
    effect.transform.position = position;

    // Program destruction at the end of the effect
    Destroy(effect.gameObject, effect.duration);

    return effect;
  }

  /// <summary>
  /// Create a particle vortex at the given position
  /// </summary>
  /// <param name="position"></param>
  public ParticleSystem MakeVortex(Vector3 position)
  {
    ParticleSystem effect = Instantiate(vortexEffect) as ParticleSystem;
    effect.transform.position = position;

    return effect;
  }

  /// <summary>
  /// Create a new trail at the given position
  /// </summary>
  /// <param name="position"></param>
  /// <returns></returns>
  public GameObject MakeTrail(Vector3 position)
  {
    GameObject trail = Instantiate(trailPrefab) as GameObject;
    trail.transform.position = position;

    return trail;
  }
}
