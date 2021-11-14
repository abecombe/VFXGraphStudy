using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class Flocking : MonoBehaviour
{
	#region ComputeShader Properties

	[SerializeField]
	ComputeShader _flockingCS = null;

	GraphicsBuffer _positionBuffer;
	public GraphicsBuffer PositionBuffer()
	{
		return _positionBuffer != null ? _positionBuffer : null;
	}

	GraphicsBuffer _smoothedPositionBuffer;
	public GraphicsBuffer SmoothedPositionBuffer()
	{
		return _smoothedPositionBuffer != null ? _smoothedPositionBuffer : null;
	}

	GraphicsBuffer _velocityBuffer;
	public GraphicsBuffer VelocityBuffer()
	{
		return _velocityBuffer != null ? _velocityBuffer : null;
	}

	GraphicsBuffer _smoothedVelocityBuffer;
	public GraphicsBuffer SmoothedVelocityBuffer()
	{
		return _smoothedVelocityBuffer != null ? _smoothedVelocityBuffer : null;
	}

	#endregion

	#region Instancing Parameters

	[SerializeField]
	int _numInstance = 10000;
	public int numInstance
	{
		get { return Mathf.Max(_numInstance, 256); }
		set { _numInstance = value; }
	}

	#endregion

	#region Flocking Parameters

	[SerializeField]
	Vector2 _speedRange = Vector2.zero;
	public Vector2 speedRange
	{
		get { return _speedRange; }
		set { _speedRange = value; }
	}

	[SerializeField]
	Vector3 _forceWeight = Vector3.zero;
	public Vector3 forceWeight
	{
		get { return _forceWeight; }
		set { _forceWeight = value; }
	}

	[SerializeField]
	Vector3 _perceptionRadius = Vector3.zero;
	public Vector3 perceptionRadius
	{
		get { return _perceptionRadius; }
		set { _perceptionRadius = value; }
	}

	[SerializeField]
	float _maxSteerForce = 0f;
	public float maxSteerForce
	{
		get { return _maxSteerForce; }
		set { _maxSteerForce = value; }
	}

	#endregion

	#region seeking Parameters

	[SerializeField]
	Vector3 _targetPosition = Vector3.zero;
	public Vector3 targetPosition
	{
		get { return _targetPosition; }
		set { _targetPosition = value; }
	}

	[SerializeField]
	float _targetSeekForce = 0f;
	public float targetSeekForce
	{
		get { return _targetSeekForce; }
		set { _targetSeekForce = value; }
	}

	[SerializeField]
	float _targetClampDistance = 0f;
	public float targetClampDistance
	{
		get { return _targetClampDistance; }
		set { _targetClampDistance = value; }
	}

	#endregion

	#region Render Settings

	[SerializeField]
	Mesh _mesh = null;
	public Mesh mesh
	{
		get { return _mesh; }
		set { _mesh = value; }
	}

	[SerializeField]
	float _scale = 0f;
	public float scale
	{
		get { return _scale; }
		set { _scale = value; }
	}

	[SerializeField, Range(0, 1)]
	float _scaleRandomness = 0f;
	public float scaleRandomness
	{
		get { return _scaleRandomness; }
		set { _scaleRandomness = value; }
	}

	[SerializeField]
	Material _material;
	public Material material
	{
		get { return _material; }
		set { _material = value; }
	}

	#endregion

	#region Misc Settings

	[SerializeField, Range(0, 1)]
	float _randomSeed = 0.5f;

	public float randomSeed
	{
		get { return _randomSeed; }
		set { _randomSeed = value; }
	}

	#endregion

	#region Resource Management

	bool _needsReset = false;

	public void NotifyConfigChange()
	{
		_needsReset = true;
	}

	void ResetResources()
	{
		ReleaseBuffer();

		InitBuffer();

		_needsReset = false;
	}

	#endregion

	#region MonoBehaviour Functions

	void Start()
	{
		var vfx = GetComponent<VisualEffect>();
		vfx.SetFloat("NumInstance", numInstance);
		InitBuffer();
	}

	void FixedUpdate()
	{
		if (_needsReset) ResetResources();

		Simulation();

		RenderInstancedMesh();
	}

	void OnDestroy()
	{
		ReleaseBuffer();
	}

    #endregion

    #region Private Functions

    Vector2 GetMousePosition()
    {
		Vector2 mousePosition = Input.mousePosition;
		mousePosition /= new Vector2(Screen.width, Screen.height);
		if (mousePosition.x < 0f || mousePosition.x > 1f || mousePosition.y < 0f || mousePosition.y > 1f)
			mousePosition = new Vector2(0.5f, 0.5f);

		return mousePosition;
	}

    void InitBuffer()
	{
		_positionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, numInstance, Marshal.SizeOf(typeof(Vector3)));
		_velocityBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, numInstance, Marshal.SizeOf(typeof(Vector3)));
		_smoothedPositionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, numInstance, Marshal.SizeOf(typeof(Vector3)));
		_smoothedVelocityBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, numInstance, Marshal.SizeOf(typeof(Vector3)));

		var positionArray = new Vector3[numInstance];
		var velocityArray = new Vector3[numInstance];
		for (var i = 0; i < numInstance; i++)
		{
			positionArray[i] = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1)) * 10f;
			var theta = Random.Range(-Mathf.PI, Mathf.PI);
			var phi = Mathf.Asin(Random.Range(-1f, 1f));
			velocityArray[i] = new Vector3(Mathf.Cos(phi) * Mathf.Cos(theta), Mathf.Cos(phi) * Mathf.Sin(theta), Mathf.Sin(phi)) * (_speedRange.x + _speedRange.y) * 0.5f;
		}

		_positionBuffer.SetData(positionArray);
		_velocityBuffer.SetData(velocityArray);
		_smoothedPositionBuffer.SetData(positionArray);
		_smoothedVelocityBuffer.SetData(velocityArray);

		positionArray = null;
		velocityArray = null;
	}

	void Simulation()
	{
		var mousePosition = GetMousePosition();
		var _targetPosition = new Vector3((mousePosition.x - 0.5f) * 20f, (mousePosition.y - 0.5f) * 11f, 0f);

		ComputeShader cs = _flockingCS;
		int kernelID = -1;

		kernelID = cs.FindKernel("FlockingCS");

		uint threadSizeX, threadSizeY, threadSizeZ;
		cs.GetKernelThreadGroupSizes(kernelID, out threadSizeX, out threadSizeY, out threadSizeZ);
		int threadGroupSize = Mathf.CeilToInt(numInstance / (float)threadSizeX);

		cs.SetBuffer(kernelID, "_PositionBuffer", _positionBuffer);
		cs.SetBuffer(kernelID, "_VelocityBuffer", _velocityBuffer);
		cs.SetBuffer(kernelID, "_SmoothedPositionBuffer", _smoothedPositionBuffer);
		cs.SetBuffer(kernelID, "_SmoothedVelocityBuffer", _smoothedVelocityBuffer);

		cs.SetInt("_NumInstance", numInstance);

		cs.SetVector("_SpeedRange", _speedRange);
		cs.SetVector("_ForceWeight", _forceWeight);
		cs.SetVector("_PerceptionRadius", _perceptionRadius);
		cs.SetFloat("_MaxSteerForce", _maxSteerForce);

		cs.SetVector("_TargetPosition", _targetPosition);
		cs.SetFloat("_TargetSeekForce", _targetSeekForce);
		cs.SetFloat("_TargetClampDistance", _targetClampDistance);

		cs.SetFloat("_DeltaTime", Time.deltaTime);

		cs.Dispatch(kernelID, threadGroupSize, 1, 1);
	}

	void ReleaseBuffer()
	{
		if (_positionBuffer != null) { _positionBuffer.Release(); _positionBuffer = null; }
		if (_velocityBuffer != null) { _velocityBuffer.Release(); _velocityBuffer = null; }
		if (_smoothedPositionBuffer != null) { _smoothedPositionBuffer.Release(); _smoothedPositionBuffer = null; }
		if (_smoothedVelocityBuffer != null) { _smoothedVelocityBuffer.Release(); _smoothedVelocityBuffer = null; }
	}

	void RenderInstancedMesh()
	{
		var vfx = GetComponent<VisualEffect>();
		vfx.SetGraphicsBuffer("PositionBuffer", _smoothedPositionBuffer);
		vfx.SetGraphicsBuffer("VelocityBuffer", _smoothedVelocityBuffer);
	}

	#endregion
}
