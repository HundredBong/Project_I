using System;
using System.Linq;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;


public class GCProbe : MonoBehaviour
{
    private ProfilerRecorder _gcAllocRecorder;
    private ProfilerRecorder _systemGCRecorder; // "System.GC.Collect" 카운트

    private double _totalAllocBytes;
    private long _maxAllocBytes;
    private int _frameCount;
    private int _startGen0Count;
    private float _elapsed;

    private const float ReportInterval = 60f; // 1분마다 로그
    private int _printCount = 1;

    private void OnEnable()
    {
        //프레임당 GC.Alloc 바이트
        _gcAllocRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC.Alloc", 1, ProfilerRecorderOptions.CollectOnlyOnCurrentThread);

        //콜렉션 이벤트 카운트(있을 때만 잡힘)
        _systemGCRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System.GC.Collect", 1);

        _startGen0Count = GC.CollectionCount(0);
    }

    private void OnDisable()
    {
        if (_gcAllocRecorder.Valid)
        {
            _gcAllocRecorder.Dispose();
        }
        if (_systemGCRecorder.Valid)
        {
            _systemGCRecorder.Dispose();
        }
    }

    private void Update()
    {
        _frameCount++;
        _elapsed += Time.unscaledDeltaTime;

        long allocBytesThisFrame = 0;

        if (_gcAllocRecorder.Valid && _gcAllocRecorder.Count > 0)
        {
            allocBytesThisFrame = Math.Max(0, _gcAllocRecorder.LastValue);
        }
        Debug.Log(allocBytesThisFrame);
        _totalAllocBytes += allocBytesThisFrame;
        if (allocBytesThisFrame > _maxAllocBytes)
        {
            _maxAllocBytes = allocBytesThisFrame;
        }

        if (_elapsed >= ReportInterval)
        {
            PrintReport();
            ResetWindow();
        }
    }

    private void PrintReport()
    {
        double avgAlloc = (_frameCount > 0) ? _totalAllocBytes / _frameCount : 0f;
        int gen0 = GC.CollectionCount(0) - _startGen0Count;

        long managedUsed = Profiler.GetTotalAllocatedMemoryLong();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"[GCProbe] ===== {_printCount}분 리포트 =====");
        sb.AppendLine($"프레임 수           : {_frameCount}");
        sb.AppendLine($"GC.Alloc/프레임 평균: {avgAlloc:n0} B");
        sb.AppendLine($"GC.Alloc/프레임 최대: {_maxAllocBytes:n0} B");
        sb.AppendLine($"Gen0 컬렉션 수      : {gen0}");
        sb.AppendLine($"System.GC.Collect   : {(_systemGCRecorder.Valid ? _systemGCRecorder.LastValue : 0)} (샘플기반)");
        sb.AppendLine($"TotalAllocated      : {managedUsed / (1024f * 1024f):n2} MB");

        Debug.Log(sb.ToString());

        _printCount++;
    }

    private void ResetWindow()
    {
        _totalAllocBytes = 0;
        _maxAllocBytes = 0;
        _frameCount = 0;
        _elapsed = 0f;
        _startGen0Count = GC.CollectionCount(0);
    }

}
