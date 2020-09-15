grammar Spreadsheet;

/*
 * Parser Rules
 */

spreadSheet
	:	statements=stm+
	;

stm
	: ';'															#emptyStm
	| 'C[' left=exp '|' right=exp ']' '=' content=exp ';'			#cellStm
	| 'C[' left=exp '|' right=exp ']' '"' '=' content=exp '"' ';'	#cellEqStm
	;


exp
	: val=value														#valueExp
	| 'C[' left=aexp ',' right=aexp ']'								#cellExp
	| left=exp '*' right=exp										#multExp
	| left=exp '/' right=exp										#divExp
	| left=exp '%' right=exp										#modExp
	| left=exp '+' right=exp										#addExp
	| left=exp '-' right=exp										#subExp				
	| left=exp '<<' right=exp										#bitShiftDownExp
	| left=exp '>>' right=exp										#bitShiftUpExp
	| left=exp '<' right=exp										#smallerExp
	| left=exp '>' right=exp										#greaterExp
	| left=exp '<=' right=exp										#smallerEqExp
	| left=exp '>=' right=exp										#greaterEqExp
	| left=exp '==' right=exp										#equalExp
	| left=exp '!=' right=exp										#unequalExp
	| left=exp '&&' right=exp										#andExp
	| left=exp '||' right=exp										#orExp
	| '!' param=exp													#notExp
	;

value
	: 'true'														#trueVal
	| 'false'														#falseVal
	| CHAR															#charVal
	| INT															#intVal
	| DECIMAL														#decimalVal
	| EMPTY															#emptyVal
	| STRING														#stringVal
	;

aexp
	: '+' param=exp													#posAExp
	| '-' param=exp													#negAExp
	| param=exp														#baseAExp
	;


/*
 * Lexer Rules
 */

WS
	:	' ' -> skip
	;

END
	:	EOF -> skip
	;

NL
	:	[\n\r] -> skip
	;

COMMENT
    :   ( '//' ~[\r\n]* '\r'? '\n'
        | '/*' .*? '*/'
        ) -> skip
    ;

EMPTY
	: '-/-'
	;

CHAR
	: [a-zA-Z]
	;

NAME
    : [a-zA-Z_][a-zA-Z_0-9]*
    ;

INT     
	: [0-9]+
	;

DECIMAL     
	: [0-9]+ ([.,] [0-9]+)?
	;

STRING
    : '"' ( EscapeSequence | ~('\\'|'"') )* '"'
    ;

fragment EscapeSequence
    : '\\' [abfnrtvz"'\\]
    | '\\' '\r'? '\n'
	;

fragment NUMBER
	: [1-9][0-9]*
	;
