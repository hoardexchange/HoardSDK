/////////////////////////////////////////////////////////////////////
//
//	QR Code Library
//
//	Convert QR Code matrix to Bitmap.
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

//remove this file from here, add a different version that will return a square bool[,] array or rewrite decoder to support varaible width/height
//as it was in original implementation - check if this supports perspective and rotation

#if TODO
using System;

namespace Hoard.Utils.QRCodeEncoder
{
    public enum ErrorSpotControl
    {
        None,
        White,
        Black,
        Alternate,
    }

    /// <summary>
    /// Convert QR code bool array to Bitmap image.
    /// </summary>
    public static class QRBitmapTools
    {
        // white and black brushes
        private static SolidBrush BrushWhite = new SolidBrush(Color.White);
        private static SolidBrush BrushBlack = new SolidBrush(Color.Black);

        /// <summary>
        /// Create Bitmap image from QR code array
        /// </summary>
        /// <param name="QRCode">QR Code</param>
        /// <param name="ModuleSize">Module size in pixels</param>
        /// <param name="QuietZone">Quiet zone in pixels</param>
        /// <returns>Bitmap image of QR code</returns>
        public static Bitmap CreateBitmap
                (
                QRCode QRCode,
                int ModuleSize,
                int QuietZone
                )
        {
            // qr code bitmap dimension
            int BitmapDimension = QRCode.QRCodeImageDimension(ModuleSize, QuietZone);

            // create picture object and make it white
            Bitmap ImageBitmap = new Bitmap(BitmapDimension, BitmapDimension);
            Graphics Graphics = Graphics.FromImage(ImageBitmap);
            Graphics.FillRectangle(BrushWhite, 0, 0, BitmapDimension, BitmapDimension);

            // shortcut
            int Dimension = QRCode.QRCodeDimension;
            bool[,] Code = QRCode.QRCodeMatrix;
            int PosX = QuietZone;
            int PosY = QuietZone;

            // paint QR Code image
            for (int Row = 0; Row < Dimension; Row++)
            {
                for (int Col = 0; Col < Dimension; Col++)
                {
                    if (Code[Row, Col]) Graphics.FillRectangle(BrushBlack, PosX, PosY, ModuleSize, ModuleSize);
                    PosX += ModuleSize;
                }
                PosX = QuietZone;
                PosY += ModuleSize;
            }

            // return bitmap
            return ImageBitmap;
        }

        /// <summary>
        /// Create Bitmap image from QR code array over given solid or hatch background
        /// </summary>
        /// <param name="QRCode">QR Code</param>
        /// <param name="ModuleSize">Module size in pixels</param>
        /// <param name="QuietZone">Quiet zone in pixels</param>
        /// <param name="Background">SolidBrush or HatchBrush background</param>
        /// <param name="ImageWidth">Output image width</param>
        /// <param name="ImageHeight">Output image height</param>
        /// <param name="QRCodeCenterPosX">QR code center X position</param>
        /// <param name="QRCodeCenterPosY">QR code center Y position</param>
        /// <param name="Rotation">QR code rotation in degrees</param>
        /// <returns>Bitmap image of QR code</returns>
        public static Bitmap CreateBitmap
                (
                QRCode QRCode,
                int ModuleSize,
                int QuietZone,
                Brush Background,
                int ImageWidth,
                int ImageHeight,
                double QRCodeCenterPosX,
                double QRCodeCenterPosY,
                double Rotation
                )
        {
            // create background bitmap and and paint it with the brush
            Bitmap BackgroundBitmap = new Bitmap(ImageWidth, ImageHeight);
            Graphics Graphics = Graphics.FromImage(BackgroundBitmap);
            Graphics.FillRectangle(Background, 0, 0, ImageWidth, ImageHeight);

            // return QR code bitmap painted over background
            return CreateBitmap(QRCode, ModuleSize, QuietZone, BackgroundBitmap, QRCodeCenterPosX, QRCodeCenterPosY, Rotation);
        }

        /// <summary>
        /// Create Bitmap image from QR code array over given solid or hatch background
        /// </summary>
        /// <param name="QRCode">QR Code</param>
        /// <param name="ModuleSize">Module size in pixels</param>
        /// <param name="QuietZone">Quiet zone in pixels</param>
        /// <param name="Background">Bitmap image background</param>
        /// <param name="QRCodeCenterPosX">QR code center X position</param>
        /// <param name="QRCodeCenterPosY">QR code center Y position</param>
        /// <param name="Rotation">QR code rotation in degrees</param>
        /// <returns>Bitmap image of QR code</returns>
        public static Bitmap CreateBitmap
                (
                QRCode QRCode,
                int ModuleSize,
                int QuietZone,
                Bitmap Background,
                double QRCodeCenterPosX,
                double QRCodeCenterPosY,
                double Rotation
                )
        {
            // qr code bitmap dimension
            int QRCodeImageDimension = QRCode.QRCodeImageDimension(ModuleSize, QuietZone);

            // transformation matrix
            Matrix Matrix = new Matrix();
            Matrix.Translate((float)QRCodeCenterPosX, (float)QRCodeCenterPosY);
            if (Rotation != 0) Matrix.Rotate((float)Rotation);

            // create a copy of the background
            Bitmap OutputImage = new Bitmap(Background);

            // create graphics object
            Graphics Graphics = Graphics.FromImage(OutputImage);

            // attach transformation matrix
            Graphics.Transform = Matrix;

            // QRCode top left corner position relative to QR code centre
            double QRCodePos = -0.5 * QRCodeImageDimension;

            // clear the area for qr code
            Graphics.FillRectangle(BrushWhite, (float)QRCodePos, (float)QRCodePos, QRCodeImageDimension, QRCodeImageDimension);

            // shortcut
            int Dimension = QRCode.QRCodeDimension;
            bool[,] Code = QRCode.QRCodeMatrix;

            // add quite zone
            QRCodePos += QuietZone;

            // paint QR Code image
            for (int Row = 0; Row < Dimension; Row++)
                for (int Col = 0; Col < Dimension; Col++)
                {
                    if (Code[Row, Col]) Graphics.FillRectangle(BrushBlack, (float)(QRCodePos + Col * ModuleSize),
                         (float)(QRCodePos + Row * ModuleSize), ModuleSize, ModuleSize);
                }

            return OutputImage;
        }

        /// <summary>
        /// Create Bitmap image from QR code array over given solid or hatch background
        /// </summary>
        /// <param name="QRCode">QR Code</param>
        /// <param name="ModuleSize">Module size in pixels</param>
        /// <param name="QuietZone">Quiet zone in pixels</param>
        /// <param name="Background">Bitmap image background</param>
        /// <param name="QRCodeCenterPosX">QR code center X position</param>
        /// <param name="QRCodeCenterPosY">QR code center Y position</param>
        /// <param name="Rotation">QR code rotation in degrees</param>
        /// <param name="CameraDistance">Perspective camera distance in pixels</param>
        /// <param name="ViewXRotation">Camera view X axis rotation in degrees</param>
        /// <returns>Bitmap image of QR code</returns>
        public static Bitmap CreateBitmap
                (
                QRCode QRCode,
                int ModuleSize,
                int QuietZone,
                Bitmap Background,
                double QRCodeCenterPosX,
                double QRCodeCenterPosY,
                double Rotation,
                double CameraDistance,
                double ViewXRotation
                )
        {
            // create perspective object
            Perspective Perspective = new Perspective(QRCodeCenterPosX, QRCodeCenterPosY, Rotation, CameraDistance, ViewXRotation);

            // create graphics object
            Graphics Graphics = Graphics.FromImage(Background);

            // qr code bitmap dimension
            int QRCodeImageDimension = QRCode.QRCodeImageDimension(ModuleSize, QuietZone);

            // half image dimension
            double HalfDimension = 0.5 * QRCodeImageDimension;

            // polygon
            PointF[] Polygon = new PointF[4];
            Perspective.GetPolygon(-HalfDimension, -HalfDimension, QRCodeImageDimension, Polygon);

            // clear the area for qr code
            Graphics.FillPolygon(BrushWhite, Polygon);

            // shortcut
            int Dimension = QRCode.QRCodeDimension;
            bool[,] Code = QRCode.QRCodeMatrix;

            // add quiet zone
            double QRCodePos = -HalfDimension + QuietZone;

            // paint QR Code image
            for (int Row = 0; Row < Dimension; Row++)
                for (int Col = 0; Col < Dimension; Col++)
                {
                    if (Code[Row, Col])
                    {
                        Perspective.GetPolygon(QRCodePos + Col * ModuleSize, QRCodePos + Row * ModuleSize, ModuleSize, Polygon);
                        Graphics.FillPolygon(BrushBlack, Polygon);
                    }
                }

            return Background;
        }

        /// <summary>
        /// Add error spots for testing
        /// </summary>
        /// <param name="ImageBitmap">Image Bitmap</param>
        /// <param name="ErrorControl">Error control enumeration</param>
        /// <param name="ErrorDiameter">Error spot diameter</param>
        /// <param name="ErrorSpotsCount">Error spots count</param>
        public static void AddErrorSpots
                (
                Bitmap ImageBitmap,
                ErrorSpotControl ErrorControl,
                double ErrorDiameter,
                double ErrorSpotsCount
                )
        {
            // random number generator
            Random RandNum = new Random();

            // create graphics object
            Graphics Graphics = Graphics.FromImage(ImageBitmap);

            double XRange = ImageBitmap.Width - ErrorDiameter - 4;
            double YRange = ImageBitmap.Height - ErrorDiameter - 4;
            SolidBrush SpotBrush = ErrorControl == ErrorSpotControl.Black ? BrushBlack : BrushWhite;

            for (int Index = 0; Index < ErrorSpotsCount; Index++)
            {
                double XPos = RandNum.NextDouble() * XRange;
                double YPos = RandNum.NextDouble() * YRange;
                if (ErrorControl == ErrorSpotControl.Alternate) SpotBrush = (Index & 1) == 0 ? BrushWhite : BrushBlack;
                Graphics.FillEllipse(SpotBrush, (float)XPos, (float)YPos, (float)ErrorDiameter, (float)ErrorDiameter);
            }
            return;
        }

        ////////////////////////////////////////////////////////////////////
        // Convert image to black and white boolean matrix
        ////////////////////////////////////////////////////////////////////

        internal bool[,] ConvertImageToBlackAndWhite
                (
                Bitmap InputImage
                )
        {
            // lock image bits
            BitmapData BitmapData = InputImage.LockBits(new Rectangle(0, 0, ImageWidth, ImageHeight),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            // address of first line
            IntPtr BitArrayPtr = BitmapData.Scan0;

            // length in bytes of one scan line
            int ScanLineWidth = BitmapData.Stride;
            if (ScanLineWidth < 0)
            {
                return false;
            }

            // image total bytes
            int TotalBytes = ScanLineWidth * ImageHeight;
            byte[] BitmapArray = new byte[TotalBytes];

            // Copy the RGB values into the array.
            Marshal.Copy(BitArrayPtr, BitmapArray, 0, TotalBytes);

            // unlock image
            InputImage.UnlockBits(BitmapData);

            // allocate gray image 
            byte[,] GrayImage = new byte[ImageHeight, ImageWidth];
            int[] GrayLevel = new int[256];

            // convert to gray
            int Delta = ScanLineWidth - 3 * ImageWidth;
            int BitmapPtr = 0;
            for (int Row = 0; Row < ImageHeight; Row++)
            {
                for (int Col = 0; Col < ImageWidth; Col++)
                {
                    int Module = (30 * BitmapArray[BitmapPtr] + 59 * BitmapArray[BitmapPtr + 1] + 11 * BitmapArray[BitmapPtr + 2]) / 100;
                    GrayLevel[Module]++;
                    GrayImage[Row, Col] = (byte)Module;
                    BitmapPtr += 3;
                }
                BitmapPtr += Delta;
            }

            // gray level cutoff between black and white
            int LevelStart;
            int LevelEnd;
            for (LevelStart = 0; LevelStart < 256 && GrayLevel[LevelStart] == 0; LevelStart++) ;
            for (LevelEnd = 255; LevelEnd >= LevelStart && GrayLevel[LevelEnd] == 0; LevelEnd--) ;
            LevelEnd++;
            if (LevelEnd - LevelStart < 2)
            {
                return false;
            }

            int CutoffLevel = (LevelStart + LevelEnd) / 2;

            // create boolean image white = false, black = true
            BlackWhiteImage = new bool[ImageHeight, ImageWidth];
            for (int Row = 0; Row < ImageHeight; Row++)
                for (int Col = 0; Col < ImageWidth; Col++)
                    BlackWhiteImage[Row, Col] = GrayImage[Row, Col] < CutoffLevel;


            // exit;
            return true;
        }

        ////////////////////////////////////////////////////////////////////
        // Decode QRCode boolean matrix
        ////////////////////////////////////////////////////////////////////

        public byte[,] ImageDecoder
                (
                Bitmap InputImage
                )
        {
            try
            {
                // empty data string output
                DataArrayList = new List<byte[]>();

                // save image dimension
                ImageWidth = InputImage.Width;
                ImageHeight = InputImage.Height;


                // convert input image to black and white boolean image
                if (!ConvertImageToBlackAndWhite(InputImage)) return null;


                // horizontal search for finders
                if (!HorizontalFindersSearch()) return null;


                // vertical search for finders
                VerticalFindersSearch();


                // remove unused finders
                if (!RemoveUnusedFinders()) return null;

            }

		catch
			{
			return null;
			}

            // look for all possible 3 finder patterns
            int Index1End = FinderList.Count - 2;
            int Index2End = FinderList.Count - 1;
            int Index3End = FinderList.Count;
            for (int Index1 = 0; Index1 < Index1End; Index1++)
                for (int Index2 = Index1 + 1; Index2 < Index2End; Index2++)
                    for (int Index3 = Index2 + 1; Index3 < Index3End; Index3++)
                    {
                        try
                        {
                            // find 3 finders arranged in L shape
                            Corner Corner = Corner.CreateCorner(FinderList[Index1], FinderList[Index2], FinderList[Index3]);

                            // not a valid corner
                            if (Corner == null) continue;
                            
                            // get corner info (version, error code and mask)
                            // continue if failed
                            if (!GetQRCodeCornerInfo(Corner)) continue;


                            // decode corner using three finders
                            // continue if successful
                            if (DecodeQRCodeCorner(Corner)) continue;

                            // qr code version 1 has no alignment mark
                            // in other words decode failed 
                            if (QRCodeVersion == 1) continue;

                            // find bottom right alignment mark
                            // continue if failed
                            if (!FindAlignmentMark(Corner)) continue;

                            // decode using 4 points
                            foreach (Finder Align in AlignList)
                            {

                                // calculate transformation based on 3 finders and bottom right alignment mark
                                SetTransMatrix(Corner, Align.Row, Align.Col);

                                // decode corner using three finders and one alignment mark
                                if (DecodeQRCodeCorner(Corner)) break;
                            }
                        }
			catch
				{
				continue;
				}
                    }

            // not found exit
            if (DataArrayList.Count == 0)
            {
                return null;
            }

            // successful exit
            return DataArrayList.ToArray();
        }
    }
}
#endif