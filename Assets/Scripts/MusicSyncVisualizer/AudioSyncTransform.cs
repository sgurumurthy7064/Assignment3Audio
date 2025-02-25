using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSyncTransform : AudioSyncer
{
    public Vector3 beatPositionOffset;
    public Vector3 beatRotation;
    public Vector3 restRotation;
    public float archHeight = 1.0f; // Height of the arch

    private Vector3 initialLocalPosition;

    public struct TransformData
    {
        public Vector3 position;
        public Vector3 rotation;
    }

    private IEnumerator MoveToTransform(TransformData _targetData)
    {
        Vector3 _currPosition = transform.localPosition;
        Vector3 _currRotation = transform.localEulerAngles;
        Vector3 _initialPosition = _currPosition;
        Vector3 _initialRotation = _currRotation;
        float _timer = 0;
        float safetyTimer = 0;

        while (_timer / timeToBeat < 1f)
        {
            float progress = _timer / timeToBeat; // Normalized progress (0 to 1)

            Vector3 parabolicPosition = Vector3.Lerp(_initialPosition, _targetData.position, progress);

            // Add arch height based on parabolic curve
            parabolicPosition.y += archHeight * Mathf.Sin(progress * Mathf.PI);

            _currRotation = Vector3.Lerp(_initialRotation, _targetData.rotation, progress);
            _timer += Time.deltaTime;
            safetyTimer += Time.deltaTime;

            transform.localPosition = parabolicPosition;
            transform.localEulerAngles = _currRotation;

            yield return null;

            if (safetyTimer > 5f)
            {
                Debug.LogError("MoveToTransform Safety timer triggered. Breaking loop.");
                break;
            }
        }

        m_isBeat = false;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (m_isBeat) return;

        transform.localPosition = Vector3.Lerp(transform.localPosition, initialLocalPosition, restSmoothTime * Time.deltaTime);
        transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, restRotation, restSmoothTime * Time.deltaTime);
    }

    public override void OnBeat()
    {
        base.OnBeat();

        StopCoroutine("MoveToTransform");
        TransformData targetData = new TransformData();
        targetData.position = transform.localPosition + beatPositionOffset;
        targetData.rotation = beatRotation;
        StartCoroutine("MoveToTransform", targetData);
    }

    private void Start()
    {
        initialLocalPosition = transform.localPosition;
    }
}