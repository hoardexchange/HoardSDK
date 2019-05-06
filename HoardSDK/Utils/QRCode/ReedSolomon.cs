/////////////////////////////////////////////////////////////////////
//
//	QR Code Library
//
//	QR Code error correction calculations.
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
	internal class ReedSolomon
	{
	internal static int INCORRECTABLE_ERROR = -1;

	internal static readonly byte[] ExpToInt = //	ExpToInt =
			{
			   1,   2,   4,   8,  16,  32,  64, 128,  29,  58, 116, 232, 205, 135,  19,  38,
			  76, 152,  45,  90, 180, 117, 234, 201, 143,   3,   6,  12,  24,  48,  96, 192,
			 157,  39,  78, 156,  37,  74, 148,  53, 106, 212, 181, 119, 238, 193, 159,  35,
			  70, 140,   5,  10,  20,  40,  80, 160,  93, 186, 105, 210, 185, 111, 222, 161,
			  95, 190,  97, 194, 153,  47,  94, 188, 101, 202, 137,  15,  30,  60, 120, 240,
			 253, 231, 211, 187, 107, 214, 177, 127, 254, 225, 223, 163,  91, 182, 113, 226,
			 217, 175,  67, 134,  17,  34,  68, 136,  13,  26,  52, 104, 208, 189, 103, 206,
			 129,  31,  62, 124, 248, 237, 199, 147,  59, 118, 236, 197, 151,  51, 102, 204,
			 133,  23,  46,  92, 184, 109, 218, 169,  79, 158,  33,  66, 132,  21,  42,  84,
			 168,  77, 154,  41,  82, 164,  85, 170,  73, 146,  57, 114, 228, 213, 183, 115,
			 230, 209, 191,  99, 198, 145,  63, 126, 252, 229, 215, 179, 123, 246, 241, 255,
			 227, 219, 171,  75, 150,  49,  98, 196, 149,  55, 110, 220, 165,  87, 174,  65,
			 130,  25,  50, 100, 200, 141,   7,  14,  28,  56, 112, 224, 221, 167,  83, 166,
			  81, 162,  89, 178, 121, 242, 249, 239, 195, 155,  43,  86, 172,  69, 138,   9,
			  18,  36,  72, 144,  61, 122, 244, 245, 247, 243, 251, 235, 203, 139,  11,  22,
			  44,  88, 176, 125, 250, 233, 207, 131,  27,  54, 108, 216, 173,  71, 142,   1,

			        2,   4,   8,  16,  32,  64, 128,  29,  58, 116, 232, 205, 135,  19,  38,
			  76, 152,  45,  90, 180, 117, 234, 201, 143,   3,   6,  12,  24,  48,  96, 192,
			 157,  39,  78, 156,  37,  74, 148,  53, 106, 212, 181, 119, 238, 193, 159,  35,
			  70, 140,   5,  10,  20,  40,  80, 160,  93, 186, 105, 210, 185, 111, 222, 161,
			  95, 190,  97, 194, 153,  47,  94, 188, 101, 202, 137,  15,  30,  60, 120, 240,
			 253, 231, 211, 187, 107, 214, 177, 127, 254, 225, 223, 163,  91, 182, 113, 226,
			 217, 175,  67, 134,  17,  34,  68, 136,  13,  26,  52, 104, 208, 189, 103, 206,
			 129,  31,  62, 124, 248, 237, 199, 147,  59, 118, 236, 197, 151,  51, 102, 204,
			 133,  23,  46,  92, 184, 109, 218, 169,  79, 158,  33,  66, 132,  21,  42,  84,
			 168,  77, 154,  41,  82, 164,  85, 170,  73, 146,  57, 114, 228, 213, 183, 115,
			 230, 209, 191,  99, 198, 145,  63, 126, 252, 229, 215, 179, 123, 246, 241, 255,
			 227, 219, 171,  75, 150,  49,  98, 196, 149,  55, 110, 220, 165,  87, 174,  65,
			 130,  25,  50, 100, 200, 141,   7,  14,  28,  56, 112, 224, 221, 167,  83, 166,
			  81, 162,  89, 178, 121, 242, 249, 239, 195, 155,  43,  86, 172,  69, 138,   9,
			  18,  36,  72, 144,  61, 122, 244, 245, 247, 243, 251, 235, 203, 139,  11,  22,
			  44,  88, 176, 125, 250, 233, 207, 131,  27,  54, 108, 216, 173,  71, 142,   1
			};

	internal static readonly byte[] IntToExp = //	IntToExp =
			{
			   0,   0,   1,  25,   2,  50,  26, 198,   3, 223,  51, 238,  27, 104, 199,  75,
			   4, 100, 224,  14,  52, 141, 239, 129,  28, 193, 105, 248, 200,   8,  76, 113,
			   5, 138, 101,  47, 225,  36,  15,  33,  53, 147, 142, 218, 240,  18, 130,  69,
			  29, 181, 194, 125, 106,  39, 249, 185, 201, 154,   9, 120,  77, 228, 114, 166,
			   6, 191, 139,  98, 102, 221,  48, 253, 226, 152,  37, 179,  16, 145,  34, 136,
			  54, 208, 148, 206, 143, 150, 219, 189, 241, 210,  19,  92, 131,  56,  70,  64,
			  30,  66, 182, 163, 195,  72, 126, 110, 107,  58,  40,  84, 250, 133, 186,  61,
			 202,  94, 155, 159,  10,  21, 121,  43,  78, 212, 229, 172, 115, 243, 167,  87,
			   7, 112, 192, 247, 140, 128,  99,  13, 103,  74, 222, 237,  49, 197, 254,  24,
			 227, 165, 153, 119,  38, 184, 180, 124,  17,  68, 146, 217,  35,  32, 137,  46,
			  55,  63, 209,  91, 149, 188, 207, 205, 144, 135, 151, 178, 220, 252, 190,  97,
			 242,  86, 211, 171,  20,  42,  93, 158, 132,  60,  57,  83,  71, 109,  65, 162,
			  31,  45,  67, 216, 183, 123, 164, 118, 196,  23,  73, 236, 127,  12, 111, 246,
			 108, 161,  59,  82,  41, 157,  85, 170, 251,  96, 134, 177, 187, 204,  62,  90,
			 203,  89,  95, 176, 156, 169, 160,  81,  11, 245,  22, 235, 122, 117,  44, 215,
			  79, 174, 213, 233, 230, 231, 173, 232, 116, 214, 244, 234, 168,  80,  88, 175
			};


	internal static int CorrectData
			(
			byte[]	ReceivedData,		// recived data buffer with data and error correction code
			int		DataLength,			// length of data in the buffer (note sometimes the array is longer than data) 
			int		ErrCorrCodewords	// numer of error correction codewords
			)
		{
		// calculate syndrome vector
		int[] Syndrome = CalculateSyndrome(ReceivedData, DataLength, ErrCorrCodewords);

		// received data has no error
		// note: this should not happen because we call this method only if error was detected
		if(Syndrome == null) return 0;

		// Modified Berlekamp-Massey
		// calculate sigma and omega
		int[] Sigma = new int[ErrCorrCodewords / 2 + 2];
		int[] Omega = new int[ErrCorrCodewords / 2 + 1];
		int ErrorCount = CalculateSigmaMBM(Sigma, Omega, Syndrome, ErrCorrCodewords);

		// data cannot be corrected
		if(ErrorCount <= 0) return INCORRECTABLE_ERROR;

		// look for error position using Chien search
		int[] ErrorPosition = new int[ErrorCount];
		if(!ChienSearch(ErrorPosition, DataLength, ErrorCount, Sigma)) return INCORRECTABLE_ERROR;

		// correct data array based on position array
		ApplyCorrection(ReceivedData, DataLength, ErrorCount, ErrorPosition, Sigma, Omega);

		// return error count before it was corrected
		return ErrorCount;
		}

	// Syndrome vector calculation
	// S0 = R0 + R1 +        R2 + ....        + Rn
	// S1 = R0 + R1 * A**1 + R2 * A**2 + .... + Rn * A**n
	// S2 = R0 + R1 * A**2 + R2 * A**4 + .... + Rn * A**2n
	// ....
	// Sm = R0 + R1 * A**m + R2 * A**2m + .... + Rn * A**mn

	internal static int[] CalculateSyndrome
			(
			byte[]		ReceivedData,		// recived data buffer with data and error correction code
			int		DataLength,			// length of data in the buffer (note sometimes the array is longer than data) 
			int		ErrCorrCodewords	// numer of error correction codewords
			)
		{
		// allocate syndrome vector
		int[] Syndrome = new int[ErrCorrCodewords];

		// reset error indicator
		bool Error = false;

		// syndrome[zero] special case
		// Total = Data[0] + Data[1] + ... Data[n]
		int Total = ReceivedData[0];
		for(int SumIndex = 1; SumIndex < DataLength; SumIndex++) Total = ReceivedData[SumIndex] ^ Total;
		Syndrome[0] = Total;
		if(Total != 0) Error = true;

		// all other synsromes
		for(int Index = 1; Index < ErrCorrCodewords;  Index++)
			{
			// Total = Data[0] + Data[1] * Alpha + Data[2] * Alpha ** 2 + ... Data[n] * Alpha ** n
			Total = ReceivedData[0];
			for(int IndexT = 1; IndexT < DataLength; IndexT++) Total = ReceivedData[IndexT] ^ MultiplyIntByExp(Total, Index);
			Syndrome[Index] = Total;
			if(Total != 0) Error = true;
			}

		// if there is an error return syndrome vector otherwise return null
		return Error ? Syndrome : null;
		}

	// Modified Berlekamp-Massey
	internal static int CalculateSigmaMBM
			(
			int[]		Sigma,
			int[]		Omega,
			int[]		Syndrome,
			int		ErrCorrCodewords
			)
		{
		int[] PolyC = new int[ErrCorrCodewords];
		int[] PolyB = new int[ErrCorrCodewords];
		PolyC[1] = 1;
		PolyB[0] = 1;
		int ErrorControl = 1;
		int ErrorCount = 0;		// L
		int m = -1;

		for(int ErrCorrIndex = 0; ErrCorrIndex < ErrCorrCodewords; ErrCorrIndex++)
			{
			// Calculate the discrepancy
			int Dis = Syndrome[ErrCorrIndex];
			for(int i = 1; i <= ErrorCount; i++) Dis ^= Multiply(PolyB[i], Syndrome[ErrCorrIndex - i]);

			if(Dis != 0)
				{
				int DisExp = IntToExp[Dis];
				int[] WorkPolyB = new int[ErrCorrCodewords];
				for(int Index = 0; Index <= ErrCorrIndex; Index++) WorkPolyB[Index] = PolyB[Index] ^ MultiplyIntByExp(PolyC[Index], DisExp);
				int js = ErrCorrIndex - m;
				if(js > ErrorCount)
					{
					m = ErrCorrIndex - ErrorCount;
					ErrorCount = js;
					if(ErrorCount > ErrCorrCodewords / 2) return INCORRECTABLE_ERROR;
					for(int Index = 0; Index <= ErrorControl; Index++) PolyC[Index] = DivideIntByExp(PolyB[Index], DisExp);
					ErrorControl = ErrorCount;
					}
				PolyB = WorkPolyB;
				}

			// shift polynomial right one
			Array.Copy(PolyC, 0, PolyC, 1, Math.Min(PolyC.Length - 1, ErrorControl));
			PolyC[0] = 0;
			ErrorControl++;
			}

		PolynomialMultiply(Omega, PolyB, Syndrome);
		Array.Copy(PolyB, 0, Sigma, 0, Math.Min(PolyB.Length, Sigma.Length));
		return ErrorCount;
		}

	// Chien search is a fast algorithm for determining roots of polynomials defined over a finite field.
	// The most typical use of the Chien search is in finding the roots of error-locator polynomials
	// encountered in decoding Reed-Solomon codes and BCH codes.
	private static bool ChienSearch
			(
			int[]	ErrorPosition,
			int		DataLength,
			int		ErrorCount,
			int[]	Sigma
			)
		{
		// last error
		int LastPosition = Sigma[1];

		// one error
		if(ErrorCount == 1)
			{
			// position is out of range
			if(IntToExp[LastPosition] >= DataLength) return false;

			// save the only error position in position array
			ErrorPosition[0] = LastPosition;
			return true;
			}

		// we start at last error position
		int PosIndex = ErrorCount - 1;
		for(int DataIndex = 0; DataIndex < DataLength; DataIndex++)
			{
			int DataIndexInverse = 255 - DataIndex;
			int Total = 1;
			for(int Index = 1; Index <= ErrorCount; Index++) Total ^= MultiplyIntByExp(Sigma[Index], (DataIndexInverse * Index) % 255);
			if(Total != 0) continue;

			int Position = ExpToInt[DataIndex];
			LastPosition ^=  Position;
			ErrorPosition[PosIndex--] = Position;
			if(PosIndex == 0)
				{
				// position is out of range
				if(IntToExp[LastPosition] >= DataLength)  return false;
				ErrorPosition[0] = LastPosition;
				return true;
				}
			}

		// search failed
		return false;
		}

	private static void ApplyCorrection
			(
			byte[]	ReceivedData,
			int		DataLength,
			int		ErrorCount,
			int[]	ErrorPosition,
			int[]	Sigma,
			int[]	Omega
			)
		{
		for(int ErrIndex = 0; ErrIndex < ErrorCount; ErrIndex++)
			{
			int ps = ErrorPosition[ErrIndex];
			int zlog = 255 - IntToExp[ps];
			int OmegaTotal = Omega[0];
			for(int Index = 1; Index < ErrorCount; Index++) OmegaTotal ^= MultiplyIntByExp(Omega[Index], (zlog * Index) % 255);
			int SigmaTotal = Sigma[1];
			for(int j = 2; j < ErrorCount; j += 2) SigmaTotal ^= MultiplyIntByExp(Sigma[j + 1], (zlog * j) % 255);
			ReceivedData[DataLength - 1 - IntToExp[ps]] ^= (byte) MultiplyDivide(ps, OmegaTotal, SigmaTotal);
			}
		return;
		}

	internal static void PolynominalDivision(byte[] Polynomial, int PolyLength, byte[] Generator, int ErrCorrCodewords)
		{
		int DataCodewords = PolyLength - ErrCorrCodewords;

		// error correction polynomial division
		for(int Index = 0; Index < DataCodewords; Index++)
			{
			// current first codeword is zero
			if(Polynomial[Index] == 0) continue;

			// current first codeword is not zero
			int Multiplier = IntToExp[Polynomial[Index]];

			// loop for error correction coofficients
			for(int GeneratorIndex = 0; GeneratorIndex < ErrCorrCodewords; GeneratorIndex++)
				{
				Polynomial[Index + 1 + GeneratorIndex] = (byte) (Polynomial[Index + 1 + GeneratorIndex] ^ ExpToInt[Generator[GeneratorIndex] + Multiplier]);
				}
			}
		return;
		}

	internal static int Multiply
			(
			int Int1,
			int Int2
			)
		{
		return (Int1 == 0 || Int2 == 0) ? 0 : ExpToInt[IntToExp[Int1] + IntToExp[Int2]];
		}

	internal static int MultiplyIntByExp
			(
			int Int,
			int Exp
			)
		{
		return Int == 0 ? 0 : ExpToInt[IntToExp[Int] + Exp];
		}

	internal static int MultiplyDivide
			(
			int Int1,
			int Int2,
			int Int3
			)
		{
		return (Int1 == 0 || Int2 == 0) ? 0 : ExpToInt[(IntToExp[Int1] + IntToExp[Int2] - IntToExp[Int3] + 255) % 255];
		}

	internal static int DivideIntByExp
			(
			int Int,
			int Exp
			)
		{
		return Int == 0 ? 0 : ExpToInt[IntToExp[Int] - Exp + 255];
		}

	internal static void PolynomialMultiply(int[] Result, int[] Poly1, int[] Poly2)
		{
		Array.Clear(Result, 0, Result.Length);
		for(int Index1 = 0; Index1 < Poly1.Length; Index1++)
			{
			if(Poly1[Index1] == 0) continue;
			int loga = IntToExp[Poly1[Index1]];
			int Index2End = Math.Min(Poly2.Length, Result.Length - Index1);
			// = Sum(Poly1[Index1] * Poly2[Index2]) for all Index2
			for(int Index2 = 0; Index2 < Index2End; Index2++)
				if(Poly2[Index2] != 0) Result[Index1 + Index2] ^= ExpToInt[loga + IntToExp[Poly2[Index2]]];
			}
		return;
		}
	}
}
