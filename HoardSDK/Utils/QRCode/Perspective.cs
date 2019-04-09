/////////////////////////////////////////////////////////////////////
//
//	QR Code Library
//
//	QR Code save image perspective calculations.
//
//	Author: Uzi Granot
//	Version: 1.0
//	Date: June 30, 2018
//	Copyright (C) 2013-2018 Uzi Granot. All Rights Reserved
//
//	QR Code Library C# class library and the attached test/demo
//  applications are free software.
//	Software developed by this author is licensed under CPOL 1.02.
//	Some portions of the QRCodeVideoDecoder are licensed under GNU Lesser
//	General Public License v3.0.
//
//	The solution is made of 4 projects:
//	1. QRCodeEncoderDecoderLibrary: QR code encoding and decoding.
//	2. QRCodeEncoderDemo: Create QR Code images.
//	3. QRCodeDecoderDemo: Decode QR code image files.
//	4. QRCodeVideoDecoder: Decode QR code using web camera.
//		This demo program is using some of the source modules of
//		Camera_Net project published at CodeProject.com:
//		https://www.codeproject.com/Articles/671407/Camera_Net-Library
//		and at GitHub: https://github.com/free5lot/Camera_Net.
//		This project is based on DirectShowLib.
//		http://sourceforge.net/projects/directshownet/
//		This project includes a modified subset of the source modules.
//
//	The main points of CPOL 1.02 subject to the terms of the License are:
//
//	Source Code and Executable Files can be used in commercial applications;
//	Source Code and Executable Files can be redistributed; and
//	Source Code can be modified to create derivative works.
//	No claim of suitability, guarantee, or any warranty whatsoever is
//	provided. The software is provided "as-is".
//	The Article accompanying the Work may not be distributed or republished
//	without the Author's consent
//
//	For version history please refer to QRCode.cs
/////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;

namespace Hoard.Utils.QRCodeEncoder
{
/// <summary>
///	Create a QR code image for testing. The image is transformed
///	using perspective algorithm.
/// </summary>
internal class Perspective
	{
	private double CenterX;
	private double CenterY;
	private double CosRot;
	private double SinRot;
	private double CamDist;
	private double CosX;
	private double SinX;
	private double CamVectY;
	private double CamVectZ;
	private double CamPosY;
	private double CamPosZ;

	internal Perspective
			(
			double CenterX,
			double CenterY,
			double ImageRot,
			double CamDist,
			double RotX
			)
		{
		// center position
		this.CenterX = CenterX;
		this.CenterY = CenterY;

		// image rotation
		double RotRad = Math.PI * ImageRot / 180.0;
		CosRot = Math.Cos(RotRad);
		SinRot = Math.Sin(RotRad);
 
		// camera distance from QR Code
		this.CamDist = CamDist;

		// x and z axis rotation constants
		double RotXRad = Math.PI * RotX / 180.0;
		CosX = Math.Cos(RotXRad);
		SinX = Math.Sin(RotXRad);

		// camera vector relative to qr code image
		CamVectY = SinX;
		CamVectZ = CosX;

		// camera position relative to qr code image
		CamPosY =  CamDist * CamVectY;
		CamPosZ =  CamDist * CamVectZ;

		// exit
		return;
		}

	// screen equation
	// CamVectX * X + CamVectY * Y + CamVectZ * Z = 0

	// line equations between QR Code point to camera position
	// X = QRPosX + (CamPosX - QRPosX) * T
	// Y = QRPosY + (CamPosY - QRPosZ) * T
	// Z = QRPosZ + (CamPosZ - QRPosZ) * T

	// line intersection with screen
	// CamVectX * (QRPosX + (CamPosX - QRPosX) * T) +
	//		CamVectY * (QRPosY + (CamPosY - QRPosY) * T) +
	//			CamVectZ * (QRPosZ + (CamPosZ - QRPosZ) * T) = 0
	//
	// (CamVectX * (CamPosX - QRPosX) + CamVectY * (CamPosY - QRPosY) + CamVectZ * (CamPosZ - QRPosZ)) * T =
	//		- CamVectX * QRPosX - CamVectY * QRPosY - CamVectZ * QRPosZ;
	//
	//	T = -(CamVectX * QRPosX + CamVectY * QRPosY + CamVectZ * QRPosZ) /
	//		(CamVectX * (CamPosX - QRPosX) + CamVectY * (CamPosY - QRPosY) + CamVectZ * (CamPosY - QRPosZ));
	//	Q = CamVectX * QRPosX + CamVectY * QRPosY + CamVectZ * QRPosZ
	//	T = Q / (Q - CamDist)

	internal PointF ScreenPosition
			(
			double QRPosX,
			double QRPosY
			)
		{
		// rotation
		double PosX = CosRot * QRPosX - SinRot * QRPosY;
		double PosY = SinRot * QRPosX + CosRot * QRPosY;

		// temp values for intersection calclulation
		double CamQR = CamVectY * PosY;
		double T = CamQR / (CamQR - CamDist);

		// screen position relative to screen center
		double ScrnPosX = CenterX + PosX * (1 - T);
		double TempPosY = PosY + (CamPosY - PosY) * T;
		double TempPosZ = CamPosZ * T; // - ScrnCenterZ;

		// rotate around x axis
		double ScrnPosY = CenterY + TempPosY * CosX - TempPosZ * SinX;

		// program test
		#if DEBUG
		double ScrnPosZ = TempPosY * SinX + TempPosZ * CosX;
		if(Math.Abs(ScrnPosZ) > 0.0001) throw new ApplicationException("Screen Z position must be zero");
		#endif

		return new PointF((float) ScrnPosX, (float) ScrnPosY);
		}

	internal void GetPolygon
			(
			double PosX,
			double PosY,
			double Side,
			PointF[] Polygon
			)
		{
		Polygon[0] = ScreenPosition(PosX, PosY);
		Polygon[1] = ScreenPosition(PosX + Side, PosY);
		Polygon[2] = ScreenPosition(PosX + Side, PosY + Side);
		Polygon[3] = ScreenPosition(PosX, PosY + Side);
		return;
		}
	}
}
