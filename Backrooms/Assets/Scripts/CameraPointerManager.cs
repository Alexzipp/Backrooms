using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPointerManager : MonoBehaviour
{
    [SerializeField] private GameObject pointer;
    [SerializeField] private float maxDistancePointer = 4.5f;
    [Range(0,1)]
    [SerializeField] private float disPointerObject = 0.95f;

    [Range(-32767, 32767)]
    public int ReticleSortingOrder = 32767;

    /// <summary>
    /// Mask used to indicate interactive objects.
    /// </summary>
    public LayerMask ReticleInteractionLayerMask = 1 << _RETICLE_INTERACTION_DEFAULT_LAYER;

    /// <summary>
    /// Default layer for interactive game objects.
    /// </summary>
    private const int _RETICLE_INTERACTION_DEFAULT_LAYER = 8;

    /// <summary>
    /// The angle in degrees defined between the 2 vectors that depart from the camera and point to
    /// the extremes of the minimum inner diameter of the reticle.
    ///
    /// Being `z` the distance from the camera to the object and `d_i` the inner diameter of the
    /// reticle, this is 2*arctg(d_i/(2*z)).
    /// </summary>
    private const float _RETICLE_MIN_INNER_ANGLE = 0.0f;

    /// <summary>
    /// The angle in degrees defined between the 2 vectors that depart from the camera and point to
    /// the extremes of the minimum outer diameter of the reticle.
    ///
    /// Being `z` the distance from the camera to the object and `d_o` the outer diameter of the
    /// reticle, this is 2*arctg(d_o/(2*z)).
    /// </summary>
    private const float _RETICLE_MIN_OUTER_ANGLE = 0.5f;

    /// <summary>
    /// Angle at which to expand the reticle when intersecting with an object (in degrees).
    /// </summary>
    private const float _RETICLE_GROWTH_ANGLE = 1.5f;

    /// <summary>
    /// Minimum distance between the camera and the reticle (in meters).
    /// </summary>
    private const float _RETICLE_MIN_DISTANCE = 0.45f;

    /// <summary>
    /// Maximum distance between the camera and the reticle (in meters).
    /// </summary>
    private const float _RETICLE_MAX_DISTANCE = 20.0f;

    /// <summary>
    /// Number of segments making the reticle circle.
    /// </summary>
    private const int _RETICLE_SEGMENTS = 20;

    /// <summary>
    /// Growth speed multiplier for the reticle.
    /// </summary>
    private const float _RETICLE_GROWTH_SPEED = 8.0f;

    /// <summary>
    /// The game object the reticle is pointing at.
    /// </summary>
    private GameObject _gazedAtObject = null;

    /// <summary>
    /// The material used to render the reticle.
    /// </summary>
    private Material _reticleMaterial;

    /// <summary>
    /// The current inner angle of the reticle (in degrees).
    /// </summary>
    private float _reticleInnerAngle;

    /// <summary>
    /// The current outer angle of the reticle (in degrees).
    /// </summary>
    private float _reticleOuterAngle;

    /// <summary>
    /// The current distance of the reticle (in meters).
    /// </summary>
    private float _reticleDistanceInMeters;

    /// <summary>
    /// The current inner diameter of the reticle, before distance multiplication (in meters).
    /// </summary>
    private float _reticleInnerDiameter;

    /// <summary>
    /// The current outer diameter of the reticle, before distance multiplication (in meters).
    /// </summary>
    private float _reticleOuterDiameter;

    private readonly string interactableTag = "Interactable";
    private float scaleSize = 0.025f;
    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    private void Start()
    {
        Renderer rendererComponent = GetComponent<Renderer>();
        rendererComponent.sortingOrder = ReticleSortingOrder;

        _reticleMaterial = rendererComponent.material;

        CreateMesh();

        GazeManager.Instance.OnGazeSelection += GazeSelection;
    }



    private void GazeSelection()
    {
        _gazedAtObject?.SendMessage("OnPointerClick", null, SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    public void Update()
    {
        // Casts ray towards camera's forward direction, to detect if a GameObject is being gazed
        // at.
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, _RETICLE_MAX_DISTANCE))
        {
            // GameObject detected in front of the camera.
            if (_gazedAtObject != hit.transform.gameObject)
            {// New GameObject.
                    _gazedAtObject?.SendMessage("OnPointerExit", null, SendMessageOptions.DontRequireReceiver);
                    _gazedAtObject = hit.transform.gameObject;
                    _gazedAtObject.SendMessage("OnPointerEnter", null, SendMessageOptions.DontRequireReceiver);
                    GazeManager.Instance.StartGazeSelection();
            }
            if (hit.transform.CompareTag(interactableTag))
            {
              PointerOnGaze(hit.point);
            }
            else
            {
                PointerOutGaze();
            }

            bool isInteractive = (1 << _gazedAtObject.layer & ReticleInteractionLayerMask) != 0;
            SetParams(hit.distance, isInteractive);
        }
        else
        {
            // No GameObject detected in front of the camera.
            _gazedAtObject?.SendMessage("OnPointerExit", null, SendMessageOptions.DontRequireReceiver);
            _gazedAtObject = null;
            ResetParams();
        }

        // Checks for screen touches.
        if (Google.XR.Cardboard.Api.IsTriggerPressed)
        {
            _gazedAtObject?.SendMessage("OnPointerClick", null, SendMessageOptions.DontRequireReceiver);
        }

        UpdateDiameters();
    }

    /// <summary>
    /// Updates the material based on the reticle properties.
    /// </summary>
    private void UpdateDiameters()
    {
        _reticleDistanceInMeters =
      Mathf.Clamp(_reticleDistanceInMeters, _RETICLE_MIN_DISTANCE, _RETICLE_MAX_DISTANCE);

        if (_reticleInnerAngle < _RETICLE_MIN_INNER_ANGLE)
        {
            _reticleInnerAngle = _RETICLE_MIN_INNER_ANGLE;
        }

        if (_reticleOuterAngle < _RETICLE_MIN_OUTER_ANGLE)
        {
            _reticleOuterAngle = _RETICLE_MIN_OUTER_ANGLE;
        }

        float inner_half_angle_radians = Mathf.Deg2Rad * _reticleInnerAngle * 0.5f;
        float outer_half_angle_radians = Mathf.Deg2Rad * _reticleOuterAngle * 0.5f;

        float inner_diameter = 2.0f * Mathf.Tan(inner_half_angle_radians);
        float outer_diameter = 2.0f * Mathf.Tan(outer_half_angle_radians);

        _reticleInnerDiameter = Mathf.Lerp(
            _reticleInnerDiameter, inner_diameter, Time.unscaledDeltaTime * _RETICLE_GROWTH_SPEED);
        _reticleOuterDiameter = Mathf.Lerp(
            _reticleOuterDiameter, outer_diameter, Time.unscaledDeltaTime * _RETICLE_GROWTH_SPEED);

        _reticleMaterial.SetFloat(
            "_InnerDiameter", _reticleInnerDiameter * _reticleDistanceInMeters);
        _reticleMaterial.SetFloat(
            "_OuterDiameter", _reticleOuterDiameter * _reticleDistanceInMeters);
        _reticleMaterial.SetFloat("_DistanceInMeters", _reticleDistanceInMeters);
    }

    /// <summary>
    /// Sets the reticle pointer's inner angle, outer angle and distance.
    /// </summary>
    /// <param name="distance">The distance to the target location.</param>
    /// <param name="interactive">Whether the pointer is pointing at an interactive object.</param>
    private void SetParams(float distance, bool interactive)
    {
        _reticleDistanceInMeters = Mathf.Clamp(distance,
                                              _RETICLE_MIN_DISTANCE,
                                              _RETICLE_MAX_DISTANCE);
        if (interactive)
        {
            _reticleInnerAngle = _RETICLE_MIN_INNER_ANGLE + _RETICLE_GROWTH_ANGLE;
            _reticleOuterAngle = _RETICLE_MIN_OUTER_ANGLE + _RETICLE_GROWTH_ANGLE;
        }
        else
        {
            _reticleInnerAngle = _RETICLE_MIN_INNER_ANGLE;
            _reticleOuterAngle = _RETICLE_MIN_OUTER_ANGLE;
        }
    }

    /// <summary>
    /// Exits the reticle pointer's target.
    /// </summary>
    private void ResetParams()
    {
        _reticleDistanceInMeters = _RETICLE_MAX_DISTANCE;
        _reticleInnerAngle = _RETICLE_MIN_INNER_ANGLE;
        _reticleOuterAngle = _RETICLE_MIN_OUTER_ANGLE;
    }

    /// <summary>
    /// Creates the mesh used to draw the reticle.
    /// </summary>
    private void CreateMesh()
    {
        Mesh mesh = new Mesh();
        gameObject.AddComponent<MeshFilter>();
        GetComponent<MeshFilter>().mesh = mesh;

        int segments_count = _RETICLE_SEGMENTS;
        int vertex_count = (segments_count + 1) * 2;

        // Vertices.
        Vector3[] vertices = new Vector3[vertex_count];

        const float kTwoPi = Mathf.PI * 2.0f;
        int vi = 0;
        for (int si = 0; si <= segments_count; ++si)
        {
            // Add two vertices for every circle segment: one at the beginning of the
            // prism, and one at the end of the prism.
            float angle = (float)si / (float)segments_count * kTwoPi;

            float x = Mathf.Sin(angle);
            float y = Mathf.Cos(angle);

            vertices[vi++] = new Vector3(x, y, 0.0f); // Outer vertex.
            vertices[vi++] = new Vector3(x, y, 1.0f); // Inner vertex.
        }

        // Triangles.
        int indices_count = (segments_count + 1) * 3 * 2;
        int[] indices = new int[indices_count];

        int vert = 0;
        int idx = 0;
        for (int si = 0; si < segments_count; ++si)
        {
            indices[idx++] = vert + 1;
            indices[idx++] = vert;
            indices[idx++] = vert + 2;

            indices[idx++] = vert + 1;
            indices[idx++] = vert + 2;
            indices[idx++] = vert + 3;

            vert += 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateBounds();
    }

    private void PointerOnGaze(Vector3 hitPoint)
    {
        float scaleFactor = scaleSize * Vector3.Distance(transform.position, hitPoint);
        pointer.transform.localScale = Vector3.one * scaleFactor;
        pointer.transform.position = CalculatePointerPosition(transform.position, hitPoint, disPointerObject);
    }
    private void PointerOutGaze()
    {
        pointer.transform.localScale = Vector3.one * 0.1f;
        pointer.transform.parent.transform.localPosition = new Vector3(0, 0, maxDistancePointer);
        pointer.transform.parent.parent.transform.rotation = transform.rotation;
        GazeManager.Instance.CancelGazeSelection();
    }

    private Vector3 CalculatePointerPosition(Vector3 p0, Vector3 p1, float t)
    {
        float x = p0.x + t * (p1.x - p0.x);
        float y = p0.y + t * (p1.y - p0.y);
        float z = p0.z + t * (p1.z - p0.z);

        return new Vector3(x, y, z);
    }
}
