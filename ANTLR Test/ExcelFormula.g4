grammar ExcelFormula;

/*
 * Parser Rules
 */

excelExpr
	: '=' expr=exp
	;

exp
	: fun=fexp														#functionExp
	| val=value														#valueExp
	| '(' expr=exp ')'												#bracketExp
	| left=exp '*' right=exp										#multExp
	| left=exp '/' right=exp										#divExp
	| left=exp '%' right=exp										#modExp
	| left=exp '+' right=exp										#addExp
	| left=exp '-' right=exp										#subExp				
	| left=exp '<' right=exp										#smallerExp
	| left=exp '>' right=exp										#greaterExp
	| left=exp '<=' right=exp										#smallerEqExp
	| left=exp '>=' right=exp										#greaterEqExp
	| left=exp '==' right=exp										#equalExp
	| left=exp '!=' right=exp										#unequalExp
	| left=exp '&&' right=exp										#andExp
	| left=exp '||' right=exp										#orExp
	| '-' param=exp													#negExp
	| '!' param=exp													#notExp
	| cell=celladress												#cellExp
	;

fexp
	: 'IF' threeArg													#ifFunc
	| 'ISBLANK' oneArg												#isblankFunc
	| 'SUM' anyArg													#sumFunc
	| 'PROD' anyArg													#prodFunc
	| 'AVERAGE' anyArg												#averageFunc
	| 'MAX' anyArg													#maxFunc
	| 'MIN' anyArg													#minFunc
	;

oneArg
	: '(' exp ')'
	;

twoArg
	: '(' exp1=exp KOMMA exp2=exp ')'
	;

threeArg
	: '(' exp1=exp KOMMA exp2=exp KOMMA exp3=exp ')'
	;

anyArg
	: '(' expr+=exp (KOMMA expr+=exp)* ')'									#anyArgBase
	| '(' left=celladress ':' right=celladress ')'							#anyArgSeq
	;

celladress
	: CELLADRESS															#baseAdress
	;

value
	: 'True'														#trueVal
	| 'False'														#falseVal
	| CHAR															#charVal
	| INT															#intVal
	| DECIMAL														#decimalVal
	| EMPTY															#emptyVal
	| STRING														#stringVal
	| DOLLARS														#dollarsVal
	| EUROS															#eurosVal
	| DATE															#dateVal
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

KOMMA
	:	[,;]
	;

EMPTY
	: '\\'
	;

INT     
	: NONFRACTPOSNUMBER
	;

CELLADRESS
	: [$]? [A-Z]+ [$]? NONFRACTPOSNUMBER
	;

CHAR
	: '\''[a-zA-Z]'\''
	;


DECIMAL     
	: [0-9]+ [.] [0-9]+
	;

EUROS
	: NUMBER '€'
	;

DOLLARS
	: NUMBER '$'
	;

STRING
    : '"' ( EscapeSequence | ~('\\'|'"') )* '"'
    ;

DATE
	: CALENDARDATE 'T' DATEDAYTIME
	;

CALENDARDATE
	: NONFRACTPOSNUMBER '-' MONTHNUMBER '-' DAYNUMBER
	;

DATEDAYTIME
	: TIMENUMBER ':' TIMENUMBER ':' TIMENUMBER
	;

fragment EscapeSequence
    : '\\' [abfnrtvz"'\\]
    | '\\' '\r'? '\n'
	;

fragment NUMBER
	: POSNUMBER
	;

fragment POSNUMBER
	: NONFRACTPOSNUMBER ([.] [0-9]+)?
	;

fragment NONFRACTPOSNUMBER
	: [0-9]+
	;

fragment MONTHNUMBER
	: [01]?[0-9]
	;

fragment DAYNUMBER
	: [0123]?[0-9]
	;

fragment TIMENUMBER
	: [0-6]?[0-9]
	;