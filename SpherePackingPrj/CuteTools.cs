﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kitware.VTK;
using Emgu.CV;
using Emgu.CV.Structure;

namespace SpherePacking.MainWindow
{
    class CuteTools
    {
        /// <summary>
        /// 生成测试模拟退火的测试图片，生成的圆是白色的
        /// example：
        ///         CuteTools.GenerateTestCircleImage(new Size(200,200),new Gray(0),20,"11.bmp");
        /// </summary>
        /// <param name="size">尺寸</param>
        /// <param name="gray">图像背景的灰度值</param>
        /// <param name="radius">测试图片的圆的颜色</param>
        /// <param name="fn">保存的图像的文件名</param>
        public static void GenerateTestCircleImage(Size size, Gray gray, int radius, string fn)
        {
            Image<Gray, Byte> image = new Image<Gray, byte>(size.Width, size.Height, gray);
            for (int i = 0; i < image.Rows/radius/2; i++)
            {
                for (int j = 0; j < image.Cols/radius/2; j++)
                {
                    CvInvoke.Circle(image, new Point(j * radius*2 + radius, i * radius*2 + radius), radius, new MCvScalar(0, 0, 0), -1);
                }
            }
            //CvInvoke.Imshow("image", image);
            CvInvoke.Imwrite(fn, image);
        }

        /// <summary>
        /// 小球在边缘碰撞后的速度
        /// 验证小球在碰到墙壁后的速度计算的正确性
        /// </summary>
        public static void TestCircleBoundCollision()
        {
            double vx, vy;
            double x = 1;
            double y = 0; //位置
            double rotAngle = Math.Atan2(y,x) - Math.PI / 2; //旋转坐标轴的角度
            double vx0 = 3;
            double vy0 = Math.Sqrt(3);  //速度
            //旋转坐标轴，将速度沿切线和法线方向分解
            vx = vx0 * Math.Cos(rotAngle) + vy0 * Math.Sin(rotAngle);
            vy = -vx0 * Math.Sin(rotAngle) + vy0 * Math.Cos(rotAngle);

            //圆心法线方向速度衰减
            vy = vy * 0.5;

            //将速度坐标轴变换回来
            vx0 = vx * Math.Cos(rotAngle) - vy * Math.Sin(rotAngle);
            vy0 = vx * Math.Sin(rotAngle) + vy * Math.Cos(rotAngle);

            Console.WriteLine(vx + ", " + vy);
            Console.WriteLine(vx0 + ", " + vy0);
        }

        /// <summary>
        /// 用体绘制的方法绘制一系列的图像
        /// </summary>
        /// <param name="format">图像文件的字符串格式</param>
        /// <param name="height">一幅图像的高度</param>
        /// <param name="width">高度</param>
        /// <param name="startIndex">起始index</param>
        /// <param name="endIndex">末尾index</param>
        /// example:
        ///     CuteTools.ShowImageSeries(@"initial/%03d.bmp",64, 64, 0, 62);
        public static void ShowImageSeries(string format, int height, int width, int startIndex, int endIndex)
        {
            if (format == null || format.Count() <= 4 || (!format.Substring(format.Count() - 3, 3).Equals("bmp")))
            {
                Console.WriteLine("image filename is not correct!!");
                return;
            }

            vtkBMPReader reader = vtkBMPReader.New();
            reader.SetFilePattern(format);

            reader.SetDataExtent(0, height - 1, 0, width - 1, startIndex, endIndex);

            reader.SetDataScalarTypeToUnsignedChar();
            reader.Update();

            

            vtkVolume vol = vtkVolume.New();
            vtkRenderer render = vtkRenderer.New();
            vtkFixedPointVolumeRayCastMapper texMapper = vtkFixedPointVolumeRayCastMapper.New();

            texMapper.SetInput(reader.GetOutput());
            vol.SetMapper(texMapper);

            vtkColorTransferFunction colorTransferFunction = vtkColorTransferFunction.New();
            colorTransferFunction.AddRGBPoint(0.0, 0.0, 1.0, 0.0);
            //colorTransferFunction.AddRGBPoint(120.0, 0.0, 0.0, 1.0);
            //colorTransferFunction.AddRGBPoint(160.0, 1.0, 0.0, 0.0);
            //colorTransferFunction.AddRGBPoint(200.0, 0.0, 1.0, 0.0);
            colorTransferFunction.AddRGBPoint(200.0, 1.0, 0, 1.0);
            colorTransferFunction.ClampingOn();

            vtkVolumeProperty vpro = vtkVolumeProperty.New();
            vtkPiecewiseFunction compositeOpacity = vtkPiecewiseFunction.New();
            compositeOpacity.AddPoint(80, 1);
            compositeOpacity.AddPoint(120, 0.2);
            compositeOpacity.AddPoint(255, 0.2);
            compositeOpacity.ClampingOn();
            vpro.SetScalarOpacity(compositeOpacity);
            vpro.SetColor( colorTransferFunction );
            vpro.SetInterpolationTypeToLinear();
            //vpro.ShadeOn();
            vol.SetProperty(vpro);   

            render.AddVolume(vol);
            render.SetBackground(1, 1, 1);

            vtkRenderWindow wnd = vtkRenderWindow.New();
            wnd.AddRenderer(render);


            vtkRenderWindowInteractor inter = vtkRenderWindowInteractor.New();
            inter.SetRenderWindow(wnd);

            inter.Initialize();
            inter.Start();
        }

    }
}