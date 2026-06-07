using Chishiki.Vision.Abstraction.Monitors;
using Chishiki.Vision.Common.Monitors;
using Chishiki.Vision.Onnx.Detector.Objects;
using Chishiki.Vision.OpenCV.Detectors.Motion;
using Microsoft.Extensions.Logging;

namespace Chishiki.Vision.OpenCV.Sources;

public class OpenCVVideoSourceMonitor(
    OpenCVVideoSource source,
    OpenCVMotionDetector detector,
    IReadOnlyCollection<YoloDetector> objectDetectors,
    VideoSourceMonitorOptions options,
    ILogger<OpenCVVideoSourceMonitor> logger) : VideoSourceMonitor(source, detector, objectDetectors, options, logger)
{
    
}
