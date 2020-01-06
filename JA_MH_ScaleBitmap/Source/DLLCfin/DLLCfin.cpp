// DLLCfin.cpp : Defines the exported functions for the DLL application.
//
#include "stdafx.h"
#include "DLLCfin.h"

#define HEADER 54

	__declspec(dllexport) void ScaleBitmap(unsigned char* bitmap, int start, int end, float ratioX, float ratioY, int widthIn, int widthOut, unsigned char* bitmapOut)
	{
		int x, y;
		for (int i = start; i < end; i += 3)
		{
			int target;
			int k = i - HEADER;
			k /= 3;

			x = (int)round((k % widthOut) * ratioX);
			y = (int)round((k / widthOut) * ratioY);

			target = (x + widthIn * y) * 3 + HEADER;

			bitmapOut[i] = bitmap[target];
			bitmapOut[i + 1] = bitmap[target + 1];
			bitmapOut[i + 2] = bitmap[target + 2];
		}
	}