using UnityEngine;
using UnityEngine.VFX;
using System.Runtime.InteropServices;

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

	[SerializeField, Range(256, 8192)]
	int _numInstance = 1024;
	public int numInstance
	{
		get { return _numInstance; }
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

	#region Target Seeking Parameters

	[SerializeField]
	GameObject _targetObject = null;
	public GameObject targetObject
	{
		get { return _targetObject; }
		set { _targetObject = value; }
	}

	[SerializeField]
	float _targetSeekForce = 0f;
	public float targetSeekForce
	{
		get { return _targetSeekForce; }
		set { _targetSeekForce = value; }
	}

	[SerializeField]
	float _targetSeekClampDistance = 0f;
	public float targetSeekClampDistance
	{
		get { return _targetSeekClampDistance; }
		set { _targetSeekClampDistance = value; }
	}

	#endregion

	#region Rendering Settings

	[SerializeField]
	Vector2 _scaleRange = Vector2.zero;
	public Vector2 scaleRange
	{
		get { return _scaleRange; }
		set { _scaleRange = value; }
	}

	[SerializeField]
	float _animationSpeed;
	public float animationSpeed
	{
		get { return _animationSpeed; }
		set { _animationSpeed = value; }
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

    void InitBuffer()
	{
		var vfx = GetComponent<VisualEffect>();
		vfx.Reinit();
		vfx.SetFloat("NumInstance", _numInstance);

		_positionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _numInstance, Marshal.SizeOf(typeof(Vector3)));
		_velocityBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _numInstance, Marshal.SizeOf(typeof(Vector3)));
		_smoothedPositionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _numInstance, Marshal.SizeOf(typeof(Vector3)));
		_smoothedVelocityBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _numInstance, Marshal.SizeOf(typeof(Vector3)));

		var positionArray = new Vector3[_numInstance];
		var velocityArray = new Vector3[_numInstance];
		for (var i = 0; i < _numInstance; i++)
		{
			positionArray[i] = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 5f;
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
		var _targetPosition = Vector3.zero;
		if (_targetObject)
			_targetPosition = _targetObject.transform.position;

		ComputeShader cs = _flockingCS;
		int kernelID = -1;

		kernelID = cs.FindKernel("FlockingCS");

		uint threadSizeX, threadSizeY, threadSizeZ;
		cs.GetKernelThreadGroupSizes(kernelID, out threadSizeX, out threadSizeY, out threadSizeZ);
		int threadGroupSize = Mathf.CeilToInt(_numInstance / (float)threadSizeX);

		cs.SetBuffer(kernelID, "_PositionBuffer", _positionBuffer);
		cs.SetBuffer(kernelID, "_VelocityBuffer", _velocityBuffer);
		cs.SetBuffer(kernelID, "_SmoothedPositionBuffer", _smoothedPositionBuffer);
		cs.SetBuffer(kernelID, "_SmoothedVelocityBuffer", _smoothedVelocityBuffer);

		cs.SetInt("_NumInstance", _numInstance);

		cs.SetVector("_SpeedRange", _speedRange);
		cs.SetVector("_ForceWeight", _forceWeight);
		cs.SetVector("_PerceptionRadius", _perceptionRadius);
		cs.SetFloat("_MaxSteerForce", _maxSteerForce);

		cs.SetVector("_TargetPosition", _targetPosition);
		cs.SetFloat("_TargetSeekForce", _targetSeekForce);
		cs.SetFloat("_TargetSeekClampDistance", _targetSeekClampDistance);

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
		if (_smoothedPositionBuffer != null)
			vfx.SetGraphicsBuffer("PositionBuffer", _smoothedPositionBuffer);
		if (_smoothedVelocityBuffer != null)
			vfx.SetGraphicsBuffer("VelocityBuffer", _smoothedVelocityBuffer);
		vfx.SetVector2("ScaleRange", _scaleRange);
		vfx.SetFloat("AnimationSpeed", _animationSpeed);
	}

	#endregion
}