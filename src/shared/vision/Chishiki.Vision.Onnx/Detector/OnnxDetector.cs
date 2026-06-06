// -----------------------------------------------------------------------------
// File:        OnnxDetector.cs
// Author:      Piergiorgio Vagnozzi
// Description: Abstract base class for ONNX Runtime-based object detectors with template method pattern.
// Created:     2025-01-01
// Modified:    2026-06-05
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Vision.Abstraction;
using Chishiki.Vision.Abstraction.Detectors;
using Chishiki.Vision.Abstraction.Detectors.Objects;
using Chishiki.Vision.OpenCV.Detectors;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;

namespace Chishiki.Vision.Onnx.Detector;

/// <summary>
/// Abstract base class for ONNX Runtime-based object detectors.
/// Provides a template method pattern for preprocessing, inference, and postprocessing
/// while allowing subclasses (e.g., YOLO variants) to customize each step.
/// </summary>
/// <param name="options">Configuration options for the ONNX detector, including model path and input size.</param>
/// <param name="logger">Logger for diagnostic output.</param>
public abstract partial class OnnxDetector : OpenCVDetector<DetectionResult<ObjectDetection>, ObjectDetection>
{
    /// <summary>
    /// Inference session for running the ONNX model. Initialized in the constructor and disposed in Dispose().
    /// </summary>
    protected InferenceSession Session { get; init; }

    /// <inheritdoc/>
    public new OnnxDetectorOptions Options => (OnnxDetectorOptions)base.Options;

    /// <summary>
    /// Initialises a new <see cref="OnnxDetector"/> with the supplied options and logger.
    /// </summary>
    /// <param name="options">Configuration options for the ONNX detector.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <exception cref="FileNotFoundException">Thrown when the model file does not exist.</exception>
    protected OnnxDetector(OnnxDetectorOptions options, ILogger<OnnxDetector> logger) : base(options, logger)
    {
        if (!File.Exists(options.ModelPath))
        {
            LogModelFileMissing(Logger!, options.ModelPath);
            throw new FileNotFoundException($"ONNX model file not found: {options.ModelPath}");
        }

        var sessionOptions = new SessionOptions
        {
            IntraOpNumThreads = options.IntraOpNumThreads,
            InterOpNumThreads = options.InterOpNumThreads
        };

        if (options.UseGpu)
        {
            sessionOptions.AppendExecutionProvider_CUDA(0);
        }

        Session = new InferenceSession(options.ModelPath, sessionOptions);
        LogSessionCreated(Logger!, options.ModelPath, options.UseGpu);
    }

    /// <inheritdoc/>
    protected override async Task<DetectionResult<ObjectDetection>> ProcessFrameAsync(IImage originalImage, Mat image, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Preprocessing: convert IImage → Tensor<float>
            var inputTensor = await PreprocessAsync(image, cancellationToken);

            // 2. Inference: run ONNX model
            var outputs = await RunInferenceAsync(inputTensor, cancellationToken);

            // 3. Postprocessing: decode outputs → ObjectDetection list
            var detections = await PostprocessAsync(outputs, image, cancellationToken);
            LogDetectionCompleted(Logger, detections.Count);
            return new DetectionResult<ObjectDetection>(originalImage, detections: detections);
        }
        catch (Exception ex)
        {
            LogDetectionFailed(Logger, Options.ModelPath, ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public override void Reset()
    {
    }

    /// <inheritdoc/>
    protected override void DisposeManaged()
    {
        Session.Dispose();
        base.DisposeManaged();
    }

    /// <summary>
    /// Converts the input <paramref name="frame"/> into a normalized tensor suitable for ONNX inference.
    /// Subclasses must implement image decoding, resizing, normalization, and CHW layout conversion.
    /// </summary>
    /// <param name="frame">The source image.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A <see cref="DenseTensor{T}"/> in NCHW layout (batch=1, channels=3, height, width).</returns>
    protected abstract Task<DenseTensor<float>> PreprocessAsync(Mat frame, CancellationToken cancellationToken);

    /// <summary>
    /// Decodes the ONNX model outputs into a list of <see cref="ObjectDetection"/> instances.
    /// Subclasses must implement output parsing, confidence filtering, and Non-Maximum Suppression.
    /// </summary>
    /// <param name="outputs">The raw output tensors from the ONNX session.</param>
    /// <param name="originalFrame">The original frame used for coordinate scaling.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A list of detected objects after NMS and filtering.</returns>
    protected abstract Task<IReadOnlyList<ObjectDetection>> PostprocessAsync(
        IDisposableReadOnlyCollection<DisposableNamedOnnxValue> outputs,
        Mat originalFrame,
        CancellationToken cancellationToken);

    /// <summary>Runs the ONNX model inference on the supplied input tensor.</summary>
    /// <param name="inputTensor">The preprocessed input tensor in NCHW layout.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>The raw output tensors from the model.</returns>
    protected virtual Task<IDisposableReadOnlyCollection<DisposableNamedOnnxValue>> RunInferenceAsync(
        DenseTensor<float> inputTensor,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var inputName = Session.InputNames[0] ?? "images";
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
        };

        LogInferenceStarting(Logger, inputName);
        var outputs = Session.Run(inputs);
        return Task.FromResult(outputs);
    }

    /// <summary>Emits a debug log entry when the ONNX inference session is created successfully.</summary>
    /// <param name="modelPath">The fully qualified path of the ONNX model file.</param>
    /// <param name="useGpu">A value indicating whether GPU execution is enabled.</param>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Created ONNX inference session for model {ModelPath} with GPU enabled: {UseGpu}")]
    private static partial void LogSessionCreated(ILogger logger, string modelPath, bool useGpu);

    /// <summary>Emits a debug log entry before running inference for the current input tensor.</summary>
    /// <param name="inputName">The ONNX input tensor name.</param>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "Running ONNX inference using input {InputName}")]
    private static partial void LogInferenceStarting(ILogger logger, string inputName);

    /// <summary>Emits a debug log entry after a detection pass completes successfully.</summary>
    /// <param name="detectionCount">The number of detections produced by the model.</param>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Debug, Message = "ONNX detection completed with {DetectionCount} detections")]
    private static partial void LogDetectionCompleted(ILogger logger, int detectionCount);

    /// <summary>Emits an error log entry when ONNX detection fails.</summary>
    /// <param name="modelPath">The fully qualified path of the ONNX model file.</param>
    /// <param name="exception">The exception that caused the detection failure.</param>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Error, Message = "ONNX detection failed for model {ModelPath}")]
    private static partial void LogDetectionFailed(ILogger logger, string modelPath, Exception exception);

    /// <summary>Emits an error log entry when the configured ONNX model file cannot be found.</summary>
    /// <param name="modelPath">The fully qualified path of the ONNX model file.</param>
    [LoggerMessage(Level = Microsoft.Extensions.Logging.LogLevel.Error, Message = "ONNX model file not found at {ModelPath}")]
    private static partial void LogModelFileMissing(ILogger logger, string modelPath);
}

