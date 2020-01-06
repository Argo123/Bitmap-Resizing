#pragma once

extern "C"
{
	__declspec(dllexport) void ScaleBitmap(unsigned char* bitmap, int start, int end, float ratioX, float ratioY, int widthIn, int widthOut, unsigned char* bitmapOut);
}