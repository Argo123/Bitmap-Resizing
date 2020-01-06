; VARIABLES
;b - Start value
;c - End value
;e - Input bitmap's width
;f - Output bitmap's width
;g - Pointer to the start of output bitmap
;h - Pointer to the start of input bitmap

.DATA
_half dd 0.500
.CODE

ScaleBitmap PROC a : DWORD, b : DWORD, c : DWORD, d : DWORD, e : DWORD, f : DWORD, g : QWORD, h : QWORD
	PUSH RSI ;Save RBP original value
	MOVSS DWORD PTR [RBP-52], XMM2 ;Save RatioX value
	MOVSS DWORD PTR [RBP-56], XMM3 ;Save RatioY value
	MOV RDI, QWORD PTR g ;Moving pointer to output bitmap to RDI
	MOV RSI, QWORD PTR h ;Moving pointer to input bitmap to RSI
	ADD RDI, 54 ;Moving RDI by header size
BitmapLoop:
	MOV EAX, DWORD PTR b ;Start variable is being moved to EAX
	CMP EAX, DWORD PTR c ;Compare Start with End
	JGE EndLoop
	;Calculate K value
	SUB EAX, 54 ;Subtract 54 (header length) from iterator
	MOV ECX, 3 ;Denominator
	XOR EDX, EDX
	DIV ECX
	MOV DWORD PTR [RBP-8], EAX ;Save K value
	;-----
	;Calculate X value
	MOV ECX, DWORD PTR f ;Denominator (WidthOut)
	DIV ECX
	CVTSI2SS XMM0, EDX ;Singed doubleword integer to single-precision floating-point and get modulo
	MOVSS XMM1, DWORD PTR [RBP-52]
	MULPS XMM0, XMM1 ;Multiply by ratio X
	;--Some kind of rounding--
	PINSRD XMM1, [_half], 0 ;move doubleword 
	ADDPS XMM0, XMM1
	CVTTSS2SI EAX, XMM0 ;Oposite of CVTSI2SS
	XOR EDX, EDX
    MOV DWORD PTR [RBP-12], EAX ;Save calculated X value
	;-----
	;Calculate Y value
	MOV EAX, DWORD PTR [RBP-8] ;Get K value
	MOV ECX, DWORD PTR f ;Denominator (WidthOut)
	DIV ECX
	CVTSI2SS XMM0, EAX ;Singed doubleword integer to single-precision floating-point (not modulo)
	MOVSS XMM1, DWORD PTR [RBP-56]
	MULPS XMM0, XMM1  ;Multiply by ratio Y
	;--Some kind of rounding--
	PINSRD XMM1, [_half], 0 ;move doubleword 
	ADDPS XMM0, XMM1
	CVTTSS2SI EAX, XMM0 ;Oposite of CVTSI2SS
	XOR EDX, EDX
    MOV DWORD PTR [RBP-16], EAX ;Save calculated Y value
	;-----
	;Calculate Target value
	MOV EAX, DWORD PTR e ;Get WidthIn
	MUL DWORD PTR [RBP-16] ;WidthIn * y
	ADD EAX, DWORD PTR [RBP-12] ;(WidthIn * y) + x
	MOV EDX, EAX ;Save EAX value so as not to multiply
	ADD EAX, EDX
	ADD EAX, EDX ;((WidthIn * y) + x) * 3
	ADD EAX, 54 ;Add Header length (54)
	MOV DWORD PTR [RBP-20], EAX ;Save calculated Target value
	;-----
	;Fill bitmapOut
	ADD RSI, RAX

	MOVZX EAX, BYTE PTR [RSI]
	MOV BYTE PTR [RDI], AL
	MOVZX EAX, BYTE PTR [RSI + 1]
	MOV BYTE PTR [RDI + 1], AL
	MOVZX EAX, BYTE PTR [RSI + 2]
	MOV BYTE PTR [RDI + 2], AL

	ADD RDI, 3
	MOV EAX, DWORD PTR [RBP-20]
	SUB RSI, RAX
	;-----
	ADD DWORD PTR b, 3 ;Iterator change
	JMP BitmapLoop
EndLoop:
	POP RSI
	RET
ScaleBitmap ENDP

END