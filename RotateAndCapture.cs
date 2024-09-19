using GTA;
using GTA.Math;
using CircleDataCollection;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

public class RotateAndCapture : Script
{
    private readonly string MODEL_NAME = "truck2"; // 替换为你想要的模型名
    // private Vector3 targetPosition = new Vector3(1708.5f, 3741.0f, 33.5f); // 默认目标坐标
    private readonly float[] elevationAngles = { 0f, 30f, 60f }; // 定义三个视角与水平线的夹角
    private const float rotationStep = 15f; // 每次旋转的角度
    private const int totalRotation = 360; // 完整旋转一圈的角度
    // private const float distanceToTarget = 50f; // 视线到坐标点的距离
    private readonly float[] distancesToTarget = { 10f, 15f, 50f }; // 定义与目标的不同距离
    private bool isCapturing = false; // 旋转和截图的标志

    public RotateAndCapture()
    {
        GTA.UI.Notification.Show("加载旋转截图脚本。");
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.O) // 使用 O 键作为启动快捷键
        {
            if (!isCapturing)
            {
                isCapturing = true;
                CaptureScreenshots();
            }
        } else if (e.KeyCode == Keys.U)
        {
            GetCameraModelDistance();
        }
    }

    private void GetCameraModelDistance()
    {
        Model targetModel = new Model(MODEL_NAME);

        // 查找所有实体并过滤出与目标模型匹配的实体
        List<Entity> entities = World.GetAllEntities().ToList();
        var targetEntities = entities.Where(entity => entity.Model == targetModel).ToList();

        if (targetEntities.Count > 0)
        {
            // 获取第一个找到的目标实体的位置
            var targetEntity = targetEntities[0];
            Vector3 targetPosition = targetEntity.Position;

            // 获取当前相机（人物视角）的位置
            Vector3 cameraPosition = GameplayCamera.Position;
            if (World.RenderingCamera != null)
            {
                // 使用自定义渲染的相机
                cameraPosition = World.RenderingCamera.Position;
            }

            // 计算距离
            float distance = cameraPosition.DistanceTo(targetPosition);

            GTA.UI.Notification.Show($"相机到模型的距离是: {distance:F2} 米");
        } else
        {
            // 如果找不到目标模型，返回-1表示没有找到
            GTA.UI.Notification.Show("找不到目标模型！");
        }
    }

    private void CaptureScreenshots() // 核心截图函数。
    {
        Vector3 targetPos = new Vector3(0, 0, 0); // 默认目标坐标，即旋转中心点
        Model targetModel = new Model(MODEL_NAME);
        List<Entity> entities = World.GetAllEntities().ToList(); // 查找所有指定模型的实体
        var modelEntities = entities.Where(entity => entity.Model == targetModel).ToList();
        if (modelEntities.Count > 0)
        {
            // 假设我们只关心第一个找到的实体
            var entity = modelEntities[0];
            targetPos = entity.Position;
            GTA.UI.Notification.Show($"模型 {targetModel} 的坐标是: {targetPos}");
        }

        Camera cam = World.CreateCamera(Vector3.Zero, Vector3.Zero, 50);
        for (int i = 0; i < elevationAngles.Length; i++) // 切换不同相机仰角
        {
            float elevationAngle = elevationAngles[i];
            float distanceToTarget = distancesToTarget[i];
            for (float angle = 0; angle < totalRotation; angle += rotationStep) // 切换水平角度
            {
                Vector3 cameraPosition = CalculateCameraPosition(targetPos, elevationAngle, angle, distanceToTarget);
                cam.Position = cameraPosition;

                cam.PointAt(targetPos); // 必须是旋转中心点的位置（不要用targetPosition，这个是默认值，且是全局变量）

                World.RenderingCamera = cam; // 在循环内重复赋值，确保渲染摄像机同步
                Wait(100); // 等待一小段时间以确保截图质量
                TakeScreenshot(angle, elevationAngle, distanceToTarget);
            }
        }
        World.RenderingCamera = null; // 取消渲染相机关联
        cam.Delete(); // 销毁相机
        isCapturing = false;
    }

    private Vector3 CalculateCameraPosition(Vector3 target, float elevationAngle, float azimuthAngle, float distance)
    {
        float elevationRadians = elevationAngle * ((float)Math.PI / 180f); // Convert degrees to radians
        float azimuthRadians = azimuthAngle * ((float)Math.PI / 180f);

        float x = target.X + distance * (float)Math.Cos(elevationRadians) * (float)Math.Cos(azimuthRadians);
        float y = target.Y + distance * (float)Math.Cos(elevationRadians) * (float)Math.Sin(azimuthRadians);
        float z = target.Z + distance * (float)Math.Sin(elevationRadians);

        return new Vector3(x, y, z);
    }

    private void TakeScreenshot(float angle, float elevationAngle, float distance)
    {
        string directory = @"E:\GTAVScreenshots"; // 保存截图的路径
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string filename = Path.Combine(directory, $"screenshot_elev{elevationAngle}_dist{distance}_azimuth{angle}.png");
        // 截图
        Bitmap screenshot = GTAVUtils.GetScreenshot();
        ImageInfo imageInfo = GetImageInfo(screenshot);
        GTAVUtils.DataPreprocess(screenshot, imageInfo).Save(filename);
    }

    private static ImageInfo GetImageInfo(Bitmap screenshot)
    {
        Vector3 camPos = World.RenderingCamera.Position;
        Vector3 camRot = World.RenderingCamera.Rotation;
        return new ImageInfo(screenshot.Width, screenshot.Height, camPos, camRot);
    }

}
