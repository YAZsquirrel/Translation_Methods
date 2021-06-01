.386
.MODEL FLAT

.DATA
; Переменные из С++
a DD ?
b DD ?
c DB ?
; Константы из С++
Const0 DB "1", 0
Const1 DD 10

.CODE
MAIN PROC
FINIT

; Выражение 0
; Начало выражения
FILD Const0; ST(0)
FISTP c
; Конец выражения


; Выражение 1
; Начало выражения
FILD Const0; ST(0)
FILD Const1; ST(0)ST(1)
; Разность
FSUB ; ST(0)
; ST(0)
FILD c; ST(0)ST(1)
FCOMPP
FSTSW AX
SAHF
JNE TRUE_0
FLDZ; ST(0)
JMP END_0
TRUE_0:
FLD1; ST(0)
END_0:
; ST(0)
FISTP b
; Конец выражения


; Выражение 2
; Начало выражения
FILD Const0; ST(0)
FILD Const1; ST(0)ST(1)
; Сумма
FADD ; ST(0)
; ST(0)
FILD c; ST(0)ST(1)
FCOMPP
FSTSW AX
SAHF
JE TRUE_1
FLDZ; ST(0)
JMP END_1
TRUE_1:
FLD1; ST(0)
END_1:
; ST(0)
FISTP b
; Конец выражения

END MAIN
