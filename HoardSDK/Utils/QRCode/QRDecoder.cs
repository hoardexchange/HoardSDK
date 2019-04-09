/////////////////////////////////////////////////////////////////////
//
//	QR Code Library
//
//	QR Code decoder.
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

using Hoard;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hoard.Utils.QRCodeEncoder
{
    public class QRDecoder : QRCode
    {
        internal List<Finder> FinderList;
        internal List<Finder> AlignList;
        internal List<byte[]> DataArrayList;

        internal bool Trans4Mode;

        // transformation cooefficients from QR modules to image pixels
        internal double Trans3a;
        internal double Trans3b;
        internal double Trans3c;
        internal double Trans3d;
        internal double Trans3e;
        internal double Trans3f;

        // transformation matrix based on three finders plus one more point
        internal double Trans4a;
        internal double Trans4b;
        internal double Trans4c;
        internal double Trans4d;
        internal double Trans4e;
        internal double Trans4f;
        internal double Trans4g;
        internal double Trans4h;

        internal const double SIGNATURE_MAX_DEVIATION = 0.25;
        internal const double HOR_VERT_SCAN_MAX_DISTANCE = 2.0;
        internal const double MODULE_SIZE_DEVIATION = 0.5; // 0.75;
        internal const double CORNER_SIDE_LENGTH_DEV = 0.8;
        internal const double CORNER_RIGHT_ANGLE_DEV = 0.25; // about Sin(4 deg)
        internal const double ALIGNMENT_SEARCH_AREA = 0.3;

        ////////////////////////////////////////////////////////////////////
        // constructors
        ////////////////////////////////////////////////////////////////////

        public QRDecoder() { }

        ////////////////////////////////////////////////////////////////////
        // Decode QRCode boolean matrix
        ////////////////////////////////////////////////////////////////////

        public byte[][] ImageDecoder
                (
                bool[,] qrCode
                )
        {
            int dim = qrCode.Rank;
            for(int i=0;i<dim;++i)
            {
                if (qrCode.GetLength(i) != dim)
                {
                    ErrorCallbackProvider.ReportError("qrCode parameter is not a square matrix");
                    return null;
                }
            }
            QRCodeMatrix = qrCode;
            QRCodeDimension = dim;

            // empty data string output
            DataArrayList = new List<byte[]>();

            // horizontal search for finders
            if (!HorizontalFindersSearch()) return null;

            // vertical search for finders
            VerticalFindersSearch();

            // remove unused finders
            if (!RemoveUnusedFinders()) return null;

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

        /// <summary>
        /// Format result for display
        /// </summary>
        /// <param name="DataByteArray"></param>
        /// <returns></returns>
        public static string QRCodeResult
                (
                byte[][] DataByteArray
                )
        {
            // no QR code
            if (DataByteArray == null) return string.Empty;

            // image has one QR code
            if (DataByteArray.Length == 1) return ByteArrayToStr(DataByteArray[0]);

            // image has more than one QR code
            StringBuilder Str = new StringBuilder();
            for (int Index = 0; Index < DataByteArray.Length; Index++)
            {
                if (Index != 0) Str.Append("\r\n");
                Str.AppendFormat("QR Code {0}\r\n", Index + 1);
                Str.Append(ByteArrayToStr(DataByteArray[Index]));
            }
            return Str.ToString();
        }


        ////////////////////////////////////////////////////////////////////
        // search row by row for finders blocks
        ////////////////////////////////////////////////////////////////////

        internal bool HorizontalFindersSearch()
        {
            // create empty finders list
            FinderList = new List<Finder>();

            // look for finder patterns
            int[] ColPos = new int[QRCodeDimension + 1];
            int PosPtr = 0;

            // scan one row at a time
            for (int Row = 0; Row < QRCodeDimension; Row++)
            {
                // look for first black pixel
                int Col;
                for (Col = 0; Col < QRCodeDimension && !QRCodeMatrix[Row, Col]; Col++) ;
                if (Col == QRCodeDimension) continue;

                // first black
                PosPtr = 0;
                ColPos[PosPtr++] = Col;

                // loop for pairs
                for (; ; )
                {
                    // look for next white
                    // if black is all the way to the edge, set next white after the edge
                    for (; Col < QRCodeDimension && QRCodeMatrix[Row, Col]; Col++) ;
                    ColPos[PosPtr++] = Col;
                    if (Col == QRCodeDimension) break;

                    // look for next black
                    for (; Col < QRCodeDimension && !QRCodeMatrix[Row, Col]; Col++) ;
                    if (Col == QRCodeDimension) break;
                    ColPos[PosPtr++] = Col;
                }

                // we must have at least 6 positions
                if (PosPtr < 6) continue;

                // build length array
                int PosLen = PosPtr - 1;
                int[] Len = new int[PosLen];
                for (int Ptr = 0; Ptr < PosLen; Ptr++) Len[Ptr] = ColPos[Ptr + 1] - ColPos[Ptr];

                // test signature
                int SigLen = PosPtr - 5;
                for (int SigPtr = 0; SigPtr < SigLen; SigPtr += 2)
                {
                    if (TestFinderSig(ColPos, Len, SigPtr, out double ModuleSize))
                        FinderList.Add(new Finder(Row, ColPos[SigPtr + 2], ColPos[SigPtr + 3], ModuleSize));
                }
            }

            // no finders found
            if (FinderList.Count < 3)
            {
                return false;
            }

            // exit
            return true;
        }

        ////////////////////////////////////////////////////////////////////
        // search row by row for alignment blocks
        ////////////////////////////////////////////////////////////////////

        internal bool HorizontalAlignmentSearch
                (
                int AreaLeft,
                int AreaTop,
                int AreaWidth,
                int AreaHeight
                )
        {
            // create empty finders list
            AlignList = new List<Finder>();

            // look for finder patterns
            int[] ColPos = new int[AreaWidth + 1];
            int PosPtr = 0;

            // area right and bottom
            int AreaRight = AreaLeft + AreaWidth;
            int AreaBottom = AreaTop + AreaHeight;

            // scan one row at a time
            for (int Row = AreaTop; Row < AreaBottom; Row++)
            {
                // look for first black pixel
                int Col;
                for (Col = AreaLeft; Col < AreaRight && !QRCodeMatrix[Row, Col]; Col++) ;
                if (Col == AreaRight) continue;

                // first black
                PosPtr = 0;
                ColPos[PosPtr++] = Col;

                // loop for pairs
                for (; ; )
                {
                    // look for next white
                    // if black is all the way to the edge, set next white after the edge
                    for (; Col < AreaRight && QRCodeMatrix[Row, Col]; Col++) ;
                    ColPos[PosPtr++] = Col;
                    if (Col == AreaRight) break;

                    // look for next black
                    for (; Col < AreaRight && !QRCodeMatrix[Row, Col]; Col++) ;
                    if (Col == AreaRight) break;
                    ColPos[PosPtr++] = Col;
                }

                // we must have at least 6 positions
                if (PosPtr < 6) continue;

                // build length array
                int PosLen = PosPtr - 1;
                int[] Len = new int[PosLen];
                for (int Ptr = 0; Ptr < PosLen; Ptr++) Len[Ptr] = ColPos[Ptr + 1] - ColPos[Ptr];

                // test signature
                int SigLen = PosPtr - 5;
                for (int SigPtr = 0; SigPtr < SigLen; SigPtr += 2)
                {
                    if (TestAlignSig(ColPos, Len, SigPtr, out double ModuleSize))
                        AlignList.Add(new Finder(Row, ColPos[SigPtr + 2], ColPos[SigPtr + 3], ModuleSize));
                }
            }

            // exit
            return AlignList.Count != 0;
        }

        ////////////////////////////////////////////////////////////////////
        // search column by column for finders blocks
        ////////////////////////////////////////////////////////////////////

        internal void VerticalFindersSearch()
        {
            // active columns
            bool[] ActiveColumn = new bool[QRCodeDimension];
            foreach (Finder HF in FinderList)
            {
                for (int Col = HF.Col1; Col < HF.Col2; Col++) ActiveColumn[Col] = true;
            }

            // look for finder patterns
            int[] RowPos = new int[QRCodeDimension + 1];
            int PosPtr = 0;

            // scan one column at a time
            for (int Col = 0; Col < QRCodeDimension; Col++)
            {
                // not active column
                if (!ActiveColumn[Col]) continue;

                // look for first black pixel
                int Row;
                for (Row = 0; Row < QRCodeDimension && !QRCodeMatrix[Row, Col]; Row++) ;
                if (Row == QRCodeDimension) continue;

                // first black
                PosPtr = 0;
                RowPos[PosPtr++] = Row;

                // loop for pairs
                for (; ; )
                {
                    // look for next white
                    // if black is all the way to the edge, set next white after the edge
                    for (; Row < QRCodeDimension && QRCodeMatrix[Row, Col]; Row++) ;
                    RowPos[PosPtr++] = Row;
                    if (Row == QRCodeDimension) break;

                    // look for next black
                    for (; Row < QRCodeDimension && !QRCodeMatrix[Row, Col]; Row++) ;
                    if (Row == QRCodeDimension) break;
                    RowPos[PosPtr++] = Row;
                }

                // we must have at least 6 positions
                if (PosPtr < 6) continue;

                // build length array
                int PosLen = PosPtr - 1;
                int[] Len = new int[PosLen];
                for (int Ptr = 0; Ptr < PosLen; Ptr++) Len[Ptr] = RowPos[Ptr + 1] - RowPos[Ptr];

                // test signature
                int SigLen = PosPtr - 5;
                for (int SigPtr = 0; SigPtr < SigLen; SigPtr += 2)
                {
                    if (!TestFinderSig(RowPos, Len, SigPtr, out double ModuleSize)) continue;
                    foreach (Finder HF in FinderList)
                    {
                        HF.Match(Col, RowPos[SigPtr + 2], RowPos[SigPtr + 3], ModuleSize);
                    }
                }
            }

            // exit
            return;
        }

        ////////////////////////////////////////////////////////////////////
        // search column by column for finders blocks
        ////////////////////////////////////////////////////////////////////

        internal void VerticalAlignmentSearch
                (
                int AreaLeft,
                int AreaTop,
                int AreaWidth,
                int AreaHeight
                )
        {
            // active columns
            bool[] ActiveColumn = new bool[AreaWidth];
            foreach (Finder HF in AlignList)
            {
                for (int Col = HF.Col1; Col < HF.Col2; Col++) ActiveColumn[Col - AreaLeft] = true;
            }

            // look for finder patterns
            int[] RowPos = new int[AreaHeight + 1];
            int PosPtr = 0;

            // area right and bottom
            int AreaRight = AreaLeft + AreaWidth;
            int AreaBottom = AreaTop + AreaHeight;

            // scan one column at a time
            for (int Col = AreaLeft; Col < AreaRight; Col++)
            {
                // not active column
                if (!ActiveColumn[Col - AreaLeft]) continue;

                // look for first black pixel
                int Row;
                for (Row = AreaTop; Row < AreaBottom && !QRCodeMatrix[Row, Col]; Row++) ;
                if (Row == AreaBottom) continue;

                // first black
                PosPtr = 0;
                RowPos[PosPtr++] = Row;

                // loop for pairs
                for (; ; )
                {
                    // look for next white
                    // if black is all the way to the edge, set next white after the edge
                    for (; Row < AreaBottom && QRCodeMatrix[Row, Col]; Row++) ;
                    RowPos[PosPtr++] = Row;
                    if (Row == AreaBottom) break;

                    // look for next black
                    for (; Row < AreaBottom && !QRCodeMatrix[Row, Col]; Row++) ;
                    if (Row == AreaBottom) break;
                    RowPos[PosPtr++] = Row;
                }

                // we must have at least 6 positions
                if (PosPtr < 6) continue;

                // build length array
                int PosLen = PosPtr - 1;
                int[] Len = new int[PosLen];
                for (int Ptr = 0; Ptr < PosLen; Ptr++) Len[Ptr] = RowPos[Ptr + 1] - RowPos[Ptr];

                // test signature
                int SigLen = PosPtr - 5;
                for (int SigPtr = 0; SigPtr < SigLen; SigPtr += 2)
                {
                    if (!TestAlignSig(RowPos, Len, SigPtr, out double ModuleSize)) continue;
                    foreach (Finder HF in AlignList)
                    {
                        HF.Match(Col, RowPos[SigPtr + 2], RowPos[SigPtr + 3], ModuleSize);
                    }
                }
            }

            // exit
            return;
        }

        ////////////////////////////////////////////////////////////////////
        // search column by column for finders blocks
        ////////////////////////////////////////////////////////////////////

        internal bool RemoveUnusedFinders()
        {
            // remove all entries without a match
            for (int Index = 0; Index < FinderList.Count; Index++)
            {
                if (FinderList[Index].Distance == double.MaxValue)
                {
                    FinderList.RemoveAt(Index);
                    Index--;
                }
            }

            // list is now empty or has less than three finders
            if (FinderList.Count < 3)
            {
                return false;
            }

            // keep best entry for each overlapping area
            for (int Index = 0; Index < FinderList.Count; Index++)
            {
                Finder Finder = FinderList[Index];
                for (int Index1 = Index + 1; Index1 < FinderList.Count; Index1++)
                {
                    Finder Finder1 = FinderList[Index1];
                    if (!Finder.Overlap(Finder1)) continue;
                    if (Finder1.Distance < Finder.Distance)
                    {
                        Finder = Finder1;
                        FinderList[Index] = Finder;
                    }
                    FinderList.RemoveAt(Index1);
                    Index1--;
                }
            }

            // list is now empty or has less than three finders
            if (FinderList.Count < 3)
            {
                return false;
            }

            // exit
            return true;
        }

        ////////////////////////////////////////////////////////////////////
        // search column by column for finders blocks
        ////////////////////////////////////////////////////////////////////

        internal bool RemoveUnusedAlignMarks()
        {
            // remove all entries without a match
            for (int Index = 0; Index < AlignList.Count; Index++)
            {
                if (AlignList[Index].Distance == double.MaxValue)
                {
                    AlignList.RemoveAt(Index);
                    Index--;
                }
            }

            // keep best entry for each overlapping area
            for (int Index = 0; Index < AlignList.Count; Index++)
            {
                Finder Finder = AlignList[Index];
                for (int Index1 = Index + 1; Index1 < AlignList.Count; Index1++)
                {
                    Finder Finder1 = AlignList[Index1];
                    if (!Finder.Overlap(Finder1)) continue;
                    if (Finder1.Distance < Finder.Distance)
                    {
                        Finder = Finder1;
                        AlignList[Index] = Finder;
                    }
                    AlignList.RemoveAt(Index1);
                    Index1--;
                }
            }

            // exit
            return AlignList.Count != 0;
        }

        ////////////////////////////////////////////////////////////////////
        // test finder signature 1 1 3 1 1
        ////////////////////////////////////////////////////////////////////

        internal bool TestFinderSig
                (
                int[] Pos,
                int[] Len,
                int Index,
                out double Module
                )
        {
            Module = (Pos[Index + 5] - Pos[Index]) / 7.0;
            double MaxDev = SIGNATURE_MAX_DEVIATION * Module;
            if (Math.Abs(Len[Index] - Module) > MaxDev) return false;
            if (Math.Abs(Len[Index + 1] - Module) > MaxDev) return false;
            if (Math.Abs(Len[Index + 2] - 3 * Module) > MaxDev) return false;
            if (Math.Abs(Len[Index + 3] - Module) > MaxDev) return false;
            if (Math.Abs(Len[Index + 4] - Module) > MaxDev) return false;
            return true;
        }

        ////////////////////////////////////////////////////////////////////
        // test alignment signature n 1 1 1 n
        ////////////////////////////////////////////////////////////////////

        internal bool TestAlignSig
                (
                int[] Pos,
                int[] Len,
                int Index,
                out double Module
                )
        {
            Module = (Pos[Index + 4] - Pos[Index + 1]) / 3.0;
            double MaxDev = SIGNATURE_MAX_DEVIATION * Module;
            if (Len[Index] < Module - MaxDev) return false;
            if (Math.Abs(Len[Index + 1] - Module) > MaxDev) return false;
            if (Math.Abs(Len[Index + 2] - Module) > MaxDev) return false;
            if (Math.Abs(Len[Index + 3] - Module) > MaxDev) return false;
            if (Len[Index + 4] < Module - MaxDev) return false;
            return true;
        }

        ////////////////////////////////////////////////////////////////////
        // Build corner list
        ////////////////////////////////////////////////////////////////////

        internal List<Corner> BuildCornerList()
        {
            // empty list
            List<Corner> Corners = new List<Corner>();

            // look for all possible 3 finder patterns
            int Index1End = FinderList.Count - 2;
            int Index2End = FinderList.Count - 1;
            int Index3End = FinderList.Count;
            for (int Index1 = 0; Index1 < Index1End; Index1++)
                for (int Index2 = Index1 + 1; Index2 < Index2End; Index2++)
                    for (int Index3 = Index2 + 1; Index3 < Index3End; Index3++)
                    {
                        // find 3 finders arranged in L shape
                        Corner Corner = Corner.CreateCorner(FinderList[Index1], FinderList[Index2], FinderList[Index3]);

                        // add corner to list
                        if (Corner != null) Corners.Add(Corner);
                    }

            // exit
            return Corners.Count == 0 ? null : Corners;
        }

        ////////////////////////////////////////////////////////////////////
        // Get QR Code corner info
        ////////////////////////////////////////////////////////////////////

        internal bool GetQRCodeCornerInfo
                (
                Corner Corner
                )
        {
            try
            {
                // initial version number
                QRCodeVersion = Corner.InitialVersionNumber();

                // qr code dimension
                QRCodeDimension = 17 + 4 * QRCodeVersion;

                // set transformation matrix
                SetTransMatrix(Corner);

                // if version number is 7 or more, get version code
                if (QRCodeVersion >= 7)
                {
                    int Version = GetVersionOne();
                    if (Version == 0)
                    {
                        Version = GetVersionTwo();
                        if (Version == 0) return false;
                    }

                    // QR Code version number is different than initial version
                    if (Version != QRCodeVersion)
                    {
                        // initial version number and dimension
                        QRCodeVersion = Version;

                        // qr code dimension
                        QRCodeDimension = 17 + 4 * QRCodeVersion;

                        // set transformation matrix
                        SetTransMatrix(Corner);
                    }
                }

                // get format info arrays
                int FormatInfo = GetFormatInfoOne();
                if (FormatInfo < 0)
                {
                    FormatInfo = GetFormatInfoTwo();
                    if (FormatInfo < 0) return false;
                }

                // set error correction code and mask code
                ErrorCorrection = FormatInfoToErrCode(FormatInfo >> 3);
                MaskCode = FormatInfo & 7;

                // successful exit
                return true;
            }
		catch
			{
			// failed exit
			return false;
			}
        }

        ////////////////////////////////////////////////////////////////////
        // Search for QR Code version
        ////////////////////////////////////////////////////////////////////

        internal bool DecodeQRCodeCorner
                (
                Corner Corner
                )
        {
            try
            {
                // create base matrix
                BuildBaseMatrix();

                // create data matrix and test fixed modules
                ConvertImageToMatrix();

                // based on version and format information
                // set number of data and error correction codewords length  
                SetDataCodewordsLength();

                // apply mask as per get format information step
                ApplyMask(MaskCode);

                // unload data from binary matrix to byte format
                UnloadDataFromMatrix();

                // restore blocks (undo interleave)
                RestoreBlocks();

                // calculate error correction
                // in case of error try to correct it
                CalculateErrorCorrection();

                // decode data
                byte[] DataArray = DecodeData();
                DataArrayList.Add(DataArray);
                
                // successful exit
                return true;
            }
		catch
			{
			// failed exit
			return false;
			}
        }

        internal void SetTransMatrix
                (
                Corner Corner
                )
        {
            // save
            int BottomRightPos = QRCodeDimension - 4;

            // transformation matrix based on three finders
            double[,] Matrix1 = new double[3, 4];
            double[,] Matrix2 = new double[3, 4];

            // build matrix 1 for horizontal X direction
            Matrix1[0, 0] = 3;
            Matrix1[0, 1] = 3;
            Matrix1[0, 2] = 1;
            Matrix1[0, 3] = Corner.TopLeftFinder.Col;

            Matrix1[1, 0] = BottomRightPos;
            Matrix1[1, 1] = 3;
            Matrix1[1, 2] = 1;
            Matrix1[1, 3] = Corner.TopRightFinder.Col;

            Matrix1[2, 0] = 3;
            Matrix1[2, 1] = BottomRightPos;
            Matrix1[2, 2] = 1;
            Matrix1[2, 3] = Corner.BottomLeftFinder.Col;

            // build matrix 2 for Vertical Y direction
            Matrix2[0, 0] = 3;
            Matrix2[0, 1] = 3;
            Matrix2[0, 2] = 1;
            Matrix2[0, 3] = Corner.TopLeftFinder.Row;

            Matrix2[1, 0] = BottomRightPos;
            Matrix2[1, 1] = 3;
            Matrix2[1, 2] = 1;
            Matrix2[1, 3] = Corner.TopRightFinder.Row;

            Matrix2[2, 0] = 3;
            Matrix2[2, 1] = BottomRightPos;
            Matrix2[2, 2] = 1;
            Matrix2[2, 3] = Corner.BottomLeftFinder.Row;

            // solve matrix1
            SolveMatrixOne(Matrix1);
            Trans3a = Matrix1[0, 3];
            Trans3c = Matrix1[1, 3];
            Trans3e = Matrix1[2, 3];

            // solve matrix2
            SolveMatrixOne(Matrix2);
            Trans3b = Matrix2[0, 3];
            Trans3d = Matrix2[1, 3];
            Trans3f = Matrix2[2, 3];

            // reset trans 4 mode
            Trans4Mode = false;
            return;
        }

        internal void SolveMatrixOne
                (
                double[,] Matrix
                )
        {
            for (int Row = 0; Row < 3; Row++)
            {
                // If the element is zero, make it non zero by adding another row
                if (Matrix[Row, Row] == 0)
                {
                    int Row1;
                    for (Row1 = Row + 1; Row1 < 3 && Matrix[Row1, Row] == 0; Row1++) ;
                    if (Row1 == 3) throw new ApplicationException("Solve linear equations failed");

                    for (int Col = Row; Col < 4; Col++) Matrix[Row, Col] += Matrix[Row1, Col];
                }

                // make the diagonal element 1.0
                for (int Col = 3; Col > Row; Col--) Matrix[Row, Col] /= Matrix[Row, Row];

                // subtract current row from next rows to eliminate one value
                for (int Row1 = Row + 1; Row1 < 3; Row1++)
                {
                    for (int Col = 3; Col > Row; Col--) Matrix[Row1, Col] -= Matrix[Row, Col] * Matrix[Row1, Row];
                }
            }

            // go up from last row and eliminate all solved values
            Matrix[1, 3] -= Matrix[1, 2] * Matrix[2, 3];
            Matrix[0, 3] -= Matrix[0, 2] * Matrix[2, 3];
            Matrix[0, 3] -= Matrix[0, 1] * Matrix[1, 3];
            return;
        }

        ////////////////////////////////////////////////////////////////////
        // Get image pixel color
        ////////////////////////////////////////////////////////////////////

        internal bool GetModule
                (
                int Row,
                int Col
                )
        {
            // get module based on three finders
            if (!Trans4Mode)
            {
                int Trans3Col = (int)Math.Round(Trans3a * Col + Trans3c * Row + Trans3e, 0, MidpointRounding.AwayFromZero);
                int Trans3Row = (int)Math.Round(Trans3b * Col + Trans3d * Row + Trans3f, 0, MidpointRounding.AwayFromZero);
                return QRCodeMatrix[Trans3Row, Trans3Col];
            }

            // get module based on three finders plus one alignment mark
            double W = Trans4g * Col + Trans4h * Row + 1.0;
            int Trans4Col = (int)Math.Round((Trans4a * Col + Trans4b * Row + Trans4c) / W, 0, MidpointRounding.AwayFromZero);
            int Trans4Row = (int)Math.Round((Trans4d * Col + Trans4e * Row + Trans4f) / W, 0, MidpointRounding.AwayFromZero);
            return QRCodeMatrix[Trans4Row, Trans4Col];
        }

        ////////////////////////////////////////////////////////////////////
        // search row by row for finders blocks
        ////////////////////////////////////////////////////////////////////

        internal bool FindAlignmentMark
                (
                Corner Corner
                )
        {
            // alignment mark estimated position
            int AlignRow = QRCodeDimension - 7;
            int AlignCol = QRCodeDimension - 7;
            int ImageCol = (int)Math.Round(Trans3a * AlignCol + Trans3c * AlignRow + Trans3e, 0, MidpointRounding.AwayFromZero);
            int ImageRow = (int)Math.Round(Trans3b * AlignCol + Trans3d * AlignRow + Trans3f, 0, MidpointRounding.AwayFromZero);
            
            // search area
            int Side = (int)Math.Round(ALIGNMENT_SEARCH_AREA * (Corner.TopLineLength + Corner.LeftLineLength), 0, MidpointRounding.AwayFromZero);

            int AreaLeft = ImageCol - Side / 2;
            int AreaTop = ImageRow - Side / 2;
            int AreaWidth = Side;
            int AreaHeight = Side;
            
            // horizontal search for finders
            if (!HorizontalAlignmentSearch(AreaLeft, AreaTop, AreaWidth, AreaHeight)) return false;

            // vertical search for finders
            VerticalAlignmentSearch(AreaLeft, AreaTop, AreaWidth, AreaHeight);

            // remove unused alignment entries
            if (!RemoveUnusedAlignMarks()) return false;

            // successful exit
            return true;
        }

        internal void SetTransMatrix
                (
                Corner Corner,
                double ImageAlignRow,
                double ImageAlignCol
                )
        {
            // top right and bottom left QR code position
            int FarFinder = QRCodeDimension - 4;
            int FarAlign = QRCodeDimension - 7;

            double[,] Matrix = new double[8, 9];

            Matrix[0, 0] = 3.0;
            Matrix[0, 1] = 3.0;
            Matrix[0, 2] = 1.0;
            Matrix[0, 6] = -3.0 * Corner.TopLeftFinder.Col;
            Matrix[0, 7] = -3.0 * Corner.TopLeftFinder.Col;
            Matrix[0, 8] = Corner.TopLeftFinder.Col;

            Matrix[1, 0] = FarFinder;
            Matrix[1, 1] = 3.0;
            Matrix[1, 2] = 1.0;
            Matrix[1, 6] = -FarFinder * Corner.TopRightFinder.Col;
            Matrix[1, 7] = -3.0 * Corner.TopRightFinder.Col;
            Matrix[1, 8] = Corner.TopRightFinder.Col;

            Matrix[2, 0] = 3.0;
            Matrix[2, 1] = FarFinder;
            Matrix[2, 2] = 1.0;
            Matrix[2, 6] = -3.0 * Corner.BottomLeftFinder.Col;
            Matrix[2, 7] = -FarFinder * Corner.BottomLeftFinder.Col;
            Matrix[2, 8] = Corner.BottomLeftFinder.Col;

            Matrix[3, 0] = FarAlign;
            Matrix[3, 1] = FarAlign;
            Matrix[3, 2] = 1.0;
            Matrix[3, 6] = -FarAlign * ImageAlignCol;
            Matrix[3, 7] = -FarAlign * ImageAlignCol;
            Matrix[3, 8] = ImageAlignCol;

            Matrix[4, 3] = 3.0;
            Matrix[4, 4] = 3.0;
            Matrix[4, 5] = 1.0;
            Matrix[4, 6] = -3.0 * Corner.TopLeftFinder.Row;
            Matrix[4, 7] = -3.0 * Corner.TopLeftFinder.Row;
            Matrix[4, 8] = Corner.TopLeftFinder.Row;

            Matrix[5, 3] = FarFinder;
            Matrix[5, 4] = 3.0;
            Matrix[5, 5] = 1.0;
            Matrix[5, 6] = -FarFinder * Corner.TopRightFinder.Row;
            Matrix[5, 7] = -3.0 * Corner.TopRightFinder.Row;
            Matrix[5, 8] = Corner.TopRightFinder.Row;

            Matrix[6, 3] = 3.0;
            Matrix[6, 4] = FarFinder;
            Matrix[6, 5] = 1.0;
            Matrix[6, 6] = -3.0 * Corner.BottomLeftFinder.Row;
            Matrix[6, 7] = -FarFinder * Corner.BottomLeftFinder.Row;
            Matrix[6, 8] = Corner.BottomLeftFinder.Row;

            Matrix[7, 3] = FarAlign;
            Matrix[7, 4] = FarAlign;
            Matrix[7, 5] = 1.0;
            Matrix[7, 6] = -FarAlign * ImageAlignRow;
            Matrix[7, 7] = -FarAlign * ImageAlignRow;
            Matrix[7, 8] = ImageAlignRow;

            for (int Row = 0; Row < 8; Row++)
            {
                // If the element is zero, make it non zero by adding another row
                if (Matrix[Row, Row] == 0)
                {
                    int Row1;
                    for (Row1 = Row + 1; Row1 < 8 && Matrix[Row1, Row] == 0; Row1++) ;
                    if (Row1 == 8) throw new ApplicationException("Solve linear equations failed");

                    for (int Col = Row; Col < 9; Col++) Matrix[Row, Col] += Matrix[Row1, Col];
                }

                // make the diagonal element 1.0
                for (int Col = 8; Col > Row; Col--) Matrix[Row, Col] /= Matrix[Row, Row];

                // subtract current row from next rows to eliminate one value
                for (int Row1 = Row + 1; Row1 < 8; Row1++)
                {
                    for (int Col = 8; Col > Row; Col--) Matrix[Row1, Col] -= Matrix[Row, Col] * Matrix[Row1, Row];
                }
            }

            // go up from last row and eliminate all solved values
            for (int Col = 7; Col > 0; Col--) for (int Row = Col - 1; Row >= 0; Row--)
                {
                    Matrix[Row, 8] -= Matrix[Row, Col] * Matrix[Col, 8];
                }

            Trans4a = Matrix[0, 8];
            Trans4b = Matrix[1, 8];
            Trans4c = Matrix[2, 8];
            Trans4d = Matrix[3, 8];
            Trans4e = Matrix[4, 8];
            Trans4f = Matrix[5, 8];
            Trans4g = Matrix[6, 8];
            Trans4h = Matrix[7, 8];

            // set trans 4 mode
            Trans4Mode = true;
            return;
        }

        ////////////////////////////////////////////////////////////////////
        // Get version code bits top right
        ////////////////////////////////////////////////////////////////////

        internal int GetVersionOne()
        {
            int VersionCode = 0;
            for (int Index = 0; Index < 18; Index++)
            {
                if (GetModule(Index / 3, QRCodeDimension - 11 + (Index % 3))) VersionCode |= 1 << Index;
            }
            return TestVersionCode(VersionCode);
        }

        ////////////////////////////////////////////////////////////////////
        // Get version code bits bottom left
        ////////////////////////////////////////////////////////////////////

        internal int GetVersionTwo()
        {
            int VersionCode = 0;
            for (int Index = 0; Index < 18; Index++)
            {
                if (GetModule(QRCodeDimension - 11 + (Index % 3), Index / 3)) VersionCode |= 1 << Index;
            }
            return TestVersionCode(VersionCode);
        }

        ////////////////////////////////////////////////////////////////////
        // Test version code bits
        ////////////////////////////////////////////////////////////////////

        internal int TestVersionCode
                (
                int VersionCode
                )
        {
            // format info
            int Code = VersionCode >> 12;

            // test for exact match
            if (Code >= 7 && Code <= 40 && QRCode.VersionCodeArray[Code - 7] == VersionCode)
            {
                return Code;
            }

            // look for a match
            int BestInfo = 0;
            int Error = int.MaxValue;
            for (int Index = 0; Index < 34; Index++)
            {
                // test for exact match
                int ErrorBits = VersionCodeArray[Index] ^ VersionCode;
                if (ErrorBits == 0) return VersionCode >> 12;

                // count errors
                int ErrorCount = CountBits(ErrorBits);

                // save best result
                if (ErrorCount < Error)
                {
                    Error = ErrorCount;
                    BestInfo = Index;
                }
            }

            return Error <= 3 ? BestInfo + 7 : 0;
        }

        ////////////////////////////////////////////////////////////////////
        // Get format info around top left corner
        ////////////////////////////////////////////////////////////////////

        public int GetFormatInfoOne()
        {
            int Info = 0;
            for (int Index = 0; Index < 15; Index++)
            {
                if (GetModule(FormatInfoOne[Index, 0], FormatInfoOne[Index, 1])) Info |= 1 << Index;
            }
            return TestFormatInfo(Info);
        }

        ////////////////////////////////////////////////////////////////////
        // Get format info around top right and bottom left corners
        ////////////////////////////////////////////////////////////////////

        internal int GetFormatInfoTwo()
        {
            int Info = 0;
            for (int Index = 0; Index < 15; Index++)
            {
                int Row = FormatInfoTwo[Index, 0];
                if (Row < 0) Row += QRCodeDimension;
                int Col = FormatInfoTwo[Index, 1];
                if (Col < 0) Col += QRCodeDimension;
                if (GetModule(Row, Col)) Info |= 1 << Index;
            }
            return TestFormatInfo(Info);
        }

        ////////////////////////////////////////////////////////////////////
        // Test format info bits
        ////////////////////////////////////////////////////////////////////

        internal int TestFormatInfo
                (
                int FormatInfo
                )
        {
            // format info
            int Info = (FormatInfo ^ 0x5412) >> 10;

            // test for exact match
            if (QRCode.FormatInfoArray[Info] == FormatInfo)
            {
                return Info;
            }

            // look for a match
            int BestInfo = 0;
            int Error = int.MaxValue;
            for (int Index = 0; Index < 32; Index++)
            {
                int ErrorCount = CountBits(QRCode.FormatInfoArray[Index] ^ FormatInfo);
                if (ErrorCount < Error)
                {
                    Error = ErrorCount;
                    BestInfo = Index;
                }
            }

            return Error <= 3 ? BestInfo : -1;
        }

        ////////////////////////////////////////////////////////////////////
        // Count Bits
        ////////////////////////////////////////////////////////////////////

        internal int CountBits
                (
                int Value
                )
        {
            int Count = 0;
            for (int Mask = 0x4000; Mask != 0; Mask >>= 1) if ((Value & Mask) != 0) Count++;
            return Count;
        }

        ////////////////////////////////////////////////////////////////////
        // Convert image to qr code matrix and test fixed modules
        ////////////////////////////////////////////////////////////////////

        internal void ConvertImageToMatrix()
        {
            // loop for all modules
            int FixedCount = 0;
            int ErrorCount = 0;
            for (int Row = 0; Row < QRCodeDimension; Row++) for (int Col = 0; Col < QRCodeDimension; Col++)
                {
                    // the module (Row, Col) is not a fixed module 
                    if ((BaseMatrix[Row, Col] & Fixed) == 0)
                    {
                        if (GetModule(Row, Col)) BaseMatrix[Row, Col] |= Black;
                    }

                    // fixed module
                    else
                    {
                        // total fixed modules
                        FixedCount++;

                        // test for error
                        if ((GetModule(Row, Col) ? Black : White) != (BaseMatrix[Row, Col] & 1)) ErrorCount++;
                    }
                }

            if (ErrorCount > FixedCount * ErrCorrPercent[(int)ErrorCorrection] / 100)
                throw new ApplicationException("Fixed modules error");
            return;
        }

        ////////////////////////////////////////////////////////////////////
        // Unload matrix data from base matrix
        ////////////////////////////////////////////////////////////////////

        internal void UnloadDataFromMatrix()
        {
            // input array pointer initialization
            int Ptr = 0;
            int PtrEnd = 8 * MaxCodewords;
            CodewordsArray = new byte[MaxCodewords];

            // bottom right corner of output matrix
            int Row = QRCodeDimension - 1;
            int Col = QRCodeDimension - 1;

            // step state
            int State = 0;
            for (; ; )
            {
                // current module is data
                if ((MaskMatrix[Row, Col] & NonData) == 0)
                {
                    // unload current module with
                    if ((MaskMatrix[Row, Col] & 1) != 0) CodewordsArray[Ptr >> 3] |= (byte)(1 << (7 - (Ptr & 7)));
                    if (++Ptr == PtrEnd) break;
                }

                // current module is non data and vertical timing line condition is on
                else if (Col == 6) Col--;

                // update matrix position to next module
                switch (State)
                {
                    // going up: step one to the left
                    case 0:
                        Col--;
                        State = 1;
                        continue;

                    // going up: step one row up and one column to the right
                    case 1:
                        Col++;
                        Row--;
                        // we are not at the top, go to state 0
                        if (Row >= 0)
                        {
                            State = 0;
                            continue;
                        }
                        // we are at the top, step two columns to the left and start going down
                        Col -= 2;
                        Row = 0;
                        State = 2;
                        continue;

                    // going down: step one to the left
                    case 2:
                        Col--;
                        State = 3;
                        continue;

                    // going down: step one row down and one column to the right
                    case 3:
                        Col++;
                        Row++;
                        // we are not at the bottom, go to state 2
                        if (Row < QRCodeDimension)
                        {
                            State = 2;
                            continue;
                        }
                        // we are at the bottom, step two columns to the left and start going up
                        Col -= 2;
                        Row = QRCodeDimension - 1;
                        State = 0;
                        continue;
                }
            }
            return;
        }

        ////////////////////////////////////////////////////////////////////
        // Restore interleave data and error correction blocks
        ////////////////////////////////////////////////////////////////////

        internal void RestoreBlocks()
        {
            // allocate temp codewords array
            byte[] TempArray = new byte[MaxCodewords];

            // total blocks
            int TotalBlocks = BlocksGroup1 + BlocksGroup2;

            // create array of data blocks starting point
            int[] Start = new int[TotalBlocks];
            for (int Index = 1; Index < TotalBlocks; Index++) Start[Index] = Start[Index - 1] + (Index <= BlocksGroup1 ? DataCodewordsGroup1 : DataCodewordsGroup2);

            // step one. iterleave base on group one length
            int PtrEnd = DataCodewordsGroup1 * TotalBlocks;

            // restore group one and two
            int Ptr;
            int Block = 0;
            for (Ptr = 0; Ptr < PtrEnd; Ptr++)
            {
                TempArray[Start[Block]] = CodewordsArray[Ptr];
                Start[Block]++;
                Block++;
                if (Block == TotalBlocks) Block = 0;
            }

            // restore group two
            if (DataCodewordsGroup2 > DataCodewordsGroup1)
            {
                // step one. iterleave base on group one length
                PtrEnd = MaxDataCodewords;

                Block = BlocksGroup1;
                for (; Ptr < PtrEnd; Ptr++)
                {
                    TempArray[Start[Block]] = CodewordsArray[Ptr];
                    Start[Block]++;
                    Block++;
                    if (Block == TotalBlocks) Block = BlocksGroup1;
                }
            }

            // create array of error correction blocks starting point
            Start[0] = MaxDataCodewords;
            for (int Index = 1; Index < TotalBlocks; Index++) Start[Index] = Start[Index - 1] + ErrCorrCodewords;

            // restore all groups
            PtrEnd = MaxCodewords;
            Block = 0;
            for (; Ptr < PtrEnd; Ptr++)
            {
                TempArray[Start[Block]] = CodewordsArray[Ptr];
                Start[Block]++;
                Block++;
                if (Block == TotalBlocks) Block = 0;
            }

            // save result
            CodewordsArray = TempArray;
            return;
        }

        ////////////////////////////////////////////////////////////////////
        // Calculate Error Correction
        ////////////////////////////////////////////////////////////////////

        protected void CalculateErrorCorrection()
        {
            // total error count
            int TotalErrorCount = 0;

            // set generator polynomial array
            byte[] Generator = GenArray[ErrCorrCodewords - 7];

            // error correcion calculation buffer
            int BufSize = Math.Max(DataCodewordsGroup1, DataCodewordsGroup2) + ErrCorrCodewords;
            byte[] ErrCorrBuff = new byte[BufSize];

            // initial number of data codewords
            int DataCodewords = DataCodewordsGroup1;
            int BuffLen = DataCodewords + ErrCorrCodewords;

            // codewords pointer
            int DataCodewordsPtr = 0;

            // codewords buffer error correction pointer
            int CodewordsArrayErrCorrPtr = MaxDataCodewords;

            // loop one block at a time
            int TotalBlocks = BlocksGroup1 + BlocksGroup2;
            for (int BlockNumber = 0; BlockNumber < TotalBlocks; BlockNumber++)
            {
                // switch to group2 data codewords
                if (BlockNumber == BlocksGroup1)
                {
                    DataCodewords = DataCodewordsGroup2;
                    BuffLen = DataCodewords + ErrCorrCodewords;
                }

                // copy next block of codewords to the buffer and clear the remaining part
                Array.Copy(CodewordsArray, DataCodewordsPtr, ErrCorrBuff, 0, DataCodewords);
                Array.Copy(CodewordsArray, CodewordsArrayErrCorrPtr, ErrCorrBuff, DataCodewords, ErrCorrCodewords);

                // make a duplicate
                byte[] CorrectionBuffer = (byte[])ErrCorrBuff.Clone();

                // error correction polynomial division
                ReedSolomon.PolynominalDivision(ErrCorrBuff, BuffLen, Generator, ErrCorrCodewords);

                // test for error
                int Index;
                for (Index = 0; Index < ErrCorrCodewords && ErrCorrBuff[DataCodewords + Index] == 0; Index++) ;
                if (Index < ErrCorrCodewords)
                {
                    // correct the error
                    int ErrorCount = ReedSolomon.CorrectData(CorrectionBuffer, BuffLen, ErrCorrCodewords);
                    if (ErrorCount <= 0)
                    {
                        throw new ApplicationException("Data is damaged. Error correction failed");
                    }

                    TotalErrorCount += ErrorCount;

                    // fix the data
                    Array.Copy(CorrectionBuffer, 0, CodewordsArray, DataCodewordsPtr, DataCodewords);
                }

                // update codewords array to next buffer
                DataCodewordsPtr += DataCodewords;

                // update pointer				
                CodewordsArrayErrCorrPtr += ErrCorrCodewords;
            }

            return;
        }

        ////////////////////////////////////////////////////////////////////
        // Convert bit array to byte array
        ////////////////////////////////////////////////////////////////////

        internal byte[] DecodeData()
        {
            // bit buffer initial condition
            BitBuffer = (UInt32)((CodewordsArray[0] << 24) | (CodewordsArray[1] << 16) | (CodewordsArray[2] << 8) | CodewordsArray[3]);
            BitBufferLen = 32;
            CodewordsPtr = 4;

            // allocate data byte list
            List<byte> DataSeg = new List<byte>();

            // data might be made of blocks
            for (; ; )
            {
                // first 4 bits is mode indicator
                EncodingMode EncodingMode = (EncodingMode)ReadBitsFromCodewordsArray(4);

                // end of data
                if (EncodingMode <= 0) break;

                // read data length
                int DataLength = ReadBitsFromCodewordsArray(DataLengthBits(EncodingMode));
                if (DataLength < 0)
                {
                    throw new ApplicationException("Premature end of data (DataLengh)");
                }

                // save start of segment
                int SegStart = DataSeg.Count;

                // switch based on encode mode
                // numeric code indicator is 0001, alpha numeric 0010, byte 0100
                switch (EncodingMode)
                {
                    // numeric mode
                    case EncodingMode.Numeric:
                        // encode digits in groups of 2
                        int NumericEnd = (DataLength / 3) * 3;
                        for (int Index = 0; Index < NumericEnd; Index += 3)
                        {
                            int Temp = ReadBitsFromCodewordsArray(10);
                            if (Temp < 0)
                            {
                                throw new ApplicationException("Premature end of data (Numeric 1)");
                            }
                            DataSeg.Add(DecodingTable[Temp / 100]);
                            DataSeg.Add(DecodingTable[(Temp % 100) / 10]);
                            DataSeg.Add(DecodingTable[Temp % 10]);
                        }

                        // we have one character remaining
                        if (DataLength - NumericEnd == 1)
                        {
                            int Temp = ReadBitsFromCodewordsArray(4);
                            if (Temp < 0)
                            {
                                throw new ApplicationException("Premature end of data (Numeric 2)");
                            }
                            DataSeg.Add(DecodingTable[Temp]);
                        }

                        // we have two character remaining
                        else if (DataLength - NumericEnd == 2)
                        {
                            int Temp = ReadBitsFromCodewordsArray(7);
                            if (Temp < 0)
                            {
                                throw new ApplicationException("Premature end of data (Numeric 3)");
                            }
                            DataSeg.Add(DecodingTable[Temp / 10]);
                            DataSeg.Add(DecodingTable[Temp % 10]);
                        }
                        break;

                    // alphanumeric mode
                    case EncodingMode.AlphaNumeric:
                        // encode digits in groups of 2
                        int AlphaNumEnd = (DataLength / 2) * 2;
                        for (int Index = 0; Index < AlphaNumEnd; Index += 2)
                        {
                            int Temp = ReadBitsFromCodewordsArray(11);
                            if (Temp < 0)
                            {
                                throw new ApplicationException("Premature end of data (Alpha Numeric 1)");
                            }
                            DataSeg.Add(DecodingTable[Temp / 45]);
                            DataSeg.Add(DecodingTable[Temp % 45]);
                        }

                        // we have one character remaining
                        if (DataLength - AlphaNumEnd == 1)
                        {
                            int Temp = ReadBitsFromCodewordsArray(6);
                            if (Temp < 0)
                            {
                                throw new ApplicationException("Premature end of data (Alpha Numeric 2)");
                            }
                            DataSeg.Add(DecodingTable[Temp]);
                        }
                        break;

                    // byte mode					
                    case EncodingMode.Byte:
                        // append the data after mode and character count
                        for (int Index = 0; Index < DataLength; Index++)
                        {
                            int Temp = ReadBitsFromCodewordsArray(8);
                            if (Temp < 0)
                            {
                                throw new ApplicationException("Premature end of data (byte mode)");
                            }
                            DataSeg.Add((byte)Temp);
                        }
                        break;

                    default:
                        throw new ApplicationException(string.Format("Encoding mode not supported {0}", EncodingMode.ToString()));
                }

                if (DataLength != DataSeg.Count - SegStart) throw new ApplicationException("Data encoding length in error");
            }

            // save data
            return DataSeg.ToArray();
        }

        ////////////////////////////////////////////////////////////////////
        // Read data from codeword array
        ////////////////////////////////////////////////////////////////////

        internal int ReadBitsFromCodewordsArray
                (
                int Bits
                )
        {
            if (Bits > BitBufferLen) return -1;
            int Data = (int)(BitBuffer >> (32 - Bits));
            BitBuffer <<= Bits;
            BitBufferLen -= Bits;
            while (BitBufferLen <= 24 && CodewordsPtr < MaxDataCodewords)
            {
                BitBuffer |= (UInt32)(CodewordsArray[CodewordsPtr++] << (24 - BitBufferLen));
                BitBufferLen += 8;
            }
            return Data;
        }
    }
}
