/////////////////////////////////////////////////////////////////////
//
//	QR Code Library
//
//	QR Code encoder.
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

namespace Hoard.Utils.QRCodeEncoder
{
/// <summary>
/// QR Code Encoder class
/// </summary>
public class QREncoder : QRCode
    {
	/// <summary>
	/// Constructor
	/// </summary>
	public QREncoder() {}

	/// <summary>
	/// Encode one string into QRCode boolean matrix
	/// </summary>
	/// <param name="ErrorCorrection">Error correction (L, M, Q, H)</param>
	/// <param name="StringDataSegment">string data segment</param>
	/// <returns>QR Code boolean matrix</returns>
	public bool[,] Encode
			(
			ErrorCorrection	ErrorCorrection,
			string StringDataSegment
			)
		{
		// empty
		if(string.IsNullOrEmpty(StringDataSegment)) return Encode(ErrorCorrection, (byte[][]) null);

		// convert string to byte array
		return Encode(ErrorCorrection, StrToByteArray(StringDataSegment));
		}

	/// <summary>
	/// Encode one string into QRCode boolean matrix
	/// </summary>
	/// <param name="ErrorCorrection">Error correction (L, M, Q, H)</param>
	/// <param name="StringDataSegments">string data segments</param>
	/// <returns>QR Code boolean matrix</returns>
	public bool[,] Encode
			(
			ErrorCorrection	ErrorCorrection,
			string[] StringDataSegments
			)
		{
		// empty
		if(StringDataSegments == null) return Encode(ErrorCorrection, (byte[][]) null);

		// create bytes arrays
		byte[][] TempDataSegArray = new byte[StringDataSegments.Length][];

		// loop for all segments
		for(int SegIndex = 0; SegIndex < StringDataSegments.Length; SegIndex++)
			{
			// convert string to byte array
			TempDataSegArray[SegIndex] = StrToByteArray(StringDataSegments[SegIndex]);
			}
		
		// convert string to byte array
		return Encode(ErrorCorrection, TempDataSegArray);
		}

	/// <summary>
	/// Encode one data segment into QRCode boolean matrix
	/// </summary>
	/// <param name="ErrorCorrection">Error correction (L, M, Q, H)</param>
	/// <param name="SingleDataSeg">Data segment byte array</param>
	/// <returns>QR Code boolean matrix</returns>
	public bool[,] Encode
			(
			ErrorCorrection	ErrorCorrection,
			byte[] SingleDataSeg
			)
		{
		return Encode(ErrorCorrection, new byte[][] {SingleDataSeg});
		}

	/// <summary>
	/// Encode data segments array into QRCode boolean matrix
	/// </summary>
	/// <param name="ErrorCorrection">Error correction (L, M, Q, H)</param>
	/// <param name="DataSegArray">Data array of byte arrays</param>
	/// <returns>QR Code boolean matrix</returns>
	public bool[,] Encode
			(
			ErrorCorrection	ErrorCorrection,
			byte[][] DataSegArray
			)
		{
		// reset result variables
		QRCodeMatrix = null;
		QRCodeVersion = 0;
		QRCodeDimension = 0;

		// test error correction
		if(ErrorCorrection != ErrorCorrection.L && ErrorCorrection != ErrorCorrection.M &&
			ErrorCorrection != ErrorCorrection.Q && ErrorCorrection != ErrorCorrection.H)
				throw new ApplicationException("Invalid error correction mode. Must be L, M, Q or H.");

		// test data segments array
		if(DataSegArray == null|| DataSegArray.Length == 0) throw new ApplicationException("Input data segment argument is missing.");

		// loop for all segments
		int Bytes = 0;
		for(int SegIndex = 0; SegIndex < DataSegArray.Length; SegIndex++)
			{
			// input string length
			byte[] DataSeg = DataSegArray[SegIndex];
			if(DataSeg == null) DataSegArray[SegIndex] = new byte[0];
			else Bytes += DataSeg.Length;
			}
		if(Bytes == 0) throw new ApplicationException("There is nothing to encode.");
		
		// save error correction
		this.ErrorCorrection = ErrorCorrection;

		// save data segments array
		this.DataSegArray = DataSegArray;

		// initialization
		Initialization();

		// encode data
		EncodeData();

		// calculate error correction
		CalculateErrorCorrection();

		// iterleave data and error correction codewords
		InterleaveBlocks();

		// build base matrix
		BuildBaseMatrix();

		// load base matrix with data and error correction codewords
		LoadMatrixWithData();

		// data masking
		SelectBastMask();

		// add format information (error code level and mask code)
		AddFormatInformation();

		// output matrix size in pixels
		QRCodeMatrix = new bool[QRCodeDimension, QRCodeDimension];

		// convert result matrix to output matrix
		// Black=true, White=false
		for(int Row = 0; Row < QRCodeDimension; Row++)
			{
			for(int Col = 0; Col < QRCodeDimension; Col++)
				{
				if((ResultMatrix[Row, Col] & 1) != 0) QRCodeMatrix[Row, Col] = true;
				}
			}

		// exit with result
		return QRCodeMatrix;
		}

	////////////////////////////////////////////////////////////////////
	// Initialization
	////////////////////////////////////////////////////////////////////

	internal void Initialization()
		{
		// create encoding mode array
		EncodingSegMode = new EncodingMode[DataSegArray.Length];

		// reset total encoded data bits
		EncodedDataBits = 0;

		// loop for all segments
		for(int SegIndex = 0; SegIndex < DataSegArray.Length; SegIndex++)
			{
			// input string length
			byte[] DataSeg = DataSegArray[SegIndex];
			int DataLength = DataSeg.Length;

			// find encoding mode
			EncodingMode EncodingMode = EncodingMode.Numeric;
			for(int Index = 0; Index < DataLength; Index++)
				{
				int Code = EncodingTable[(int) DataSeg[Index]];
				if(Code < 10) continue;
				if(Code < 45)
					{
					EncodingMode = EncodingMode.AlphaNumeric;
					continue;
					}
				EncodingMode = EncodingMode.Byte;
				break;			
				}

			// calculate required bit length
			int DataBits = 4;
			switch(EncodingMode)
				{
				case EncodingMode.Numeric:
					DataBits += 10 * (DataLength / 3);
					if((DataLength % 3) == 1) DataBits += 4; 
					else if((DataLength % 3) == 2) DataBits += 7; 
					break;

				case EncodingMode.AlphaNumeric:
					DataBits += 11 * (DataLength / 2);
					if((DataLength & 1) != 0) DataBits += 6; 
					break;

				case EncodingMode.Byte:
					DataBits += 8 * DataLength;
					break;
				}

			EncodingSegMode[SegIndex] = EncodingMode;
			EncodedDataBits += DataBits;
			}

		// find best version
		int TotalDataLenBits = 0;
		for(QRCodeVersion = 1; QRCodeVersion <= 40; QRCodeVersion++)
			{
			// number of bits on each side of the QR code square
			QRCodeDimension = 17 + 4 * QRCodeVersion;

			SetDataCodewordsLength();
			TotalDataLenBits = 0;
			for(int Seg = 0; Seg < EncodingSegMode.Length; Seg++) TotalDataLenBits += DataLengthBits(EncodingSegMode[Seg]);
			if(EncodedDataBits + TotalDataLenBits <= MaxDataBits) break;
			}

		if(QRCodeVersion > 40) throw new ApplicationException("Input data string is too long");
		EncodedDataBits += TotalDataLenBits;
		return;
		}
			
	////////////////////////////////////////////////////////////////////
	// QRCode: Convert data to bit array
	////////////////////////////////////////////////////////////////////
	internal void EncodeData()
		{
		// codewords array
		CodewordsArray = new byte[MaxCodewords];

		// reset encoding members
		CodewordsPtr = 0;
		BitBuffer = 0;
		BitBufferLen = 0;

		// loop for all segments
		for(int SegIndex = 0; SegIndex < DataSegArray.Length; SegIndex++)
			{
			// input string length
			byte[] DataSeg = DataSegArray[SegIndex];
			int DataLength = DataSeg.Length;

			// first 4 bits is mode indicator
			// numeric code indicator is 0001, alpha numeric 0010, byte 0100
			SaveBitsToCodewordsArray((int) EncodingSegMode[SegIndex], 4);

			// character count
			SaveBitsToCodewordsArray(DataLength, DataLengthBits(EncodingSegMode[SegIndex]));
			
			// switch based on encode mode
			switch(EncodingSegMode[SegIndex])
				{				
				// numeric mode
				case EncodingMode.Numeric: 
					// encode digits in groups of 3
					int NumEnd = (DataLength / 3) * 3;
					for(int Index = 0; Index < NumEnd; Index += 3) SaveBitsToCodewordsArray(
						100 * EncodingTable[(int) DataSeg[Index]] + 10 * EncodingTable[(int) DataSeg[Index + 1]] + EncodingTable[(int) DataSeg[Index + 2]], 10);

					// we have one digit remaining
					if(DataLength - NumEnd == 1) SaveBitsToCodewordsArray(EncodingTable[(int) DataSeg[NumEnd]], 4);

					// we have two digits remaining
					else if(DataLength - NumEnd == 2) SaveBitsToCodewordsArray(10 * EncodingTable[(int) DataSeg[NumEnd]] + EncodingTable[(int) DataSeg[NumEnd + 1]], 7);
					break;

				// alphanumeric mode
				case EncodingMode.AlphaNumeric: 
					// encode digits in groups of 2
					int AlphaNumEnd = (DataLength / 2) * 2;
					for(int Index = 0; Index < AlphaNumEnd; Index += 2)
						SaveBitsToCodewordsArray(45 * EncodingTable[(int) DataSeg[Index]] + EncodingTable[(int) DataSeg[Index + 1]], 11);

					// we have one character remaining
					if(DataLength - AlphaNumEnd == 1) SaveBitsToCodewordsArray(EncodingTable[(int) DataSeg[AlphaNumEnd]], 6);
					break;
					

				// byte mode					
				case EncodingMode.Byte: 
					// append the data after mode and character count
					for(int Index = 0; Index < DataLength; Index++) SaveBitsToCodewordsArray((int) DataSeg[Index], 8);
					break;
				}
			}
			
		// set terminator
		if(EncodedDataBits < MaxDataBits) SaveBitsToCodewordsArray(0, MaxDataBits - EncodedDataBits < 4 ? MaxDataBits - EncodedDataBits : 4);

		// flush bit buffer
		if(BitBufferLen > 0) CodewordsArray[CodewordsPtr++] = (byte) (BitBuffer >> 24);

		// add extra padding if there is still space
		int PadEnd = MaxDataCodewords - CodewordsPtr;
		for(int PadPtr = 0; PadPtr < PadEnd; PadPtr++) CodewordsArray[CodewordsPtr + PadPtr] = (byte) ((PadPtr & 1) == 0 ? 0xEC : 0x11); 

		// exit
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Save data to codeword array
	////////////////////////////////////////////////////////////////////
	internal void SaveBitsToCodewordsArray
			(
			int	Data,
			int	Bits
			)
		{
		BitBuffer |= (uint) Data << (32 - BitBufferLen - Bits);
		BitBufferLen += Bits;
		while(BitBufferLen >= 8)
			{
			CodewordsArray[CodewordsPtr++] = (byte) (BitBuffer >> 24);
			BitBuffer <<= 8;
			BitBufferLen -= 8;
			}
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Calculate Error Correction
	////////////////////////////////////////////////////////////////////
	internal void CalculateErrorCorrection()
		{
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
		for(int BlockNumber = 0; BlockNumber < TotalBlocks; BlockNumber++)
			{
			// switch to group2 data codewords
			if(BlockNumber == BlocksGroup1)
				{
				DataCodewords = DataCodewordsGroup2;
				BuffLen = DataCodewords + ErrCorrCodewords;
				}

			// copy next block of codewords to the buffer and clear the remaining part
			Array.Copy(CodewordsArray, DataCodewordsPtr, ErrCorrBuff, 0, DataCodewords);
			Array.Clear(ErrCorrBuff, DataCodewords, ErrCorrCodewords);

			// update codewords array to next buffer
			DataCodewordsPtr += DataCodewords;

			// error correction polynomial division
			PolynominalDivision(ErrCorrBuff, BuffLen, Generator, ErrCorrCodewords);

			// save error correction block			
			Array.Copy(ErrCorrBuff, DataCodewords, CodewordsArray, CodewordsArrayErrCorrPtr, ErrCorrCodewords);
			CodewordsArrayErrCorrPtr += ErrCorrCodewords;
			}
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Interleave data and error correction blocks
	////////////////////////////////////////////////////////////////////
	internal void InterleaveBlocks()
		{
		// allocate temp codewords array
		byte[] TempArray = new byte[MaxCodewords];

		// total blocks
		int TotalBlocks = BlocksGroup1 + BlocksGroup2;

		// create array of data blocks starting point
		int[] Start = new int[TotalBlocks];
		for(int Index = 1; Index < TotalBlocks; Index++) Start[Index] = Start[Index - 1] + (Index <= BlocksGroup1 ? DataCodewordsGroup1 : DataCodewordsGroup2);

		// step one. iterleave base on group one length
		int PtrEnd = DataCodewordsGroup1 * TotalBlocks;

		// iterleave group one and two
		int Ptr;
		int Block = 0;
		for(Ptr = 0; Ptr < PtrEnd; Ptr++)
			{
			TempArray[Ptr] = CodewordsArray[Start[Block]];
			Start[Block]++;
			Block++;
			if(Block == TotalBlocks) Block = 0;
			}

		// interleave group two
		if(DataCodewordsGroup2 > DataCodewordsGroup1)
			{
			// step one. iterleave base on group one length
			PtrEnd = MaxDataCodewords;

			Block = BlocksGroup1;
			for(; Ptr < PtrEnd; Ptr++)
				{
				TempArray[Ptr] = CodewordsArray[Start[Block]];
				Start[Block]++;
				Block++;
				if(Block == TotalBlocks) Block = BlocksGroup1;
				}
			}

		// create array of error correction blocks starting point
		Start[0] = MaxDataCodewords;
		for(int Index = 1; Index < TotalBlocks; Index++) Start[Index] = Start[Index - 1] + ErrCorrCodewords;

		// step one. iterleave base on group one length

		// iterleave all groups
		PtrEnd = MaxCodewords;
		Block = 0;
		for(; Ptr < PtrEnd; Ptr++)
			{
			TempArray[Ptr] = CodewordsArray[Start[Block]];
			Start[Block]++;
			Block++;
			if(Block == TotalBlocks) Block = 0;
			}

		// save result
		CodewordsArray = TempArray;
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Load base matrix with data and error correction codewords
	////////////////////////////////////////////////////////////////////
	internal void LoadMatrixWithData()
		{
		// input array pointer initialization
		int Ptr = 0;
		int PtrEnd = 8 * MaxCodewords;

		// bottom right corner of output matrix
		int Row = QRCodeDimension - 1;
		int Col = QRCodeDimension - 1;

		// step state
		int State = 0;
		for(;;) 
			{
			// current module is data
			if((BaseMatrix[Row, Col] & NonData) == 0)
				{
				// load current module with
				if((CodewordsArray[Ptr >> 3] & (1 << (7 - (Ptr & 7)))) != 0) BaseMatrix[Row, Col] = DataBlack;
				if(++Ptr == PtrEnd) break;
				}

			// current module is non data and vertical timing line condition is on
			else if(Col == 6) Col--;

			// update matrix position to next module
			switch(State)
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
					if(Row >= 0)
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
					if(Row < QRCodeDimension)
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
	// Select Mask
	////////////////////////////////////////////////////////////////////
	internal void SelectBastMask()
		{
		int BestScore = int.MaxValue;
		MaskCode = 0;

		for(int TestMask = 0; TestMask < 8; TestMask++)
			{
			// apply mask
			ApplyMask(TestMask);

			// evaluate 4 test conditions
			int Score = EvaluationCondition1();
			if(Score >= BestScore) continue;
			Score += EvaluationCondition2();
			if(Score >= BestScore) continue;
			Score += EvaluationCondition3();
			if(Score >= BestScore) continue;
			Score += EvaluationCondition4();
			if(Score >= BestScore) continue;

			// save as best mask so far
			ResultMatrix = MaskMatrix;
			MaskMatrix = null;
			BestScore = Score;
			MaskCode = TestMask;
			}
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Evaluation condition #1
	// 5 consecutive or more modules of the same color
	////////////////////////////////////////////////////////////////////
	internal int EvaluationCondition1()
		{
		int Score = 0;

		// test rows
		for(int Row = 0; Row < QRCodeDimension; Row++)
			{
			int Count = 1;
			for(int Col = 1; Col < QRCodeDimension; Col++)
				{
				// current cell is not the same color as the one before
				if(((MaskMatrix[Row, Col - 1] ^ MaskMatrix[Row, Col]) & 1) != 0)
					{
					if(Count >= 5) Score += Count - 2;
					Count = 0;
					}
				Count++;
				}

			// last run
			if(Count >= 5) Score += Count - 2;
			}

		// test columns
		for(int Col = 0; Col < QRCodeDimension; Col++)
			{
			int Count = 1;
			for(int Row = 1; Row < QRCodeDimension; Row++)
				{
				// current cell is not the same color as the one before
				if(((MaskMatrix[Row - 1, Col] ^ MaskMatrix[Row, Col]) & 1) != 0)
					{
					if(Count >= 5) Score += Count - 2;
					Count = 0;
					}
				Count++;
				}

			// last run
			if(Count >= 5) Score += Count - 2;
			}
		return Score;
		}

	////////////////////////////////////////////////////////////////////
	// Evaluation condition #2
	// same color in 2 by 2 area
	////////////////////////////////////////////////////////////////////
	internal int EvaluationCondition2()
		{
		int Score = 0;
		// test rows
		for(int Row = 1; Row < QRCodeDimension; Row++) for(int Col = 1; Col < QRCodeDimension; Col++)
			{
			// all are black
			if(((MaskMatrix[Row - 1, Col - 1] & MaskMatrix[Row - 1, Col] & MaskMatrix[Row, Col - 1] & MaskMatrix[Row, Col]) & 1) != 0) Score += 3;

			// all are white
			else if(((MaskMatrix[Row - 1, Col - 1] | MaskMatrix[Row - 1, Col] | MaskMatrix[Row, Col - 1] | MaskMatrix[Row, Col]) & 1) == 0) Score += 3;
			}
		return Score;
		}

	////////////////////////////////////////////////////////////////////
	// Evaluation condition #3
	// pattern dark, light, dark, dark, dark, light, dark
	// before or after 4 light modules
	////////////////////////////////////////////////////////////////////
	internal int EvaluationCondition3()
		{
		int Score = 0;

		// test rows
		for(int Row = 0; Row < QRCodeDimension; Row++)
			{
			int Start = 0;

			// look for a lignt run at least 4 modules
			for(int Col = 0; Col < QRCodeDimension; Col++)
				{
				// current cell is white
				if((MaskMatrix[Row, Col] & 1) == 0) continue;

				// more or equal to 4
				if(Col - Start >= 4)
					{
					// we have 4 or more white
					// test for pattern before the white space
					if(Start >= 7 && TestHorizontalDarkLight(Row, Start - 7)) Score += 40;

					// test for pattern after the white space
					if(QRCodeDimension - Col >= 7 && TestHorizontalDarkLight(Row, Col))
						{
						Score += 40;
						Col += 6;
						}
					}

				// assume next one is white
				Start = Col + 1;
				}

			// last run
			if(QRCodeDimension - Start >= 4 && Start >= 7 && TestHorizontalDarkLight(Row, Start - 7)) Score += 40;
			}

		// test columns
		for(int Col = 0; Col < QRCodeDimension; Col++)
			{
			int Start = 0;

			// look for a lignt run at least 4 modules
			for(int Row = 0; Row < QRCodeDimension; Row++)
				{
				// current cell is white
				if((MaskMatrix[Row, Col] & 1) == 0) continue;

				// more or equal to 4
				if(Row - Start >= 4)
					{
					// we have 4 or more white
					// test for pattern before the white space
					if(Start >= 7 && TestVerticalDarkLight(Start - 7, Col)) Score += 40;

					// test for pattern after the white space
					if(QRCodeDimension - Row >= 7 && TestVerticalDarkLight(Row, Col))
						{
						Score += 40;
						Row += 6;
						}
					}

				// assume next one is white
				Start = Row + 1;
				}

			// last run
			if(QRCodeDimension - Start >= 4 && Start >= 7 && TestVerticalDarkLight(Start - 7, Col)) Score += 40;
			}

		// exit
		return Score;
		}

	////////////////////////////////////////////////////////////////////
	// Evaluation condition #4
	// blak to white ratio
	////////////////////////////////////////////////////////////////////

	internal int EvaluationCondition4()
		{
		// count black cells
		int Black = 0;
		for(int Row = 0; Row < QRCodeDimension; Row++) for(int Col = 0; Col < QRCodeDimension; Col++) if((MaskMatrix[Row, Col] & 1) != 0) Black++;

		// ratio
		double Ratio = (double) Black / (double) (QRCodeDimension * QRCodeDimension);

		// there are more black than white
		if(Ratio > 0.55) return (int) (20.0 * (Ratio - 0.5)) * 10;
		else if(Ratio < 0.45) return (int) (20.0 * (0.5 - Ratio)) * 10;
		return 0;
		}

	////////////////////////////////////////////////////////////////////
	// Test horizontal dark light pattern
	////////////////////////////////////////////////////////////////////
	internal bool TestHorizontalDarkLight
			(
			int	Row,
			int	Col
			)
		{
		return (MaskMatrix[Row, Col] & ~MaskMatrix[Row, Col + 1] & MaskMatrix[Row, Col + 2] & MaskMatrix[Row, Col + 3] &
					MaskMatrix[Row, Col + 4] & ~MaskMatrix[Row, Col + 5] & MaskMatrix[Row, Col + 6] & 1) != 0;
		}

	////////////////////////////////////////////////////////////////////
	// Test vertical dark light pattern
	////////////////////////////////////////////////////////////////////
	internal bool TestVerticalDarkLight
			(
			int	Row,
			int	Col
			)
		{
		return (MaskMatrix[Row, Col] & ~MaskMatrix[Row + 1, Col] & MaskMatrix[Row + 2, Col] & MaskMatrix[Row + 3, Col] &
					MaskMatrix[Row + 4, Col] & ~MaskMatrix[Row + 5, Col] & MaskMatrix[Row + 6, Col] & 1) != 0;
		}

	////////////////////////////////////////////////////////////////////
	// Add format information
	// version, error correction code plus mask code
	////////////////////////////////////////////////////////////////////
	internal void AddFormatInformation()
		{
		int Mask;

		// version information
		if(QRCodeVersion >= 7)
			{
			int Pos = QRCodeDimension - 11;
			int VerInfo = VersionCodeArray[QRCodeVersion - 7];

			// top right
			Mask = 1;
			for(int Row = 0; Row < 6; Row++) for(int Col = 0; Col < 3; Col++)
				{
				ResultMatrix[Row, Pos + Col] = (VerInfo & Mask) != 0 ? FixedBlack : FixedWhite;
				Mask <<= 1;
				}

			// bottom left
			Mask = 1;
			for(int Col = 0; Col < 6; Col++) for(int Row = 0; Row < 3; Row++)
				{
				ResultMatrix[Pos + Row, Col] =  (VerInfo & Mask) != 0 ? FixedBlack : FixedWhite;
				Mask <<= 1;
				}
			}

		// error correction code and mask number
		int FormatInfoPtr = 0; // M is the default
		switch(ErrorCorrection)
			{
			case ErrorCorrection.L:
				FormatInfoPtr = 8;
				break;

			case ErrorCorrection.Q:
				FormatInfoPtr = 24;
				break;

			case ErrorCorrection.H:
				FormatInfoPtr = 16;
				break;
			}
		int FormatInfo = FormatInfoArray[FormatInfoPtr + MaskCode];

		// load format bits into result matrix
		Mask = 1;
		for(int Index = 0; Index < 15; Index++)
			{
			int FormatBit = (FormatInfo & Mask) != 0 ? FixedBlack : FixedWhite;
			Mask <<= 1;

			// top left corner
			ResultMatrix[FormatInfoOne[Index, 0], FormatInfoOne[Index, 1]] = (byte) FormatBit;

			// bottom left and top right corners
			int Row = FormatInfoTwo[Index, 0];
			if(Row < 0) Row += QRCodeDimension;
			int Col = FormatInfoTwo[Index, 1];
			if(Col < 0) Col += QRCodeDimension;
			ResultMatrix[Row, Col] = (byte) FormatBit;
			}
		return;
		}
    }
}
