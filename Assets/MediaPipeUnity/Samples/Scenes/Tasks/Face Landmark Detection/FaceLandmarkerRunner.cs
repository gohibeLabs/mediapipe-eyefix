// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using Mediapipe;
using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Vision.FaceLandmarker;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample;
using Mediapipe.Unity.Sample.FaceLandmarkDetection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace MediaPipeUnity.Samples.Scenes.Tasks.Face_Landmark_Detection
{
    public class FaceLandmarkerRunner : VisionTaskApiRunner<FaceLandmarker>
    {
        [FormerlySerializedAs("_faceLandmarkerResultAnnotationController")] [SerializeField] private FaceLandmarkerResultAnnotationController faceLandmarkerResultAnnotationController;

        private Mediapipe.Unity.Experimental.TextureFramePool _textureFramePool;

        public readonly FaceLandmarkDetectionConfig Config = new();

        public override void Stop()
        {
            base.Stop();
            _textureFramePool?.Dispose();
            _textureFramePool = null;
        }

        protected override IEnumerator Run()
        {
            Debug.Log($"Delegate = {Config.Delegate}");
            Debug.Log($"Running Mode = {Config.RunningMode}");
            Debug.Log($"NumFaces = {Config.NumFaces}");
            Debug.Log($"MinFaceDetectionConfidence = {Config.MinFaceDetectionConfidence}");
            Debug.Log($"MinFacePresenceConfidence = {Config.MinFacePresenceConfidence}");
            Debug.Log($"MinTrackingConfidence = {Config.MinTrackingConfidence}");
            Debug.Log($"OutputFaceBlendshapes = {Config.OutputFaceBlendshapes}");
            Debug.Log($"OutputFacialTransformationMatrixes = {Config.OutputFacialTransformationMatrixes}");

            yield return AssetLoader.PrepareAssetAsync(Config.ModelPath);

            var options = Config.GetFaceLandmarkerOptions(Config.RunningMode == Mediapipe.Tasks.Vision.Core.RunningMode.LIVE_STREAM ? OnFaceLandmarkDetectionOutput : null);
            taskApi = FaceLandmarker.CreateFromOptions(options);
            var imageSource = ImageSourceProvider.ImageSource;

            yield return imageSource.Play();

            if (!imageSource.isPrepared)
            {
                Debug.LogError("Failed to start ImageSource, exiting...");
                yield break;
            }

            // Use RGBA32 as the input format.
            // TODO: When using GpuBuffer, MediaPipe assumes that the input format is BGRA, so maybe the following code needs to be fixed.
            _textureFramePool = new Mediapipe.Unity.Experimental.TextureFramePool(imageSource.textureWidth, imageSource.textureHeight);

            // NOTE: The screen will be resized later, keeping the aspect ratio.
            screen.Initialize(imageSource);

            SetupAnnotationController(faceLandmarkerResultAnnotationController, imageSource);

            var transformationOptions = imageSource.GetTransformationOptions();
            var flipHorizontally = transformationOptions.flipHorizontally;
            var flipVertically = transformationOptions.flipVertically;
            var imageProcessingOptions = new Mediapipe.Tasks.Vision.Core.ImageProcessingOptions(rotationDegrees: (int)transformationOptions.rotationAngle);

            AsyncGPUReadbackRequest req = default;
            var req1 = req;
            var waitUntilReqDone = new WaitUntil(() => req1.done);
            var result = FaceLandmarkerResult.Alloc(options.numFaces);

            while (true)
            {
                if (isPaused)
                    yield return new WaitWhile(() => isPaused);

                if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                // Copy current image to TextureFrame
                req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
                yield return waitUntilReqDone;

                if (req.hasError)
                {
                    Debug.LogError($"Failed to read texture from the image source, exiting...");
                    break;
                }

                var image = textureFrame.BuildCPUImage();
                switch (taskApi.runningMode)
                {
                    case Mediapipe.Tasks.Vision.Core.RunningMode.IMAGE:
                        if (taskApi.TryDetect(image, imageProcessingOptions, ref result))
                            faceLandmarkerResultAnnotationController.DrawNow(result);
                        else
                            faceLandmarkerResultAnnotationController.DrawNow(default);
                        break;
                    case Mediapipe.Tasks.Vision.Core.RunningMode.VIDEO:
                        if (taskApi.TryDetectForVideo(image, GetCurrentTimestampMillisec(), imageProcessingOptions, ref result))
                            faceLandmarkerResultAnnotationController.DrawNow(result);
                        else
                            faceLandmarkerResultAnnotationController.DrawNow(default);
                        break;
                    case Mediapipe.Tasks.Vision.Core.RunningMode.LIVE_STREAM:
                        taskApi.DetectAsync(image, GetCurrentTimestampMillisec(), imageProcessingOptions);
                        break;
                }

                textureFrame.Release();
            }
        }

        private void OnFaceLandmarkDetectionOutput(FaceLandmarkerResult result, Image image, long timestamp)
        {
            //Debug.Log("RESULT = " + result.facialTransformationMatrixes);
            Classifications data = result.faceBlendshapes[0];
            ExpressionApplier.Instance.ApplyDataOnFace(data);
            ExpressionApplier.Instance.SetFaceRotation(result.facialTransformationMatrixes);
            IrisTracker.Instance.TrackIrisMovement(result.faceLandmarks);
            //foreach(Category cat in data.categories)
            //{
            //    Debug.Log("BS " + cat.categoryName + " Score = " + (cat.score * 100));
            //}
            //_faceLandmarkerResultAnnotationController.DrawLater(result);
        }
    }
}